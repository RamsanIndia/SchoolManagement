using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRoleRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Use SQL to safely drop constraint only if it exists
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.table_constraints 
                        WHERE constraint_name = 'FK_UserRoles_Users_UserId1' 
                        AND table_name = 'UserRoles'
                    ) THEN
                        ALTER TABLE ""UserRoles"" DROP CONSTRAINT ""FK_UserRoles_Users_UserId1"";
                    END IF;
                END $$;
            ");

            // ✅ Safely drop index only if it exists
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_UserRoles_ExpiresAt"";
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_UserRoles_UserId_RoleId_IsActive"";
            ");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_UserRoles_UserId1"";
            ");

            // ✅ Safely drop column only if it exists
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'UserRoles' 
                        AND column_name = 'UserId1'
                    ) THEN
                        ALTER TABLE ""UserRoles"" DROP COLUMN ""UserId1"";
                    END IF;
                END $$;
            ");

            // Create new indexes
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ExpiresAt",
                table: "UserRoles",
                column: "ExpiresAt",
                filter: "\"ExpiresAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_IsActive",
                table: "UserRoles",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_Unique",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes (safe to use normal drop in Down since Up created them)
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_ExpiresAt",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_IsActive",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId_Unique",
                table: "UserRoles");

            // Add back the old column if needed
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserRoles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Recreate old indexes
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_ExpiresAt",
                table: "UserRoles",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_IsActive",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId1",
                table: "UserRoles",
                column: "UserId1");

            // Add back the foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId1",
                table: "UserRoles",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}