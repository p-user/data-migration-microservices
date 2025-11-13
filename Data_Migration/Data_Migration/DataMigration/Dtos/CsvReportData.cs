namespace Data_Migration.DataMigration.Dtos
{

        public record CsvReportData(DateTime ImportDate, string SourceFile,int TotalRows,int SuccessfulRows,int FailedRows,TimeSpan Duration,
        List<string> Errors,
        List<int> SuccessfulRowNumbers
    );
    
}
