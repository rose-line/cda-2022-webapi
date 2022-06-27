using Entites.Models;
using System;
using System.Collections.Generic;

namespace Contrats
{
  public interface IEmployeRepo
  {
    IEnumerable<Employe> GetEmployes(Guid id, bool tracked);

    Employe GetEmployeDeLEntreprise(Guid entrepriseId, Guid employeId, bool tracked);

    void CreerPourEntreprise(Guid entrepriseId, Employe employe);

  }
}
