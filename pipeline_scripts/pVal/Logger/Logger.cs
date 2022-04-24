using System;
using System.Collections.Generic;
using System.Text;


// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
// INTEL CONFIDENTIAL
// Copyright (2019) (2020) Intel Corporation
//
// The source code contained or described herein and all documents related to the source code ("Material") are
// owned by Intel Corporation or its suppliers or licensors. Title to the Material remains with Intel Corporation
// or its suppliers and licensors. The Material contains trade secrets and proprietary and confidential
// information of Intel Corporation or its suppliers and licensors. The Material is protected by worldwide copyright
// and trade secret laws and treaty provisions. No part of the Material may be used, copied, reproduced, modified,
// published, uploaded, posted, transmitted, distributed, or disclosed in any way without Intel Corporation's prior express
// written permission.
//
// No license under any patent, copyright, trade secret or other intellectual property right is granted to or
// conferred upon you by disclosure or delivery of the Materials, either expressly, by implication, inducement,
// estoppel or otherwise. Any license under such intellectual property rights must be express and approved by
// Intel in writing.
// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Logger
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// This class provides extended logging capabilities.
    /// </summary>
    public static class Logger
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

        /// <summary>
        /// String representing name of log file to create, formatted with date of execution.
        /// </summary>
        private static readonly string LogFileName = string.Format("pVal-{0:yyyy-MM-dd_HH-mm-ss}.log", DateTime.Now);

        /// <summary>
        /// Value representing whether to print all messages to console.
        /// </summary>
        public static bool IsFullConsoleRequired = true;

        /// <summary>
        /// Print line in console or logger.
        /// </summary>
        /// <param name="text">Text to print.</param>
        /// <param name="type">Define where to print and define the color.</param>
        public static void PrintLine(string text, PrintType type = PrintType.CONSOLE_ONLY)
        {
            var prefix = string.Empty;
            var posfix = string.Empty;
            Console.ForegroundColor = ConsoleColor.White;
            switch (type)
            {
                case PrintType.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR:");
                    WriteLine("ERROR:");
                    prefix = "\t";
                    posfix = "\n";
                    Console.ForegroundColor = ConsoleColor.White;
                    goto case PrintType.DEFAULT;

                case PrintType.REDLINE_ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    goto case PrintType.DEFAULT;

                case PrintType.WARNING_LOGGER:

                    if (IsFullConsoleRequired)
                    {
                        goto case PrintType.WARNING;
                    }

                    WriteLine("WARNING:");
                    prefix = "\t";
                    posfix = "\n";
                    WriteLine(string.Concat(prefix, text, posfix));
                    break;

                case PrintType.WARNING:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("WARNING:");
                    WriteLine("WARNING:");
                    prefix = "\t";
                    posfix = "\n";
                    Console.ForegroundColor = ConsoleColor.White;
                    goto case PrintType.DEFAULT;

                case PrintType.ERROR_CONSOLE:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    goto case PrintType.CONSOLE_ONLY;

                case PrintType.CYAN_CONSOLE:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    goto case PrintType.CONSOLE_ONLY;

                case PrintType.GREEN_CONSOLE:
                    Console.ForegroundColor = ConsoleColor.Green;
                    goto case PrintType.CONSOLE_ONLY;

                case PrintType.DEBUG:
                    if (IsFullConsoleRequired)
                    {
                        goto case PrintType.DEFAULT;
                    }

                    break;

                case PrintType.DEFAULT:
                    Console.WriteLine(string.Concat(prefix, text, posfix));
                    WriteLine(string.Concat(prefix, text, posfix));
                    break;

                case PrintType.CONSOLE_ONLY:
                    Console.WriteLine(string.Concat(prefix, text, posfix));
                    break;

                case PrintType.LOGGER_ONLY:
                    WriteLine(string.Concat(prefix, text, posfix));
                    break;

                case PrintType.LOGGER_SEPARATOR:
                    WriteSeparator();
                    WriteLine(string.Concat(prefix, text, posfix));
                    WriteSeparator();
                    break;
            }
        }

        /// <summary>
        /// This method initializes or opens or start streaming to log file.
        /// </summary>
        /// <param name="logFileBasePath">Path of the file to which the .exe writes log info.</param>
        public static void InitializeLogFile(string logFileBasePath)
        {
            foreach (var pValLog in Directory.EnumerateFiles(logFileBasePath, "pVal-*.log"))
            {
                File.Delete(Path.Combine(logFileBasePath, pValLog));
            }

            File.Delete(Path.Combine(logFileBasePath, "Tosoutput.log"));

            Trace.Listeners.Clear();
            Directory.CreateDirectory(logFileBasePath);
            var logFilePath = Path.Combine(logFileBasePath, LogFileName);
            TextWriterTraceListener text = new TextWriterTraceListener(logFilePath);
            text.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ThreadId;
            Trace.Listeners.Add(text);
            Trace.AutoFlush = true;

            WriteLine("------------ PrimeVal.exe - {0} ---------------", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            WriteSeparator();
        }

        /// <summary>
        /// Writes to log file only the message no format is supported.
        /// </summary>
        /// <param name="message"> Message to be written to log file.</param>
        private static void WriteLine(string message)
        {
            Trace.WriteLine(string.Format(@"{0}  {1}", DateTime.Now.ToString("HH:mm:ss.fff"), message));
        }

        /// <summary>
        /// Writes lines to log file based on the format and arguments provided.
        /// </summary>
        /// <param name="format">Gives the format to be printed.</param>
        /// <param name="args">Array of arguments to be printed to log file</param>
        private static void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        /// <summary>
        /// Writes desired format or symbols to log file.
        /// </summary>
        private static void WriteSeparator()
        {
            WriteLine("##############################################################################################");
        }
    }
}