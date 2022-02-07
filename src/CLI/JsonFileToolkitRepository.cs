using System;
using System.IO;

namespace automate
{
    internal class JsonFileToolkitRepository : IToolkitRepository
    {
        private const string ToolkitFileExtension = ".toolkit";

        public string Save(PatternToolkit toolkit)
        {
            var filename = CreateFilenameForToolkit(toolkit.PatternName, toolkit.Version);
            EnsurePathExists(filename);

            using (var file = File.CreateText(filename))
            {
                file.Write(toolkit.ToJson());
            }

            return filename;
        }

        private static void EnsurePathExists(string filename)
        {
            var directory = Directory.GetParent(filename)?.FullName ?? string.Empty;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string CreateFilenameForToolkit(string name, string version)
        {
            var filename = Path.ChangeExtension($"{name}_{version}", ToolkitFileExtension);
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            return Path.Combine(directory, filename);
        }
    }
}