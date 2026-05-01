using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureTask.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddUserProfileImageBlobPath : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProfileImageBlobPath",
            table: "Users",
            type: "nvarchar(512)",
            maxLength: 512,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProfileImageBlobPath",
            table: "Users");
    }
}
