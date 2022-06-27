namespace Contrats
{
  public interface ILoggable
  {
    void LogInfo(string message);

    void LogAvertissement(string message);

    void LogDebug(string message);

    void LogErreur(string message);
  }
}
