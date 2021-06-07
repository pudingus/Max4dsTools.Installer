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
            public string NAME = "";
            public List<string> FILES = new List<string>();
            public List<string> REGKEYS = new List<string>();
            public List<string> FOLDERS = new List<string>();
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

            section = sections.Find(t => t.Name == "name");
            if (section != null) db.NAME = section.Lines[0];

            section = sections.Find(t => t.Name == "registry");
            if (section != null) db.REGKEYS = section.Lines;

            section = sections.Find(t => t.Name == "folders");
            if (section != null) db.FOLDERS = section.Lines;

            return db;
        }

        public static void Uninstall(Db db, Action<string> a) {
            foreach (var text in db.FILES) {
                if (File.Exists(text)) {
                    try {
                        File.Delete(text);
                        a.Invoke($"Remove: {text}");
                    }
                    catch (Exception) {
                        a.Invoke($"Error: {text}");
                    }
                }
                else {
                    a.Invoke($"Not found: {text}");
                }
            }

            foreach (var folder in db.FOLDERS) {
                try {
                    Directory.Delete(folder);  //delete only empty folder
                    a.Invoke($"Remove: {folder}");
                }
                catch (IOException) { }
                catch (SecurityException) { }
                catch (UnauthorizedAccessException) { }
            }

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
                    Arguments = $"/c timeout /t 1 /nobreak & del \"{exePath}\" & del \"{dbPath}\" ",
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
