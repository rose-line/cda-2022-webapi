using Contrats;
using Entites;
using Entites.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
  public class EntrepriseRepo : RepositoryBase<Entreprise>, IEntrepriseRepo
  {
    public EntrepriseRepo(RepoContext context) : base(context)
    {
    }

    public IEnumerable<Entreprise> GetEntreprises(bool tracked)
    {
      return FindAll(tracked).OrderBy(e => e.Nom).ToList();
    }

    public Entreprise GetEntreprise(Guid id, bool tracked)
    {
      return FindByCondition(e => e.Id.Equals(id), tracked).SingleOrDefault();
    }

    public void Creer(Entreprise entreprise)
    {
      Create(entreprise);
    }
  }
}
