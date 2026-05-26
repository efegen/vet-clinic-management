using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetClinic.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicableSpeciesToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicableSpecies",
                table: "Services",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicableSpecies",
                table: "Services");
        }
    }
}
