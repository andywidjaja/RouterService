using System;

namespace PTrust.Services.ShapeManagerRouter
{
    public interface IPtLogger
    {
        void LogDebug(string logText);

        void LogError(string logText);

        void LogError(string logText, Exception exception);

        void LogInfo(string logText);

        void LogWarn(string logText);
    }
}