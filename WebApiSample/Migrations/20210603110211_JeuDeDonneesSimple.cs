using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApiSample.Migrations
{
    public partial class JeuDeDonneesSimple : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Entreprises",
                columns: new[] { "EntrepriseId", "Adresse", "Nom", "Pays" },
                values: new object[] { new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"), "Adresse01 chemin du 01", "Entreprise01 SA", "FR" });

            migrationBuilder.InsertData(
                table: "Entreprises",
                columns: new[] { "EntrepriseId", "Adresse", "Nom", "Pays" },
                values: new object[] { new Guid("378431c9-4b7c-4fb2-9263-abcac4167c09"), "Adresse02 rue du 02", "Entreprise02 SARL", "BE" });

            migrationBuilder.InsertData(
                table: "Employes",
                columns: new[] { "EmployeId", "Age", "EntrepriseId", "Nom", "Poste" },
                values: new object[] { new Guid("f4263866-da7b-4d34-8804-d2338575c320"), 18, new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"), "Employé 01", "Concepteur logiciel" });

            migrationBuilder.InsertData(
                table: "Employes",
                columns: new[] { "EmployeId", "Age", "EntrepriseId", "Nom", "Poste" },
                values: new object[] { new Guid("c2efc608-3741-4acf-89d4-f82f0211c31f"), 20, new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"), "Employé 02", "Architecte logiciel" });

            migrationBuilder.InsertData(
                table: "Employes",
                columns: new[] { "EmployeId", "Age", "EntrepriseId", "Nom", "Poste" },
                values: new object[] { new Guid("80ec219c-5a54-4670-96ad-92969a997454"), 19, new Guid("378431c9-4b7c-4fb2-9263-abcac4167c09"), "Employe 03", "Admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employes",
                keyColumn: "EmployeId",
                keyValue: new Guid("80ec219c-5a54-4670-96ad-92969a997454"));

            migrationBuilder.DeleteData(
                table: "Employes",
                keyColumn: "EmployeId",
                keyValue: new Guid("c2efc608-3741-4acf-89d4-f82f0211c31f"));

            migrationBuilder.DeleteData(
                table: "Employes",
                keyColumn: "EmployeId",
                keyValue: new Guid("f4263866-da7b-4d34-8804-d2338575c320"));

            migrationBuilder.DeleteData(
                table: "Entreprises",
                keyColumn: "EntrepriseId",
                keyValue: new Guid("378431c9-4b7c-4fb2-9263-abcac4167c09"));

            migrationBuilder.DeleteData(
                table: "Entreprises",
                keyColumn: "EntrepriseId",
                keyValue: new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"));
        }
    }
}
