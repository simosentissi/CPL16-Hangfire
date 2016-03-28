﻿using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using Core.Jobs;
using Hangfire;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Web.Configuration
{
    public static class Bootstrap
    {
        public static void Start()
        {
            BootstrapNLog();
        }

        private static void BootstrapNLog()
        {
            var config = new LoggingConfiguration();

            var colorConsoleTarget = new ColoredConsoleTarget()
            {
                Name = "colorConsole"
            };

            config.AddTarget("colorConsole", colorConsoleTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, colorConsoleTarget));

            var databaseTarget = new DatabaseTarget()
            {
                ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString,
                CommandText = "INSERT INTO EventLog (Logger, [TimeStamp], Level, Message, Exception, StackTrace) " +
                              "VALUES (@logger, @timeStamp, @level, " +
                              "CASE WHEN LEN(@message) > 4000 THEN LEFT(@message, 3988) + '[truncated]' ELSE @message END," +
                              "CASE WHEN LEN(@exception) > 4000 THEN LEFT(@exception, 3988) + '[truncated]' ELSE @exception END," +
                              "CASE WHEN LEN(@stacktrace) > 4000 THEN LEFT(@stacktrace, 3988) + '[truncated]' ELSE @stacktrace END" +
                              ")",
                Parameters =
                {
                    new DatabaseParameterInfo("@logger", new SimpleLayout("${logger}")),
                    new DatabaseParameterInfo("@timestamp", new SimpleLayout("${date}")),
                    new DatabaseParameterInfo("@level", new SimpleLayout("${level}")),
                    new DatabaseParameterInfo("@message", new SimpleLayout("${message}")),
                    new DatabaseParameterInfo("@exception", new SimpleLayout("${exception}")),
                    new DatabaseParameterInfo("@stacktrace", new SimpleLayout("${exception:stacktrace}")),
                }
            };

            config.AddTarget("database", databaseTarget);

            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, databaseTarget));
            LogManager.Configuration = config;
        }
    }
}
