using System;
using Microsoft.Extensions.Logging;

namespace ServiceStack.Jwks.Tests {

    /// <summary>
    /// Somehow, using the ConsoleLogger from Microsoft.Extensions.Logging doesn't output logs
    /// </summary>
    public class TestLogger : ILogger, IDisposable {
        private readonly string name;

        public TestLogger() { }

        public TestLogger(string name) {
            this.name = name;
        }

        public void Dispose() { }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter) {
            Console.WriteLine($"{name}:{formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel)=> true;

        public IDisposable BeginScope<TState>(TState state)=> this;
    }

    public class TestLoggerProvider : ILoggerProvider {
        public ILogger CreateLogger(string categoryName)=> new TestLogger(categoryName);
        public void Dispose() { }
    }

    public static class TestLoggerExtensions {
        public static void AddTestLogger(this ILoggingBuilder logging)=> logging.AddProvider(new TestLoggerProvider());
    }
}