using System;
using System.Collections.Generic;
using System.IO;

namespace SmartLogAnalyzer.Infrastructure.Services
{
    public static class EnvLoader
    {
        private static bool _loaded = false;
        private static readonly Dictionary<string, string> _variables = new();

        public static void Load(string? envFilePath = null)
        {
            if (_loaded) return;

            string? path = envFilePath ?? FindEnvFile();

            if (path != null && File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    var separatorIndex = trimmed.IndexOf('=');
                    if (separatorIndex > 0)
                    {
                        var key = trimmed.Substring(0, separatorIndex).Trim();
                        var value = trimmed.Substring(separatorIndex + 1).Trim();
                        if (value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
                            value = value.Substring(1, value.Length - 2);
                        _variables[key] = value;
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
            }

            _loaded = true;
        }

        public static string? Get(string key)
        {
            if (_variables.TryGetValue(key, out var value))
                return value;
            return Environment.GetEnvironmentVariable(key);
        }

        public static string Get(string key, string defaultValue)
        {
            return Get(key) ?? defaultValue;
        }

        private static string? FindEnvFile()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                var envPath = Path.Combine(dir.FullName, ".env");
                if (File.Exists(envPath))
                    return envPath;
                dir = dir.Parent;
            }
            return null;
        }
    }
}