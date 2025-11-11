

using Data_Migration.DataMigration.Features.ExcelReader;
using Data_Migration.DataMigration.Features.WorkOrderImport;

namespace Data_Migration.DataMigration.Features.ClientImport
{
    public class ClientImportService: IClientImportService
    {
        private readonly Data_Migration_Dbcontext _context;
        private readonly IExcelReaderService _excelReader;
        private readonly IBulkInsertService _bulkInsert;
        private readonly ILogger<ClientImportService> _logger;


        public ClientImportService(Data_Migration_Dbcontext context, IExcelReaderService excelReader, IBulkInsertService bulkInsert, ILogger<ClientImportService> logger)
        {
            _context = context;
            _excelReader = excelReader;
            _bulkInsert = bulkInsert;
            _logger = logger;
        }

        public async Task<ImportResultDto> ImportClientsAsync(string filePath)
        {
            var sw = Stopwatch.StartNew();
            var errors = new List<string>();
            var successCount = 0;

            _logger.LogInformation("Starting client import from: {FilePath}", filePath);

            try
            {
                // Read in parallel batches
                var batches = await _excelReader.ReadInParallelBatchesAsync(filePath,MapClientRow,batchSize: 50000 );

                var allClients = batches.SelectMany(b => b.Items).ToList();
                var totalRows = allClients.Count;

                _logger.LogInformation("Processing {Count:N0} clients", totalRows);

                // Remove duplicates based on FirstName + LastName
                var uniqueClients = allClients
                    .GroupBy(c => new { c.FirstName, c.LastName })
                    .Select(g => g.First())
                    .ToList();

                if (uniqueClients.Count < allClients.Count)
                {
                    _logger.LogWarning("Removed {Count} duplicate clients",
                        allClients.Count - uniqueClients.Count);
                }

                // Bulk insert
                await _bulkInsert.BulkInsertAsync(uniqueClients, "Clients");

                successCount = uniqueClients.Count;
                sw.Stop();

                _logger.LogInformation("Client import completed: {Success:N0} clients in {Duration}s",
                    successCount, sw.Elapsed.TotalSeconds);

                return
                    new ImportResultDto(TotalRows: totalRows,SuccessfulRows: successCount,FailedRows: totalRows - successCount,Duration: sw.Elapsed,Errors: errors
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Client import failed");
                throw;
            }
        }

        private ClientDto MapClientRow(ExcelWorksheet worksheet, int row)
        {
            // As the clients excel contain only one column, we continue to assume on the same way
            var clientData = worksheet.Cells[row, 1].Text.Trim();

            if (string.IsNullOrWhiteSpace(clientData))
            {
                throw new ArgumentException($"Invalid client data at row {row}: Client column is empty");
            }

            //Split the full name 
            var parts = clientData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new ArgumentException($"Invalid client data at row {row}: Expected 'FirstName LastName' format");
            }

            var firstName = parts[0];
            var lastName = parts[parts.Length - 1];

            return new ClientDto(firstName, lastName);
        }

    }
}
