using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_Service.Data.Migrations
{
    /// <inheritdoc />
    public partial class update_to_erp_workorder_id_removed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                schema: "ERP",
                table: "workOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkOrderId",
                schema: "ERP",
                table: "workOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
