using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenFamilyToRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AlterColumn<int>(
                name: "UserType",
                table: "Users",
                type: "integer",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                table: "RefreshTokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            // ✅ ADD TokenFamily column
            migrationBuilder.AddColumn<string>(
                name: "TokenFamily",
                table: "RefreshTokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            // ✅ ADD TokenFamily index
            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenFamily",
                table: "RefreshTokens",
                column: "TokenFamily");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ DROP TokenFamily index
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_TokenFamily",
                table: "RefreshTokens");

            // ✅ DROP TokenFamily column
            migrationBuilder.DropColumn(
                name: "TokenFamily",
                table: "RefreshTokens");

            migrationBuilder.AlterColumn<string>(
                name: "UserType",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

        }
    }
}