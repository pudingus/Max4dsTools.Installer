using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uninstall
{   
    public static class Operation
    {
        class Section
        {
            public string Name = "";
            public readonly List<string> Lines = new List<string>();
        }

        public class Db
        {
            public List<string> FILES = new List<string>();
            public List<string> REGKEYS = new List<string>();
        }

        private static bool allowCleanup = false;

        public static Db LoadDb(string dbName) {
            using var reader = new StreamReader(dbName);
            string line;
            List<Section> sections = new List<Section>();
            Section currSection = new Section();
            while ((line = reader.ReadLine()) != null) {
                if (line[0] == '>') {
                    var sectName = line.Remove(0, 1);
                    currSection = new Section { Name = sectName };
                    sections.Add(currSection);
                }
                else {
                    currSection.Lines.Add(line);
                }
            }

            var db = new Db();

            Section section;
            section = sections.Find(t => t.Name == "files");
            if (section != null) db.FILES = section.Lines;

            section = sections.Find(t => t.Name == "registry");
            if (section != null) db.REGKEYS = section.Lines;

            return db;
        }

        private static bool IsPathDirectory(string path) {
            var sep1 = Path.DirectorySeparatorChar.ToString();
            var sep2 = Path.AltDirectorySeparatorChar.ToString();
            return path.EndsWith(sep1) || path.EndsWith(sep2);
        }

        public static void Uninstall(Db db, Action<string> a) {
            db.FILES.Reverse();

            foreach (var path in db.FILES) {
                if (IsPathDirectory(path)) {
                    if (Directory.Exists(path)) {
                        try {
                            Directory.Delete(path); //deletes only empty folder
                            a($"Remove: {path}");
                        }
                        catch (Exception) {
                            a($"Error: {path}");
                        }
                    }
                    else {
                        a($"Not found: {path}");
                    }
                }
                else {
                    if (File.Exists(path)) {
                        try {
                            File.Delete(path);
                            a($"Remove: {path}");
                        }
                        catch (Exception) {
                            a($"Error: {path}");
                        }
                    }
                    else {
                        a($"Not found: {path}");
                    }
                }
            }

            db.REGKEYS.Reverse();

            foreach (var key in db.REGKEYS) {
                try {
                    DeleteKey(key);
                    a.Invoke($"Remove: {key}");

                }
                catch (Exception) {
                    a.Invoke($"Not found: {key}");
                }
            }
            allowCleanup = true;
        }

        public static void Cleanup(string exePath, string dbPath) {
            if (allowCleanup) {
                Process.Start(new ProcessStartInfo() {
                    // 'del' doesnt set errorlevel on failure, thats why this looks weird
                    Arguments = $"/c timeout /t 1 /nobreak & del \"{exePath}\" 2>&1 1>nul | findstr \"^\" || del \"{dbPath}\" ",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
            }                      
        }

        //stackoverflow.com/a/954837
        private static bool IsDirectoryEmpty(string path) {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }


        private static RegistryKey GetRootKeyFromString(string key) {
            if (key == "HKEY_LOCAL_MACHINE") {
                return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            else if (key == "HKEY_CURRENT_USER") {
                return RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            }
            return null;
        }

        private static void DeleteKey(string key) {
            var index = key.IndexOf('\\');
            var root = key.Substring(0, index);
            var subKey = key.Remove(0, index+1);

            var rootKey = GetRootKeyFromString(root);

            rootKey?.DeleteSubKey(subKey);
        }
    }
}
