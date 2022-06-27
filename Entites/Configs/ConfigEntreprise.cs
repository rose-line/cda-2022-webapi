using Entites.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Entites.Configs
{
  public class ConfigEntreprise : IEntityTypeConfiguration<Entreprise>
  {
    public void Configure(EntityTypeBuilder<Entreprise> builder)
    {
      builder.HasData
      (
        new Entreprise
        {
          Id = new Guid("3b79c0d9-e8ed-4ca0-a3e0-d9cb92998866"),
          Nom = "Entreprise01 SA",
          Adresse = "Adresse01 chemin du 01",
          Pays = "FR"
        },
        new Entreprise
        {
          Id = new Guid("378431c9-4b7c-4fb2-9263-abcac4167c09"),
          Nom = "Entreprise02 SARL",
          Adresse = "Adresse02 rue du 02",
          Pays = "BE"
        }
      );
    }
  }
}
