namespace Data_Migration.DataMigration.Exceptions
{
    public class ExcelReadException : MigrationException
    {
        public ExcelReadException(string message, int rowNumber = 0) : base(message, rowNumber) { }
    }
}
