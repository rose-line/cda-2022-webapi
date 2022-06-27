using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entites.Models
{
  public class Employe
  {
    [Column("EmployeId")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Le nom de l'employé est requis.")]
    [MaxLength(30, ErrorMessage = "La taille maximale pour le nom est de 30 caractères.")]
    public string Nom { get; set; }

    [Required(ErrorMessage = "L'âge est requis.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Le poste est requis.")]
    [MaxLength(20, ErrorMessage = "La taille maximale pour le poste est de 20 caractères.")]
    public string Poste { get; set; }

    [ForeignKey(nameof(Entreprise))]
    public Guid EntrepriseId { get; set; }

    public Entreprise entreprise { get; set; }
  }
}
