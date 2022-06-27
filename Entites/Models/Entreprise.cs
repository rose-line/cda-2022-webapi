using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entites.Models
{
  public class Entreprise
  {
    [Column("EntrepriseId")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Le nom de l'entreprise est requis.")]
    [MaxLength(50, ErrorMessage = "La taille maximale pour le nom est de 50 caractères.")]
    public string Nom { get; set; }

    [Required(ErrorMessage = "L'adresse est requise.")]
    [MaxLength(80, ErrorMessage = "La taille maximale pour l'adresse est de 80 caractères.")]
    public string Adresse { get; set; }

    public string Pays { get; set; }

    public ICollection<Employe> Employes { get; set; }
  }
}
