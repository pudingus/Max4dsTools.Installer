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
using System.Runtime.InteropServices;

using setup_common;

namespace setup
{
    public partial class SetupForm : Form
    {
        private enum Pages
        {
            Directory,
            Installing,
            Completed,
            Aborted
        }

        const string NAME = Shared.NAME;
        const string VERSION = Shared.VERSION;

        const string UNINS_NAME = "max4ds_uninstall.exe";
        const string PUBLISHER = "pudingus";
        const string URL = "https://github.com/pudingus/Max4dsTools";

        const string RES_PATH = "setup.embed.";

        bool ABORT = false;
        private StreamWriter db;
        private string _instDir;
        private Pages page = Pages.Directory;


        public SetupForm() {
            InitializeComponent();
            this.Text = $"{NAME} {VERSION} Setup";
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private string FindDirInVersion(RegistryKey key, int version) {
            if (key == null) return null;

            string[] languages = { "MAX-1:409", "MAX-1:40C", "MAX-1:407", "MAX-1:411", "MAX-1:412", "MAX-1:804" };

            if (version >= 15) {
                try {
                    string dir = key.GetValue("Installdir") as string;

                    if (dir != null && Directory.Exists(dir)) {
                        return dir;
                    }
                }
                catch (SecurityException) { }
                catch (ObjectDisposedException) { }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            else {
                foreach (var language in languages) {
                    try {
                        using var langKey = key.OpenSubKey(language);
                        if (langKey == null) continue;

                        string dir = langKey.GetValue("Installdir") as string;

                        if (dir != null && Directory.Exists(dir)) {
                            return dir;
                        }
                    }
                    catch (ArgumentNullException) { }
                    catch (ObjectDisposedException) { }
                    catch (SecurityException) { }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }            

            return null;
        }

        private string FindDirInHive(RegistryKey hive) {
            if (hive == null) return null;

            int[] versions = { 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11 };
            string[] editions = { "3dsMax", "3dsMaxDesign" };

            foreach (var edition in editions) {

                foreach (var version in versions) {
                    string value = null;
                    try {
                        using var key = hive.OpenSubKey($@"SOFTWARE\Autodesk\{edition}\{version}.0");

                        value = FindDirInVersion(key, version);
                    }
                    catch (ArgumentNullException) { }
                    catch (ObjectDisposedException) { }
                    catch (SecurityException) { }                    

                    if (value != null) return value;                    
                }
            }
            return null;
        }

        private string Find3dsMaxDir() {
            string dir = null;
            try {
                var views = new List<RegistryView> { 
                    RegistryView.Registry64,
                    RegistryView.Registry32 
                };

                foreach (var view in views) {
                    //"If you request a 64-bit view on a 32-bit operating system, the returned keys will be in the 32-bit view."
                    using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);

                    dir = FindDirInHive(hklm);

                    if (dir != null) break;
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

            // calculate needed size
            using var zipStream = GetEmbeddedResourceStream("files.zip");

            var zip = new ZipArchive(zipStream);
            long length = 0;
            foreach (var entry in zip.Entries) {
                length += entry.Length;
            }         

            lblSize.Text = $"At least {length / 1024} KB of free disk space is required.";
            // ------------------------
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

        private bool IsInstallDirValid(string dir) {
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
                lblPage.Text = "Installation Failed";
                lblShort.Text = "Setup was unsuccessful.";
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

        //only for zip entries
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

        struct Item
        {
            public string path;
            public int[] condition;
            public string newPath;
        };

        private string RemapPaths(string fullname, int majorVer, Item[] list) {
            fullname = fullname.Replace("/", "\\");          

            bool match = false;
            foreach (var item in list) {
                if (fullname.StartsWith(item.path) && (item.condition == null || item.condition.Contains(majorVer))) {
                    match = true;

                    if (item.newPath != null) {
                        fullname = item.newPath + fullname.Remove(0, item.path.Length);
                    }
                    break;
                }
            }
            if (match == false) fullname = null;

            return fullname;
        }

        private void ExtractZip(Stream zipStream, string instDir, int majorVer) {

            var macrosDir = (majorVer >= 1 && majorVer < 15) ? "ui\\macroscripts\\" : null;

            var list = new Item[] {
                new Item { path = "stdplugs\\2014\\", condition = new int[] {16}, newPath = "stdplugs\\" },
                new Item { path = "stdplugs\\2015-2016\\", condition = new int[] {17, 18}, newPath = "stdplugs\\" },
                new Item { path = "stdplugs\\2017\\", condition = new int[] {19}, newPath = "stdplugs\\" },
                new Item { path = "stdplugs\\2018\\", condition = new int[] {20}, newPath = "stdplugs\\" },
                new Item { path = "stdplugs\\2019\\", condition = new int[] {21}, newPath = "stdplugs\\" },
                new Item { path = "stdplugs\\2020-2021\\", condition = new int[] {22, 23}, newPath = "stdplugs\\" },
                new Item { path = "stdplugs\\2022\\", condition = new int[] {24}, newPath = "stdplugs\\" },
                new Item { path = "scripts\\" },
                new Item { path = "macroscripts\\", newPath = macrosDir },
            };

            db.WriteLine(">files");
            var zip = new ZipArchive(zipStream);

            foreach (var entry in zip.Entries) {
                string fullname = RemapPaths(entry.FullName, majorVer, list);

                if (fullname == null) continue;

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
                            var result = TryCreateDir(dirPath);

                            while (result == false) {
                                var dresult = RetryCancel("Error creating directory:", dirPath);                                
                                if (dresult == DialogResult.Retry) {
                                    result = TryCreateDir(dirPath);
                                }
                                else { 
                                    Abort();  
                                    return; 
                                }
                            }

                            db.WriteLine(dirsum);
                        }
                    }
                }
                else {                    
                    try {
                        var fullpath = Path.Combine(instDir, fullname);
                        {
                            using var entryStream = entry.Open();
                            var result = WriteStreamToFile(entryStream, fullpath);

                            while (result == false) {
                                var dresult = RetryCancel("Error opening file for writing:", fullpath);
                                if (dresult == DialogResult.Retry) {
                                    result = WriteStreamToFile(entryStream, fullpath);
                                }
                                else {
                                    Abort(); 
                                    return; 
                                }
                            }

                            db.WriteLine(fullname);
                            db.Flush();
                        }                        
                        File.SetLastWriteTimeUtc(fullpath, entry.LastWriteTime.UtcDateTime);
                        LogLine($"Extract: {fullname}");
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                        LogLine($"Error: {fullname}");
                    }
                }                
            }          
        }

        private int GetVersion(string exePath) {
            int majorVer = 0;
            try {
                var info = FileVersionInfo.GetVersionInfo(exePath);                
                majorVer = info.FileMajorPart;                
            }
            catch (FileNotFoundException) { }
            return majorVer;
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
                var dresult = RetryCancel("Error opening file for writing:", uninsPath);
                if (dresult == DialogResult.Retry) {
                    result = WriteStreamToFile(uninsStream, uninsPath);
                }
                else {
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
                    db.WriteLine(">registry");
                    db.WriteLine(key.Name);
                    db.Flush();
                    key.SetValue("DisplayName", $"{NAME} {VERSION}");
                    key.SetValue("DisplayVersion", VERSION);
                    key.SetValue("DisplayIcon", uninsPath + ",0");
                    key.SetValue("Publisher", PUBLISHER);
                    key.SetValue("UninstallString", uninsPath);
                    key.SetValue("NoModify", 1);
                    key.SetValue("NoRepair", 1);
                    key.SetValue("URLInfoAbout", URL);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }

            try {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                using var key = baseKey.CreateSubKey(@"Software\" + NAME);
                db.WriteLine(key.Name);
                db.Flush();
                key.SetValue("LastInstallDir", _instDir);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
        }

        private void DisableFile(string filename) {
            try {
                File.Move(filename, filename + "__off");
            }         
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private string GetDbPath(string uninsPath) {
            string uninsDir = Path.GetDirectoryName(uninsPath);
            string dbName = Path.GetFileNameWithoutExtension(uninsPath) + ".dat";
            return Path.Combine(uninsDir, dbName);
        }

        private void LogLine(string line) {
            if (line == null) return;
            //Debug.WriteLine(line);
            tbxLog.AppendText(line + Environment.NewLine);
        }

        private void Abort() {
            SetPage(Pages.Aborted);
            ABORT = true;
            LogLine("Aborted");
            db?.Close();
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

        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(Process process, int buffer = 1024) {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                null;
        }
    

        private void Handler(Pages page) {
            SetPage(page);
        }

        private bool ValidatePath(string instDir) {
            //check if directory exists
            if (!IsInstallDirValid(instDir)) return false;

            //check if directory contains 3dsmax.exe
            var exeName = "3dsmax.exe";
            var exePath = Path.Combine(instDir, exeName);

            if (!File.Exists(exePath)) {
                MessageBox.Show($"{exeName} not found in the provided directory.\nPlease select a different folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }



            //check if 3dsmax.exe in the directory is running
            var procs = Process.GetProcessesByName("3dsmax");
            //var procs = Process.GetProcesses();

            foreach (var proc in procs) {
                string name = "";
                try {
                    name = GetMainModuleFileName(proc);
                }
                catch (Exception) { }

                Debug.WriteLine(name);
                if (name == exePath) {
                    MessageBox.Show($"{exeName} is running.\nPlease close 3ds Max before proceeding.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void PostExtract() {
            var file = Path.Combine(_instDir, "scripts/startup/startup_mafia_4ds.ms");
            DisableFile(file);
        }

        private void UninstallPrevious(string uninsPath) {
            if (uninsPath == null) return;

            if (File.Exists(uninsPath)) {
                Refresh();
                LogLine("Removing previous installation...");
                try {
                    using var proc = Process.Start(uninsPath, "/silent /dontdelete");
                    
                    if (proc.WaitForExit(15000)) {
                        if (proc.ExitCode > 0) {
                            LogLine($"Uninstallation failed. Exit code: {proc.ExitCode}");
                            Abort();
                        }
                        else {
                            proc.Close();
                            System.Threading.Thread.Sleep(400);
                        }
                    }
                    else {
                        LogLine($"Uninstallation failed");
                        proc.Kill();
                        Abort();
                    }
                }
                catch (Exception ex) {
                    LogLine($"Uninstallation failed");
                    LogLine($"{ex.GetType().Name}: {ex.Message}");
                    Abort();
                }
            }                      
        }

        private DialogResult RetryCancel(string error, string path) {
            return MessageBox.Show($"{error} \n\n{path}\n\nClick retry to try again, or\nCancel to abort the installation.", Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
        }
        
        private void Install(string instDir, Action<Pages> action) {
            _instDir = instDir;

            if (!ValidatePath(instDir)) return;

            //SetPage(Pages.Installing);
            action(Pages.Installing);
            Directory.SetCurrentDirectory(instDir);
            LogLine($"Output folder: {instDir}");

            var uninsPath = Path.Combine(instDir, UNINS_NAME);

            UninstallPrevious(uninsPath);

            if (ABORT) return;

            var dbPath = GetDbPath(uninsPath);
            db = TryStreamWriter(dbPath);

            while (db == null) {
                var dresult = RetryCancel("Error opening file for writing:", dbPath);
                if (dresult == DialogResult.Retry) {
                    db = TryStreamWriter(dbPath);
                }
                else {
                    Abort();
                    return;
                }
            }

            WriteUninstaller(uninsPath);
            if (ABORT) return;

            WriteRegistry(uninsPath);
            if (ABORT) return;

            //--------------- 
            var exeName = "3dsmax.exe";
            var exePath = Path.Combine(instDir, exeName);

            using var zipStream = GetEmbeddedResourceStream("files.zip");

            ExtractZip(zipStream, instDir, GetVersion(exePath));   //try

            //------------ -----------------------

            if (ABORT) return;            

            PostExtract();

            //SetPage(Pages.Completed);
            action(Pages.Completed);

            LogLine("Completed");
            Refresh();

            db.Close();
        }

        private void btnInstall_Click(object sender, EventArgs e) {
            if (page == Pages.Completed || page == Pages.Aborted) {
                this.Close();
                return;
            }

            Install(tbxDestination.Text, Handler);
        }

        private void tbxLog_KeyDown(object sender, KeyEventArgs e) {
            if (!(e.Control && e.KeyCode == Keys.C)) {
                e.SuppressKeyPress = true;
            }
        }
    }
}
