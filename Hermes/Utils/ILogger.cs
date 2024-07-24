using System;

namespace Hermes.Utils;

public interface ILogger
{
    void Error(Exception ex, string message);
    void Error(string message);
    void Debug(string message);
    void Debug(Exception ex, string message);
    void Warn(string message);
    void Warn(Exception ex, string message);
    void Info(string message);
}