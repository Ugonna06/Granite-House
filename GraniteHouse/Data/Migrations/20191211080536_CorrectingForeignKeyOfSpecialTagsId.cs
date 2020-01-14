using Microsoft.EntityFrameworkCore.Migrations;

namespace GraniteHouse.Data.Migrations
{
    public partial class CorrectingForeignKeyOfSpecialTagsId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_SpecialTags_ProductTypesId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SpecialTagsId",
                table: "Products",
                column: "SpecialTagsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SpecialTags_SpecialTagsId",
                table: "Products",
                column: "SpecialTagsId",
                principalTable: "SpecialTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_SpecialTags_SpecialTagsId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SpecialTagsId",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SpecialTags_ProductTypesId",
                table: "Products",
                column: "ProductTypesId",
                principalTable: "SpecialTags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
