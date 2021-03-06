﻿using System;
using System.Threading;
using System.Windows;
using Serilog;
using Serilog.Core;

namespace PoeTradeHub.UI
{
    public static class Program
    {
        private const string ApplicationGuid = "E2706AEA-744F-401F-B868-33C423ABE531";

        [STAThread]
        public static void Main(string[] args)
        {
            using (var mutex = new Mutex(false, $@"Global\{ApplicationGuid}"))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Another instance of the application is already running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ILogger logger = ConfigureRootLogger();
                logger.Information("Starting application");

                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }

        private static ILogger ConfigureRootLogger()
        {
            Logger logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();

            Log.Logger = logger;
            return logger;
        }
    }
}
