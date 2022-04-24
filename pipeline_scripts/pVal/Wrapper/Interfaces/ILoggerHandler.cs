namespace Wrapper
{
    public enum PrintType
    {
        GREEN_CONSOLE,
        CYAN_CONSOLE,
        DEFAULT,
        ERROR_CONSOLE,
        ERROR,
        REDLINE_ERROR,
        CONSOLE_ONLY,
        LOGGER_ONLY,
        DEBUG,
        LOGGER_SEPARATOR,
        WARNING,
        WARNING_LOGGER
    }

    public interface ILoggerHandler
    {
        public void InitializeLogFile(string logFileBasePath);

        public void PrintLine(string text, PrintType type = PrintType.CONSOLE_ONLY);
    }
}
