using Contrats;
using Entites;
using Entites.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
  public class EmployeRepo : RepositoryBase<Employe>, IEmployeRepo
  {
    public EmployeRepo(RepoContext context) : base(context)
    {
    }

    public IEnumerable<Employe> GetEmployes(Guid entrepriseId, bool tracked)
    {
      return FindByCondition(emp => emp.EntrepriseId.Equals(entrepriseId), tracked).OrderBy(emp => emp.Nom);
    }

    public Employe GetEmployeDeLEntreprise(Guid entrepriseId, Guid employeId, bool tracked)
    {
      return FindByCondition(emp => emp.EntrepriseId.Equals(entrepriseId) && emp.Id.Equals(employeId), tracked).SingleOrDefault();
    }

    public void CreerPourEntreprise(Guid entrepriseId, Employe employe)
    {
      employe.EntrepriseId = entrepriseId;
      Create(employe);
    }
  }
}
