namespace Data_Migration.DataMigration.Dtos
{
    public record ImportResultDto(
    int TotalRows,
    int SuccessfulRows,
    int FailedRows,
    TimeSpan Duration,
    List<string> Errors
);

    public record ExcelBatchDto<T>(
    List<T> Items,
    int BatchNumber,
    int StartRow,
    int EndRow
);
}
