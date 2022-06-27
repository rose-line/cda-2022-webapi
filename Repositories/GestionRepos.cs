using Contrats;
using Entites;

namespace Repositories
{
  public class GestionRepos : IGestionRepos
  {
    private RepoContext _context;
    private IEntrepriseRepo _entrepriseRepo;
    private IEmployeRepo _employeRepo;

    public GestionRepos(RepoContext context)
    {
      _context = context;
    }

    public IEntrepriseRepo Entreprises
    {
      get
      {
        if (_entrepriseRepo == null)
        {
          _entrepriseRepo = new EntrepriseRepo(_context);
        }
        return _entrepriseRepo;
      }
    }

    public IEmployeRepo Employes
    {
      get
      {
        if (_employeRepo == null)
        {
          _employeRepo = new EmployeRepo(_context);
        }
        return _employeRepo;
      }
    }

    public void Save() => _context.SaveChanges();

  }
}
