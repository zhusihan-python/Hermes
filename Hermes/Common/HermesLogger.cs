using NLog;
using System;

namespace Hermes.Common;

public class HermesLogger : ILogger
{
    private const string LogFileName = "hermes.log";
    private const string Layout = "${longdate} | ${level:uppercase=true} | ${message} | ${threadid}";
    private readonly Logger _logger;

    public HermesLogger()
    {
        var config = new NLog.Config.LoggingConfiguration();

        var logfile = new NLog.Targets.FileTarget("logfile") { FileName = LogFileName };
        var consoleTarget = new NLog.Targets.ConsoleTarget("logconsole");
        logfile.Layout = Layout;
        consoleTarget.Layout = Layout;

        config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
#if !DEBUG
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
#endif
        NLog.LogManager.Configuration = config;

        this._logger = NLog.LogManager.GetCurrentClassLogger();
    }

    public void Error(Exception ex, string message)
    {
        this._logger.Error(ex, message);
    }

    public void Error(string message)
    {
        this._logger.Error(message);
    }

    public void Debug(string message)
    {
        this._logger.Debug(message);
    }

    public void Debug(Exception ex, string message)
    {
        this._logger.Debug(ex, message);
    }

    public void Warn(string message)
    {
        this._logger.Warn(message);
    }

    public void Warn(Exception ex, string message)
    {
        this._logger.Warn(ex, message);
    }

    public void Info(string message)
    {
        this._logger.Info(message);
    }
}