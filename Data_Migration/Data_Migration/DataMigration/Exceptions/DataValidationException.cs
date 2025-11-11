namespace Data_Migration.DataMigration.Exceptions
{
    public class DataValidationException : MigrationException
    {
        public string FieldName { get; }

        public DataValidationException(string message, string fieldName, int rowNumber = 0)
            : base(message, rowNumber)
        {
            FieldName = fieldName;
        }
    }
}
