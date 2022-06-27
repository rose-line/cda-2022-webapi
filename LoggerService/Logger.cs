
using Contrats;
using NLog;

namespace ServiceLogging
{
  public class Logger : ILoggable
  {
    private static ILogger logger = LogManager.GetCurrentClassLogger();

    public void LogDebug(string message)
    {
      logger.Debug(message);
    }

    public void LogErreur(string message)
    {
      logger.Error(message);
    }

    public void LogInfo(string message)
    {
      logger.Info(message);
    }

    public void LogAvertissement(string message)
    {
      logger.Warn(message);
    }
  }
}
