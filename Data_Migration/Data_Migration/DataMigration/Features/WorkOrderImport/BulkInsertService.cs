using Data_Migration.Data;

namespace Data_Migration.DataMigration.Features.WorkOrderImport
{
    public class BulkInsertService : IBulkInsertService
    {
        private readonly Data_Migration_Dbcontext _context;
        private readonly ILogger<BulkInsertService> _logger;

        public BulkInsertService(Data_Migration_Dbcontext context, ILogger<BulkInsertService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task BulkInsertAsync<T>(List<T> items, string tableName) where T : class
        {
            if (!items.Any()) return;

            var sw = Stopwatch.StartNew();
            var connectionString = _context.Database.GetConnectionString();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = tableName,
                BatchSize = 10000,
                BulkCopyTimeout = 600,
                EnableStreaming = true
            };

            // Map properties to columns
            var dataTable = ConvertToDataTable(items);

            foreach (DataColumn column in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }

            try
            {
                await bulkCopy.WriteToServerAsync(dataTable);
                sw.Stop();

                _logger.LogInformation("Bulk insert completed: {Count:N0} rows to {Table} in {Duration}s ({Rate:N0} rows/sec)",
                    items.Count, tableName, sw.Elapsed.TotalSeconds, items.Count / sw.Elapsed.TotalSeconds
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk insert failed for table {Table}", tableName);
                throw;
            }
        }
        private DataTable ConvertToDataTable<T>(List<T> items) where T : class
        {
            var dataTable = new DataTable();
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanRead && IsSimpleType(p.PropertyType))
                .ToList();

            // Add columns
            foreach (var prop in properties)
            {
                var columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, columnType);
            }

            // Add rows
            foreach (var item in items)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    row[prop.Name] = value ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private bool IsSimpleType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType.IsPrimitive
                || underlyingType == typeof(string)
                || underlyingType == typeof(decimal)
                || underlyingType == typeof(DateTime)
                || underlyingType == typeof(DateTimeOffset)
                || underlyingType == typeof(TimeSpan)
                || underlyingType == typeof(Guid);
        }
    }
}
