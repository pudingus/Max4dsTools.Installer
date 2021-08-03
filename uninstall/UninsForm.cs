using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using setup_common;

namespace uninstall
{
    public partial class UninsForm : Form
    {
        const string NAME = Common.NAME;

        Operation.Db db;
        public UninsForm(Operation.Db db) {
            InitializeComponent();
            this.db = db;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private enum Pages
        {
            Directory,
            Uninstalling,
            Completed
        }

        private Pages page = Pages.Directory;

        private void UninsForm_Load(object sender, EventArgs e) {
            tbxPath.Text = Directory.GetCurrentDirectory();
            Text = NAME + " Uninstall";
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        } 

        private void LogLine(string line) {
            Debug.WriteLine(line);
            tbxLog.AppendText(line + Environment.NewLine);
        }

        private void SetPage(Pages page) {
            this.page = page;
            if (page == Pages.Uninstalling) {
                lblPage.Text = "Uninstalling";
                lblShort.Text = $"Please wait while {NAME} is being uninstalled.";
                lblLong.Text = "Uninstalling...";
                btnCancel.Enabled = false;
                progressBar.Visible = true;
                lblPath.Visible = false;
                tbxPath.Visible = false;
                tbxLog.Visible = true;
            }
            else if (page == Pages.Completed) {
                page = Pages.Completed;
                lblPage.Text = "Uninstallation Complete";
                lblShort.Text = "Uninstall was completed successfully";
                lblLong.Text = "Completed";
                progressBar.Value = 100;
                btnUninstall.Text = "Close";
            }
        }

        private void btnUninstall_Click(object sender, EventArgs e) {

            if (page == Pages.Completed) {
                Close();
                return;
            }

            var exeName = "3dsmax.exe";
            var exePath = Path.Combine(Directory.GetCurrentDirectory(), exeName);
            
            if (Common.IsMaxRunning(exePath)) {
                MessageBox.Show($"{exeName} is running.\nPlease close 3ds Max before proceeding.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            SetPage(Pages.Uninstalling);
            LogLine(Directory.GetCurrentDirectory());

            Operation.Uninstall(db, LogLine);

            SetPage(Pages.Completed);

            LogLine("Completed");
        }

        private void tbxLog_KeyDown(object sender, KeyEventArgs e) {
            if (!(e.Control && e.KeyCode == Keys.C)) {                
                e.SuppressKeyPress = true;
            }
        }
    }
}
