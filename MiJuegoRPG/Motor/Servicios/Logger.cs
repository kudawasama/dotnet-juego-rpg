using System;
using MiJuegoRPG.Interfaces;

namespace MiJuegoRPG.Motor.Servicios
{
    public enum LogLevel { Error = 0, Warn = 1, Info = 2, Debug = 3 }

    public static class Logger
    {
        public static bool Enabled { get; set; } = true;
        public static LogLevel Level { get; set; } = LogLevel.Info;

        private static Action<string>? _sink;

        static Logger()
        {
            _sink = s => Console.WriteLine(s);
        }

        public static void SetSink(IUserInterface ui)
        {
            _sink = s => ui.WriteLine(s);
        }

        public static void Debug(string message)
        {
            if (!Enabled || Level < LogLevel.Debug) return;
            _sink?.Invoke(message);
        }
        public static void Info(string message)
        {
            if (!Enabled || Level < LogLevel.Info) return;
            _sink?.Invoke(message);
        }
        public static void Warn(string message)
        {
            if (!Enabled || Level < LogLevel.Warn) return;
            _sink?.Invoke(message);
        }
        public static void Error(string message)
        {
            if (!Enabled || Level < LogLevel.Error) return;
            _sink?.Invoke(message);
        }
    }
}
