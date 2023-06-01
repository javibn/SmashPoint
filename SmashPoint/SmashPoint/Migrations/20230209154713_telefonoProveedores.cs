using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmashPoint.Migrations
{
    public partial class telefonoProveedores : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Proveedor",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Proveedor");
        }
    }
}
