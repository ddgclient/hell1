namespace Wrapper
{
    using System.Collections.Generic;
    using Logger;

    /// <summary>
    /// Wrapper for the Logger namespace; allows for redirection of inputs during mocking.
    /// </summary>
    class LoggerHandler : ILoggerHandler
    {
        private Dictionary<Wrapper.PrintType, Logger.PrintType> enumWrapper = new Dictionary<Wrapper.PrintType, Logger.PrintType>()
        {
            { Wrapper.PrintType.GREEN_CONSOLE      , Logger.PrintType.GREEN_CONSOLE   },
            { Wrapper.PrintType.CYAN_CONSOLE       , Logger.PrintType.CYAN_CONSOLE    },
            { Wrapper.PrintType.DEFAULT            , Logger.PrintType.DEFAULT         },
            { Wrapper.PrintType.ERROR_CONSOLE      , Logger.PrintType.ERROR_CONSOLE   },
            { Wrapper.PrintType.ERROR              , Logger.PrintType.ERROR           },
            { Wrapper.PrintType.REDLINE_ERROR      , Logger.PrintType.REDLINE_ERROR   },
            { Wrapper.PrintType.CONSOLE_ONLY       , Logger.PrintType.CONSOLE_ONLY    },
            { Wrapper.PrintType.LOGGER_ONLY        , Logger.PrintType.LOGGER_ONLY     },
            { Wrapper.PrintType.DEBUG              , Logger.PrintType.DEBUG           },
            { Wrapper.PrintType.LOGGER_SEPARATOR   , Logger.PrintType.LOGGER_SEPARATOR },
            { Wrapper.PrintType.WARNING            , Logger.PrintType.WARNING         },
            { Wrapper.PrintType.WARNING_LOGGER     , Logger.PrintType.WARNING_LOGGER  },
        }; 

        public void InitializeLogFile(string logFileBasePath)
        {
            Logger.InitializeLogFile(logFileBasePath);
        }

        public void PrintLine(string text, Wrapper.PrintType type = Wrapper.PrintType.CONSOLE_ONLY)
        {
            Logger.PrintLine(text, enumWrapper[type]);
        }
    }
}
