using Microsoft.Extensions.Logging;

namespace PowerPositions.Tests;

internal class TestLogger<T> : ILogger<T>
{
    public readonly List<LogEntry> Entries = new();

    public IDisposable BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        Entries.Add(new LogEntry(logLevel, eventId, message, exception));
    }

    public record LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}