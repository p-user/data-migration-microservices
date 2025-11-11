namespace Data_Migration.DataMigration.Exceptions
{
    public class ParsingException : MigrationException
    {
        public ParsingException(string message, int rowNumber = 0) : base(message, rowNumber) { }
    }
}
