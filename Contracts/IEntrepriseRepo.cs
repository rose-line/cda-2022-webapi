using Entites.Models;
using System;
using System.Collections.Generic;

namespace Contrats
{
  public interface IEntrepriseRepo
  {

    IEnumerable<Entreprise> GetEntreprises(bool tracked);

    Entreprise GetEntreprise(Guid id, bool tracked);

    void Creer(Entreprise entreprise);

  }
}
