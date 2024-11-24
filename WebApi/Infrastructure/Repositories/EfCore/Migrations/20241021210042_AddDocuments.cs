using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Repositories.EFCore.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class AddDocuments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Tests");

        migrationBuilder.CreateTable(
            name: "Document",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Path = table.Column<string>(type: "text", nullable: false),
                Size = table.Column<long>(type: "bigint", nullable: false),
                UploadTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                FileName = table.Column<string>(type: "text", nullable: false),
                Title = table.Column<string>(type: "text", nullable: false),
                Author = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Document", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Document");

        migrationBuilder.CreateTable(
            name: "Tests",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tests", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Tests",
            columns: new[] { "Id", "Name" },
            values: new object[,]
            {
                { 1, "Test" },
                { 2, "Test 2" }
            });
    }
}
