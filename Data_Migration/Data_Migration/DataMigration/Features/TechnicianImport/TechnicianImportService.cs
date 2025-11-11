
using Data_Migration.DataMigration.Features.ExcelReader;
using Data_Migration.DataMigration.Features.WorkOrderImport;

namespace Data_Migration.DataMigration.Features.TechnicianImport
{
    public class TechnicianImportService : ITechnicianImportService
    {
        private readonly Data_Migration_Dbcontext _context;
        private readonly IExcelReaderService _excelReader;
        private readonly IBulkInsertService _bulkInsert;
        private readonly ILogger<TechnicianImportService> _logger;

        public TechnicianImportService(Data_Migration_Dbcontext context, IExcelReaderService excelReader, IBulkInsertService bulkInsert, ILogger<TechnicianImportService> logger)
        {
            _context=context;
            _excelReader=excelReader;
            _bulkInsert=bulkInsert;
            _logger=logger;
        }

        public async Task<ImportResultDto> ImportTechniciansAsync(string filePath)
        {
            var sw = Stopwatch.StartNew();
            var errors = new List<string>();

            _logger.LogInformation("👨‍🔧 Starting technician import from: {FilePath}", filePath);

            try
            {
                // Extract unique technicians from work orders
                var batches = await _excelReader.ReadInParallelBatchesAsync(
                    filePath,
                    MapTechnicianRow,
                    batchSize: 50000
                );

                var allTechnicians = batches.SelectMany(b => b.Items).ToList();

                // Get unique technicians
                var uniqueTechnicians = allTechnicians
                    .GroupBy(t => new { t.FirstName, t.LastName })
                    .Select(g => g.First())
                    .ToList();

                _logger.LogInformation("Found {Count:N0} unique technicians", uniqueTechnicians.Count);

                // Bulk insert
                await _bulkInsert.BulkInsertAsync(uniqueTechnicians, "Technicians");

                sw.Stop();

                _logger.LogInformation("Technician import completed in {Duration}s",
                    sw.Elapsed.TotalSeconds);

                return new ImportResultDto(
                    TotalRows: allTechnicians.Count,
                    SuccessfulRows: uniqueTechnicians.Count,
                    FailedRows: 0,
                    Duration: sw.Elapsed,
                    Errors: errors
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Technician import failed");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetTechnicianLookupAsync()
        {
            var technicians = await _context.Technicians
                .AsNoTracking()
                .Select(t => new { t.Id, t.FirstName, t.LastName })
                .ToListAsync();

            return technicians.ToDictionary(t => $"{t.FirstName}|{t.LastName}",t => t.Id);
        }

        private TechnicianDto MapTechnicianRow(ExcelWorksheet worksheet, int row)
        {
            // Column 1: Technician name
            var technicianName = worksheet.Cells[row, 1].Text.Trim();
            var parts = technicianName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new ArgumentException($"Invalid technician name at row {row}");
            }

            return new TechnicianDto(parts[0], string.Join(" ", parts.Skip(1)));
        }
    }
}
