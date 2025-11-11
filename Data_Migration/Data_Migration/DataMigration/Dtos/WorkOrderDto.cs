namespace Data_Migration.DataMigration.Dtos
{

    public record WorkOrderRawDto(
        string TechnicianName,
        string Notes,
        decimal Total,
        int RowNumber
    );

    public record WorkOrderProcessedDto(
        string TechnicianFirstName,
        string TechnicianLastName,
        string ClientFirstName,
        string ClientLastName,
        DateTime ServiceDate,
        string Notes,
        decimal Total,
        int RowNumber
    );

}
