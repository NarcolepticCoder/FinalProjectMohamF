using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnOrTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "Type", "Value" },
                values: new object[,]
                {
                    { new Guid("1fbc51ad-2017-43f1-bca8-cfb12176852e"), "permissions", "Audit.ViewAuthEvents" },
                    { new Guid("c038251a-d6cf-45ff-b61e-b1e82d13362a"), "permissions", "Audit.RoleChanges" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("2aef3323-8359-448f-9063-da6e7b7af161"), "Can view auth events & role changes", "SecurityAuditor" },
                    { new Guid("5512081a-e9fc-4150-b83b-cc29a7dcf63a"), "Can view authentication events", "AuthObserver" },
                    { new Guid("b9d50a31-1d68-4988-8de2-436f0d791579"), "Default role for new users", "BasicUser" }
                });

            migrationBuilder.InsertData(
                table: "RoleClaims",
                columns: new[] { "ClaimId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("1fbc51ad-2017-43f1-bca8-cfb12176852e"), new Guid("2aef3323-8359-448f-9063-da6e7b7af161") },
                    { new Guid("c038251a-d6cf-45ff-b61e-b1e82d13362a"), new Guid("2aef3323-8359-448f-9063-da6e7b7af161") },
                    { new Guid("1fbc51ad-2017-43f1-bca8-cfb12176852e"), new Guid("5512081a-e9fc-4150-b83b-cc29a7dcf63a") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { new Guid("1fbc51ad-2017-43f1-bca8-cfb12176852e"), new Guid("2aef3323-8359-448f-9063-da6e7b7af161") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { new Guid("c038251a-d6cf-45ff-b61e-b1e82d13362a"), new Guid("2aef3323-8359-448f-9063-da6e7b7af161") });

            migrationBuilder.DeleteData(
                table: "RoleClaims",
                keyColumns: new[] { "ClaimId", "RoleId" },
                keyValues: new object[] { new Guid("1fbc51ad-2017-43f1-bca8-cfb12176852e"), new Guid("5512081a-e9fc-4150-b83b-cc29a7dcf63a") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b9d50a31-1d68-4988-8de2-436f0d791579"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("1fbc51ad-2017-43f1-bca8-cfb12176852e"));

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: new Guid("c038251a-d6cf-45ff-b61e-b1e82d13362a"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("2aef3323-8359-448f-9063-da6e7b7af161"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5512081a-e9fc-4150-b83b-cc29a7dcf63a"));
        }
    }
}
