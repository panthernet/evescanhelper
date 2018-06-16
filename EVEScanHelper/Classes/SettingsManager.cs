using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EVEScanHelper.Classes
{
    public class SettingsManager
    {
        public const string DATA_FOLDER = "Data";
        public static string AppFolder = AppDomain.CurrentDomain.BaseDirectory;
        public static string SystemsFile { get; } = Path.Combine(AppFolder, DATA_FOLDER, "systems.txt");

        public static IList<string> GetSystems()
        {
            return File.ReadAllLines(SystemsFile);
        }
    }
}
