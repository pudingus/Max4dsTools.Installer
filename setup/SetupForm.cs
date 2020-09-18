using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FolderSelect;
using Microsoft.Win32;
using Core.IO;
using System.Reflection;
using System.IO.Compression;
using System.Security;
using System.Threading.Tasks;

namespace setup
{
    public partial class SetupForm : Form
    {
        const string NAME = "Max 4ds Tools";
        const string VERSION = "0.5.0";
        const string UNINS_NAME = "max4ds_uninstall.exe";

        readonly int[] versions = { 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11 };
        readonly string[] editions = { "3dsMax", "3dsMaxDesign" };
        readonly string[] languages = { "MAX-1:409", "MAX-1:40C", "MAX-1:407", "MAX-1:411", "MAX-1:412", "MAX-1:804" };

        const string RES_PATH = "setup.embed.";

        bool ABORT = false;

        private StreamWriter log;

        private string _instDir;

        public SetupForm() {
            InitializeComponent();
            this.Text = $"{NAME} {VERSION} Setup";
            this.FormClosing += SetupForm_FormClosing;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private string FindDirInHive(RegistryKey hive) {
            foreach (var edition in editions) {
                foreach (var version in versions) {

                    using var key = hive.OpenSubKey($@"SOFTWARE\Autodesk\{edition}\{version}.0");
                    if (key == null) {
                        continue;
                    }
                    else {
                        var value = key.GetValue("Installdir") as string;
                        if (value == null || !Directory.Exists(value)) {
                            foreach (var language in languages) {
                                using var langKey = key.OpenSubKey(language);
                                if (langKey == null) {
                                    continue;
                                }
                                else {
                                    value = langKey.GetValue("Installdir") as string;
                                    if (value == null) continue;
                                    else if (!Directory.Exists(value)) continue;
                                    else return value as string;
                                }
                            }

                        }
                        else {
                            key.Close();
                            return value as string;
                        }
                    }
                }
            }
            return null;
        }

        private string Find3dsMaxDir() {
            string dir = null;
            try {
                var HKLM64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                dir = FindDirInHive(HKLM64);
                HKLM64.Close();
                if (dir == null) {
                    var HKLM32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    dir = FindDirInHive(HKLM32);
                    HKLM32.Close();
                }
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch { }
            return dir;
        }
                
        private string FindPreviousDir() {
            try {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey($"Software\\{NAME}");
                var value = key?.GetValue($"LastInstallDir") as string;
                if (value == null || !Directory.Exists(value)) return null;
                else return value;
            }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
            catch { }
            return null;
        }

        private void SetupForm_Load(object sender, EventArgs e) {

            var dir = FindPreviousDir();
            if (dir == null) {
                dir = Find3dsMaxDir();
            }
            
            if (dir == null) {
                dir = "Select 3ds Max folder";
            }
            tbxDestination.Text = dir;
        }

        private void SetupForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (page == Pages.Installing) {
                e.Cancel = true;
            }
            else if (page == Pages.Directory) {
                if (AreYouSure() == DialogResult.No) {
                    e.Cancel = true;
                }
            }
        }

        private enum Pages
        {
            Directory,
            Installing,
            Completed,
            Aborted
        }

        private Pages page = Pages.Directory;

        private void btnCancel_Click(object sender, EventArgs e) {            
            this.Close();           
        }

        private DialogResult AreYouSure() {
            return MessageBox.Show($"Are you sure you want to quit {Text}?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }        

        private void btnBrowse_Click(object sender, EventArgs e) {
            var dlg = new FolderSelectDialog();

            var result = dlg.ShowDialog();
            if (result) {
                tbxDestination.Text = dlg.FileName;
            }
        }

        private bool IsValidDir(string dir) {
            bool absolute = PathCore.IsPathFullyQualified(dir);
            if (!absolute) {
                MessageBox.Show("Invalid directory.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else if (!Directory.Exists(dir)) {
                MessageBox.Show("Directory doesn't exist.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void SetPage(Pages page) {
            this.page = page;
            if (page == Pages.Installing) {
                lblPage.Text = "Installing";
                lblShort.Text = $"Please wait while {NAME} is being installed.";
                lblLong.Text = "Installing...";
                progressBar.Visible = true;
                grpDestination.Visible = false;
                btnCancel.Enabled = false;
                tbxLog.Visible = true;
                btnInstall.Enabled = false;
                lblSize.Visible = false;
            }
            else if (page == Pages.Completed) {
                lblPage.Text = "Installation Complete";
                lblShort.Text = "Setup was completed successfully.";                
                lblLong.Text = "Completed";
                btnInstall.Text = "Close";
                btnCancel.Enabled = false;
                progressBar.Value = 100;
                btnInstall.Enabled = true;
            }
            else if (page == Pages.Aborted) {
                lblPage.Text = "Installation Aborted";
                lblShort.Text = "Setup was unsuccessfull.";
                lblLong.Text = "Aborted";
                btnInstall.Text = "Close";
                btnCancel.Enabled = false;
                progressBar.Value = 100;
                btnInstall.Enabled = true;
            }
        }

        private void RenameFile(string path, string newName) {
            string dir = Path.GetDirectoryName(path);
            File.Move(path, Path.Combine(dir, newName));
        }

        private bool IsPathDirectory (string path) {
            var sep1 = Path.DirectorySeparatorChar.ToString();
            var sep2 = Path.AltDirectorySeparatorChar.ToString();
            return path.EndsWith(sep1) || path.EndsWith(sep2);
        }

        private bool TryCreateDir(string dir) {
            var result = false;
            try {
                Directory.CreateDirectory(dir);
                result = true;
                //LogLine("Create folder: " + dirsum);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return result;
        }

        private void ExtractZip(Stream zipStream, string instDir, string macrosDir) {
            log.WriteLine(">files");
            var dirs = new List<string>();
            var zip = new ZipArchive(zipStream);
            foreach (var entry in zip.Entries) {
                var fullname = entry.FullName;
                fullname = fullname.Replace("/", "\\");
                // remap folder name
                var replaceName = "macros\\";
                if (fullname.StartsWith(replaceName)) {
                    fullname = macrosDir + fullname.Remove(0, replaceName.Length);
                }
                // -------------

                Debug.WriteLine(fullname);

                if (IsPathDirectory(fullname)) {
                    var sep1 = Path.DirectorySeparatorChar;
                    var sep2 = Path.AltDirectorySeparatorChar;
                    var segments = fullname.Split(new char[]{sep1, sep2}, StringSplitOptions.RemoveEmptyEntries);


                    string dirsum = "";
                    foreach (var sp in segments) {
                        dirsum += sp + "\\";
                        var dirPath = Path.Combine(instDir, dirsum);
                        if (!Directory.Exists(dirPath)) {
                            dirs.Add(dirsum);
                            var result = TryCreateDir(dirPath);

                            while (result == false) {
                                var dresult = MessageBox.Show($"Error creating directory: \n\n{dirPath}\n\nClick retry to try again, or\nCancel to abort the installation.", Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                                if (dresult == DialogResult.Retry) {
                                    result = TryCreateDir(dirPath);
                                }
                                else { Abort(); return; }
                            }
                        }
                    }
                }
                else {
                    log.WriteLine(fullname);
                    try {
                        var fullpath = Path.Combine(instDir, fullname);
                        {
                            using var entryStream = entry.Open();

                            var result = WriteStreamToFile(entryStream, fullpath);

                            while (result == false) {
                                var dresult = MessageBox.Show($"Error opening file for writing: \n\n{fullpath}\n\nClick abort to stop the installation,\nRetry to try again, or\nIgnore to skip this file.", Text, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                                if (dresult == DialogResult.Retry) {
                                    result = WriteStreamToFile(entryStream, fullpath);
                                }
                                else if (dresult == DialogResult.Ignore) {
                                    result = true;
                                }
                                else if (dresult == DialogResult.Abort) { Abort(); return; }
                            }
                        }                        
                        File.SetLastWriteTimeUtc(fullpath, entry.LastWriteTime.UtcDateTime);
                        LogLine($"Extract: {fullname}");
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                        LogLine($"Error: {fullname}");
                    }
                }                
            }
            if (dirs.Count != 0) {
                dirs.Reverse();
                log.WriteLine(">folders");
                foreach (var dir in dirs) {
                    log.WriteLine(dir);
                }
            }
            
        }

        private bool IsOldVersion(string exePath) {
            bool oldVersion = false;
            try {
                var info = FileVersionInfo.GetVersionInfo(exePath);
                var version = info.FileVersion;
                if (version != null) {
                    var majorVer = info.FileMajorPart;
                    if (majorVer < 15) {
                        oldVersion = true;
                    }
                }
            }
            catch (FileNotFoundException) {

            }
            return oldVersion;
        }

        private Stream GetEmbeddedResourceStream(string resName) {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(RES_PATH + resName);
        }

        private bool WriteStreamToFile(Stream stream, string filePath) {
            var result = false;
            try {
                using var file = File.Create(filePath);
                stream.CopyTo(file);
                result = true;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return result;
        }

        private void WriteUninstaller(string uninsPath) {
            
            using var uninsStream = GetEmbeddedResourceStream("uninstall.exe");
            var result = WriteStreamToFile(uninsStream, uninsPath);

            while (result == false) {
                var dresult = MessageBox.Show($"Error opening file for writing: \n\n{uninsPath}\n\nClick abort to stop the installation,\nRetry to try again, or\nIgnore to skip this file.", Text, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                if (dresult == DialogResult.Retry) {
                    result = WriteStreamToFile(uninsStream, uninsPath);
                }
                else if (dresult == DialogResult.Ignore) {
                    result = true;
                }
                else if (dresult == DialogResult.Abort) {
                    Abort(); 
                    return;
                }
            }

            var relPath = PathCore.GetRelativePath(_instDir, uninsPath);
            LogLine($"Write uninstaller: {relPath}");            
        }

        private void WriteRegistry(string uninsPath) {
            try {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                var guid = Guid.NewGuid();
                var subname = NAME + "__" + guid.ToString("N");
                {
                    using var key = baseKey.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + subname);
                    log.WriteLine(">registry");
                    log.WriteLine(key.Name);
                    key.SetValue("DisplayName", NAME);
                    key.SetValue("DisplayVersion", VERSION);
                    key.SetValue("DisplayIcon", uninsPath + ",0");
                    key.SetValue("Publisher", "pudingus");
                    key.SetValue("UninstallString", uninsPath);
                    key.SetValue("NoModify", 1);
                    key.SetValue("NoRepair", 1);
                } 
                {
                    using var key = baseKey.CreateSubKey(@"Software\" + NAME);
                    log.WriteLine(key.Name);
                    key.SetValue("LastInstallDir", _instDir);
                }

            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
        }

        private void DisableFile(string filename) {
            try {
                File.Move(filename, filename + "__off");
            }         
            catch (IOException) {

            }
            catch (UnauthorizedAccessException) {

            }
        }

        private string GetLogPath(string uninsPath) {
            var uninsDir = Path.GetDirectoryName(uninsPath);
            var logName = Path.GetFileNameWithoutExtension(uninsPath) + ".dat";
            return Path.Combine(uninsDir, logName);
        }

        private void LogLine(string line) {
            //Debug.WriteLine(line);
            tbxLog.AppendText(line + Environment.NewLine);
        }

        private void Abort() {
            SetPage(Pages.Aborted);
            ABORT = true;
            LogLine("Aborted");
            log?.Close();
        }

        private StreamWriter TryStreamWriter(string path) {
            StreamWriter result = null;
            try {
                result = new StreamWriter(path);                
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return result;
        }

        private void btnInstall_Click(object sender, EventArgs e) {
            if (page == Pages.Completed || page == Pages.Aborted) {
                this.Close();
                return;
            }
            var instDir = tbxDestination.Text;
            _instDir = instDir;

            if (!IsValidDir(instDir)) return;

            SetPage(Pages.Installing);
            Directory.SetCurrentDirectory(instDir);
            LogLine($"Output folder: {instDir}");

            var uninsPath = Path.Combine(instDir, UNINS_NAME);

            try {
                if (File.Exists(uninsPath)) {
                    this.Refresh();
                    LogLine("Removing previous installation...");
                    var proc = Process.Start(uninsPath, "/silent /dontdelete");
                    proc.WaitForExit();
                    System.Threading.Thread.Sleep(400);
                }
            }
            catch (IOException) { }
            catch (Win32Exception) { }

            var logPath = GetLogPath(uninsPath);
            log = TryStreamWriter(logPath);

            while (log == null) {
                var dresult = MessageBox.Show($"Error opening file for writing: \n\n{logPath}\n\nClick retry to try again, or\nCancel to abort the installation.", Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dresult == DialogResult.Retry) {
                    log = TryStreamWriter(logPath);
                }
                else {
                    Abort();
                    return;
                }
            }

            log.WriteLine(">name");
            log.WriteLine(NAME);

            log.WriteLine(">version");
            log.WriteLine(VERSION);

            WriteUninstaller(uninsPath);

            if (ABORT) return;

            var exeName = "3dsmax.exe";
            var exePath = Path.Combine(instDir, exeName);
            bool useOldFolderStructure = IsOldVersion(exePath);

            var macrosDir = useOldFolderStructure ? "ui\\macroscripts\\" : "macroscripts\\";

            using var zipStream = GetEmbeddedResourceStream("files.zip");
            
            ExtractZip(zipStream, instDir, macrosDir);   //try

            if (ABORT) return;

            WriteRegistry(uninsPath);

            var disableFile = Path.Combine(instDir, "scripts/startup/startup_mafia_4ds.ms");
            DisableFile(disableFile);

            SetPage(Pages.Completed);
            LogLine("Completed");
            this.Refresh();

            log.Close();
        }

        private void tbxLog_KeyDown(object sender, KeyEventArgs e) {
            if (!(e.Control && e.KeyCode == Keys.C)) {
                e.SuppressKeyPress = true;
            }
        }
    }
}
