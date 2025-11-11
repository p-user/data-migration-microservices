

namespace Data_Migration.DataMigration.Features.ExcelReader
{
    public interface IExcelReaderService
    {
        Task<List<ExcelBatchDto<T>>> ReadInParallelBatchesAsync<T>(string filePath,Func<ExcelWorksheet, int, T> rowMapper,int batchSize = 50000) where T : class;

        Task<int> GetTotalRowCountAsync(string filePath);
    }
}
