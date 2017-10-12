
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Exercise.Data
{

    public class MyLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger();
        }

        public void Dispose()
        {
        }

        private class MyLogger : ILogger
        {

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
              Func<TState, Exception, string> formatter)
            {
                //Note, the DbCommandLogData class will be replaced at some point so be wary of using it
                if (true)//(state is DbCommand)
                {
                    Debug.WriteLine(formatter(state, exception));
                    Debug.WriteLine("");
                }
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}
