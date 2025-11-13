using Data_Migration.Data;
using Data_Migration.DataMigration.Dtos;
using Data_Migration.DataMigration.Features.CsvReport;
using Data_Migration.DataMigration.Features.ExcelReader;
using Data_Migration.DataMigration.Features.TechnicianImport;
using Data_Migration.DataMigration.Models;
using Microsoft.IdentityModel.Logging;
using Slugify;
using System.Threading;


namespace Data_Migration.DataMigration.Features.WorkOrderImport
{
    public class WorkOrderImportService : IWorkOrderImportService
    {
        private readonly Data_Migration_Dbcontext _context;
        private readonly IExcelReaderService _excelReader;
        private readonly INotesParserService _notesParser;
        private readonly IBulkInsertService _bulkInsert;
        private readonly ITechnicianImportService _technicianService;
        private readonly ILogger<WorkOrderImportService> _logger;
        private readonly ICsvReportService _csvReportService;
        private readonly SlugHelper _slugHelper = new SlugHelper(new SlugHelperConfiguration
        {
            ForceLowerCase = false,
            CollapseDashes = false,
            TrimWhitespace = true
        });


        public WorkOrderImportService(Data_Migration_Dbcontext context,IExcelReaderService excelReader,INotesParserService notesParser,IBulkInsertService bulkInsert,ITechnicianImportService technicianService,ILogger<WorkOrderImportService> logger, ICsvReportService csvReportService)
        {
            _context = context;
            _excelReader = excelReader;
            _notesParser = notesParser;
            _bulkInsert = bulkInsert;
            _technicianService = technicianService;
            _logger = logger;
            _csvReportService = csvReportService;
        }


        public async Task<ImportResultDto> ImportWorkOrdersAsync(string filePath)
        {
            var sw = Stopwatch.StartNew();
            var errors = new ConcurrentBag<string>();
            var successfulRowNumbers = new ConcurrentBag<int>();

            _logger.LogInformation("Starting work order import from: {FilePath}", filePath);

            try
            {
                // Read  data in parallel
                _logger.LogInformation("Reading Excel file in parallel...");
                var rawBatches = await _excelReader.ReadInParallelBatchesAsync(
                    filePath,
                    MapWorkOrderRawRow,
                    batchSize: 50000
                );

                var totalRows = rawBatches.Sum(b => b.Items.Count);
                _logger.LogInformation("Read {Count:N0} raw work orders", totalRows);


                // Parse/transform in parallel
                _logger.LogInformation("Parsing notes and transforming data...");
                var processedBatches = new ConcurrentBag<List<WorkOrderProcessedDto>>();

                await Parallel.ForEachAsync(rawBatches,  new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    async (batch, ct) =>
                    {
                        var processed = new List<WorkOrderProcessedDto>();
                        foreach (var raw in batch.Items)
                        {
                            try
                            {
                                var parsed = _notesParser.ParseNotes(raw);
                                processed.Add(parsed);
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Row {raw.RowNumber}: {ex.Message}");
                            }
                        }
                        processedBatches.Add(processed);
                        await Task.CompletedTask;
                    });

                var allProcessed = processedBatches.SelectMany(b => b).ToList();
                _logger.LogInformation("Parsed {Success:N0} work orders, {Failed} errors",
                    allProcessed.Count, errors.Count);

                // Build lookup dictionaries
                _logger.LogInformation("Building lookup dictionaries...");
                var techLookup = await _technicianService.GetTechnicianLookupAsync();
                var clientLookup = await GetClientLookupAsync();

                // Map to final entities
                _logger.LogInformation("Mapping to work order entities...");
                //var workOrders = allProcessed
                //    .Select(p => MapToWorkOrder(p, techLookup, clientLookup, errors))
                //    .Where(wo => wo != null)
                //    .ToList();

                var workOrders = allProcessed
                    .Select(p =>
                    {
                       

                        var wo = MapToWorkOrder(p, techLookup, clientLookup, errors);
                        if (wo != null)
                        {
                            successfulRowNumbers.Add(p.RowNumber);
                        }
                        return wo;
                    })
                    .Where(wo => wo != null)
                    .ToList();


                _logger.LogInformation("Mapped {Count:N0} valid work orders", workOrders.Count);

                // Bulk insert
                _logger.LogInformation("Performing bulk insert...");
                await _bulkInsert.BulkInsertAsync(workOrders, "WorkOrders");

                sw.Stop();

                _logger.LogInformation("Work order import completed in {Duration}s",sw.Elapsed.TotalSeconds);

                // Generate report
                var reportData = new CsvReportData(
                    ImportDate: DateTime.Now,
                    SourceFile: Path.GetFileName(filePath),
                    TotalRows: totalRows,
                    SuccessfulRows: workOrders.Count,
                    FailedRows: totalRows - workOrders.Count,
                    Duration: sw.Elapsed,
                    Errors: errors.ToList(),
                    SuccessfulRowNumbers: successfulRowNumbers.OrderBy(r => r).ToList()
                );

                var reportPath = await _csvReportService.GenerateImportReportAsync(reportData);

                //ToDo: Dispatch event 
                return new ImportResultDto(
                    TotalRows: totalRows,
                    SuccessfulRows: workOrders.Count,
                    FailedRows: totalRows - workOrders.Count,
                    Duration: sw.Elapsed,
                    Errors: errors.Take(100).ToList() // Limit error list
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Work order import failed");
                throw;
            }
        }

        private WorkOrderRawDto MapWorkOrderRawRow(ExcelWorksheet worksheet, int row)
        {
            return new WorkOrderRawDto(
                TechnicianName: worksheet.Cells[row, 1].Text.Trim(),
                Notes: worksheet.Cells[row, 2].Text.Trim(),
                Total: decimal.Parse(worksheet.Cells[row, 3].Text),
                RowNumber: row
            );
        }

        private WorkOrder? MapToWorkOrder(
            WorkOrderProcessedDto dto,
            Dictionary<string, int> techLookup,
            Dictionary<string, int> clientLookup,
            ConcurrentBag<string> errors)
        {
            try
            {
                var techKey = $"{dto.TechnicianFirstName}|{dto.TechnicianLastName}";
                var clientKey = $"{dto.ClientFirstName}|{dto.ClientLastName}";

                if (!techLookup.TryGetValue(techKey, out var techId))
                {
                    errors.Add($"Row {dto.RowNumber}: Technician not found - {techKey}");
                    return null;
                }

                if (!clientLookup.TryGetValue(clientKey, out var clientId))
                {
                    errors.Add($"Row {dto.RowNumber}: Client not found - {clientKey}");
                    return null;
                }

                return new WorkOrder
                {
                    TechnicianId = techId,
                    ClientId = clientId,
                    ServiceDate = dto.ServiceDate,
                    Notes = dto.Notes,
                    Total = dto.Total
                };
            }
            catch (Exception ex)
            {
                errors.Add($"Row {dto.RowNumber}: {ex.Message}");
                return null;
            }
        }

        private async Task<Dictionary<string, int>> GetClientLookupAsync(CancellationToken cancellationToken = default)
        {
            var clients = await _context.Clients
                .AsNoTracking()
                .Select(c => new { c.Id, c.FirstName, c.LastName })
                .ToListAsync(cancellationToken);

            return clients.ToDictionary(
                c => $"{_slugHelper.GenerateSlug(c.FirstName)}|{_slugHelper.GenerateSlug(c.LastName)}",
                c => c.Id
            );
        }

    }
}
