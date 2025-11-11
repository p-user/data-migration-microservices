namespace Data_Migration.DataMigration.Features.ExcelReader
{
    public class ParallelExcelReaderService : IExcelReaderService
    {
        private readonly ILogger<ParallelExcelReaderService> _logger;
        private readonly int _maxDegreeOfParallelism;

        public ParallelExcelReaderService(ILogger<ParallelExcelReaderService> logger)
        {
            _logger = logger;
            _maxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1);
        }

        public async Task<int> GetTotalRowCountAsync(string filePath)
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            return worksheet.Dimension?.Rows ?? 0;
        }

        public async Task<List<ExcelBatchDto<T>>> ReadInParallelBatchesAsync<T>(string filePath,Func<ExcelWorksheet, int, T> rowMapper,int batchSize = 50000) where T : class
        {
            //Start timing
            var sw = Stopwatch.StartNew();

            _logger.LogInformation("Starting parallel Excel reading: {FilePath}", filePath);

            var totalRows = await GetTotalRowCountAsync(filePath);
            _logger.LogInformation("Total rows detected: {TotalRows:N0}", totalRows);

            // Calculate batch ranges
            var batches = CreateBatchRanges(totalRows, batchSize);
            _logger.LogInformation("Created {BatchCount} batches of ~{BatchSize:N0} rows", batches.Count, batchSize);

            // Read file into memory all at once
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var results = new ConcurrentBag<ExcelBatchDto<T>>();

            // Process batches in parallel
            await Parallel.ForEachAsync(batches,new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism },
                async (batch, ct) =>
                {
                    try
                    {
                        var batchData = await Task.Run(() =>
                            ProcessBatch(fileBytes, batch, rowMapper), ct);

                        results.Add(batchData);

                        _logger.LogInformation("Batch {BatchNum} processed: rows {Start}-{End} ({Count:N0} items)",batch.BatchNumber, batch.StartRow, batch.EndRow, batchData.Items.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing batch {BatchNum}", batch.BatchNumber);
                        throw;
                    }
                });

            sw.Stop();
            var resultList = results.OrderBy(b => b.BatchNumber).ToList();

            _logger.LogInformation("Parallel reading completed in {Duration}s. Total items: {Count:N0}",sw.Elapsed.TotalSeconds, resultList.Sum(b => b.Items.Count));

            return resultList;
        }

        private ExcelBatchDto<T> ProcessBatch<T>(byte[] fileBytes,BatchRange range,Func<ExcelWorksheet, int, T> rowMapper) where T : class
        {
            using var stream = new MemoryStream(fileBytes);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            var items = new List<T>(range.EndRow - range.StartRow + 1);

            for (int row = range.StartRow; row <= range.EndRow; row++)
            {
                try
                {
                    var item = rowMapper(worksheet, row);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    // Log
                    Console.WriteLine($" Row {row} error: {ex.Message}");
                }

                // continue processing
            }

            return new ExcelBatchDto<T>(items, range.BatchNumber, range.StartRow, range.EndRow);
        }

        private List<BatchRange> CreateBatchRanges(int totalRows, int batchSize)
        {
            var ranges = new List<BatchRange>();
            int batchNumber = 1;

            // Skip header row 
            for (int start = 2; start <= totalRows; start += batchSize)
            {
                int end = Math.Min(start + batchSize - 1, totalRows);
                ranges.Add(new BatchRange(batchNumber++, start, end));
            }

            return ranges;
        }

        private record BatchRange(int BatchNumber, int StartRow, int EndRow);
    }
}

