namespace Contrats
{
  public interface IGestionRepos
  {
    IEntrepriseRepo Entreprises { get; }
    IEmployeRepo Employes { get; }
    void Save();
  }
}
