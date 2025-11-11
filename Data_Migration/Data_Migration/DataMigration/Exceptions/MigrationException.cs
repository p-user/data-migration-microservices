namespace Data_Migration.DataMigration.Exceptions
{
    public class MigrationException : Exception
    {
        public int RowNumber { get; }

        public MigrationException(string message, int rowNumber = 0) : base(message)
        {
            RowNumber = rowNumber;
        }

        public MigrationException(string message, Exception innerException, int rowNumber = 0)
            : base(message, innerException)
        {
            RowNumber = rowNumber;
        }
    }
}
