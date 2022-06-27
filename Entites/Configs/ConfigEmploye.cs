using Entites.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Entites.Configs
{
  public class ConfigEmploye : IEntityTypeConfiguration<Employe>
  {
    public void Configure(EntityTypeBuilder<Employe> builder)
    {
      builder.HasData
      (
      new Employe
      {
        Id = new Guid("f4263866-da7b-4d34-8804-d2338575c320"),
        Nom = "Employé 01",
        Age = 18,
        Poste = "Concepteur logiciel",
        EntrepriseId = new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866")
      },
      new Employe
      {
        Id = new Guid("c2efc608-3741-4acf-89d4-f82f0211c31f"),
        Nom = "Employé 02",
        Age = 20,
        Poste = "Architecte logiciel",
        EntrepriseId = new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866")
      },
      new Employe
      {
        Id = new Guid("80ec219c-5a54-4670-96ad-92969a997454"),
        Nom = "Employe 03",
        Age = 19,
        Poste = "Admin",
        EntrepriseId = new Guid("378431c9-4b7c-4fb2-9263-abcac4167c09")
      }
      );
    }
  }
}
