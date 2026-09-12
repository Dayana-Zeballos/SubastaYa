using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubastaYa.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndAuctionEconomics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Auctions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Auctions",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumIncrement",
                table: "Auctions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_CategoryId",
                table: "Auctions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_Status_EndsAt",
                table: "Auctions",
                columns: new[] { "Status", "EndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Categories_CategoryId",
                table: "Auctions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Categories_CategoryId",
                table: "Auctions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_CategoryId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_Status_EndsAt",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "MinimumIncrement",
                table: "Auctions");
        }
    }
}
