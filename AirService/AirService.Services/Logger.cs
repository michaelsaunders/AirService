using System;
using log4net;

namespace AirService.Services
{
    public class Logger
    {
        public static void Debug(string message, Exception exception)
        {
            foreach (var logger in LogManager.GetCurrentLoggers())
            {
                logger.Debug(message, exception);
            }
        }

        public static void Log(string message, Exception exception)
        {
            foreach (var logger in LogManager.GetCurrentLoggers())
            {
                logger.Error(message, exception);
            }
        }

        public static void Trace(string message)
        {
            foreach (var logger in LogManager.GetCurrentLoggers())
            {
                logger.Info(message);
            }
        }
    }
}