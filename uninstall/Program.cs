using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace uninstall
{
    static class Program
    {
        class Options
        {
            public bool delete = true;
            public bool silent = false;
        }

        private static Options GetOptions(string[] args) {
            var options = new Options();
            
            for (int i = 0; i < args.Length; i++) {
                if (args[i].ToLower() == "/dontdelete") {
                    options.delete = false;
                }
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].ToLower() == "/silent") {
                    options.silent = true;
                }
            }

            return options;
        }

        private static void LogLine(string line) {
            Debug.WriteLine(line);
        }

        private static string GetLogPath(string uninsPath) {
            var uninsDir = Path.GetDirectoryName(uninsPath);
            var logName = Path.GetFileNameWithoutExtension(uninsPath) + ".dat";
            return Path.Combine(uninsDir, logName);
        }

        /// <summary>
        /// Hlavní vstupní bod aplikace.
        /// </summary>
        [STAThread]
        static int Main(string[] args) {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
            var options = GetOptions(args);

            string exePath = Application.ExecutablePath;
            string exeDir = Path.GetDirectoryName(exePath);
            Directory.SetCurrentDirectory(exeDir);

            //Debug.WriteLine(Directory.GetCurrentDirectory());

            if (options.silent) {
                try {
                    string dbPath = GetLogPath(exePath);

                    var db = Operation.LoadDb(dbPath);
                    Operation.Uninstall(db, LogLine);
                    //System.Threading.Thread.Sleep(90000);

                    if (options.delete) Operation.Cleanup(exePath, dbPath);
                }
                catch (Exception) {
                    return 1;
                }
            }
            else {
                Operation.Db db;
                string dbPath;

                try {
                    exePath = Application.ExecutablePath;
                    dbPath = GetLogPath(exePath);

                    db = Operation.LoadDb(dbPath);
                }
                catch (Exception ex) {    //default .NET Framework handler is not available before Application.Run
                    MessageBox.Show($"Cannot open uninstall database.\n\n{ex.GetType().Name}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return 1;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new UninsForm(db));
                
                try {
                    if (options.delete) Operation.Cleanup(exePath, dbPath);
                }
                catch (Exception ex) {
                    //MessageBox.Show($"Error occured, some files could not be removed:\n{exePath}\n{dbPath}\n\n{ex.GetType().Name}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MessageBox.Show($"Some files could not be removed:\n{exePath}\n{dbPath}\n\nYou can remove them manually.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return 1;
                }
            }
            
            return 0;
        }
    }
}
