namespace uninstall
{
    partial class UninsForm
    {
        /// <summary>
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód generovaný Návrhářem Windows Form

        /// <summary>
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent() {
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblLong = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblShort = new System.Windows.Forms.Label();
            this.lblPage = new System.Windows.Forms.Label();
            this.tbxPath = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.lblPath = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tbxLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label6.Location = new System.Drawing.Point(11, 314);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(480, 2);
            this.label6.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Location = new System.Drawing.Point(-1, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(506, 2);
            this.label5.TabIndex = 17;
            // 
            // lblLong
            // 
            this.lblLong.Location = new System.Drawing.Point(21, 74);
            this.lblLong.Name = "lblLong";
            this.lblLong.Size = new System.Drawing.Size(447, 34);
            this.lblLong.TabIndex = 14;
            this.lblLong.Text = "Max 4ds Tools will be uninstalled from the following folder.\r\nIf you are upgradin" +
    "g to a newer version, there\'s no need to uninstall.";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.lblShort);
            this.panel1.Controls.Add(this.lblPage);
            this.panel1.Location = new System.Drawing.Point(-4, -5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(507, 64);
            this.panel1.TabIndex = 13;
            // 
            // lblShort
            // 
            this.lblShort.AutoSize = true;
            this.lblShort.Location = new System.Drawing.Point(25, 31);
            this.lblShort.Name = "lblShort";
            this.lblShort.Size = new System.Drawing.Size(219, 13);
            this.lblShort.TabIndex = 1;
            this.lblShort.Text = "Remove Max 4ds Tools from your computer.";
            // 
            // lblPage
            // 
            this.lblPage.AutoSize = true;
            this.lblPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPage.Location = new System.Drawing.Point(19, 14);
            this.lblPage.Name = "lblPage";
            this.lblPage.Size = new System.Drawing.Size(142, 13);
            this.lblPage.TabIndex = 0;
            this.lblPage.Text = "Uninstall Max 4ds Tools";
            // 
            // tbxPath
            // 
            this.tbxPath.Location = new System.Drawing.Point(129, 142);
            this.tbxPath.Name = "tbxPath";
            this.tbxPath.ReadOnly = true;
            this.tbxPath.Size = new System.Drawing.Size(350, 21);
            this.tbxPath.TabIndex = 3;
            this.tbxPath.Text = "D:\\max4ds_d";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(414, 326);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Location = new System.Drawing.Point(328, 326);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(75, 23);
            this.btnUninstall.TabIndex = 10;
            this.btnUninstall.Text = "Uninstall";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(21, 145);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(90, 13);
            this.lblPath.TabIndex = 18;
            this.lblPath.Text = "Uninstalling from:";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(24, 92);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(449, 18);
            this.progressBar.TabIndex = 19;
            this.progressBar.Visible = false;
            // 
            // tbxLog
            // 
            this.tbxLog.Location = new System.Drawing.Point(24, 116);
            this.tbxLog.Multiline = true;
            this.tbxLog.Name = "tbxLog";
            this.tbxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxLog.Size = new System.Drawing.Size(449, 181);
            this.tbxLog.TabIndex = 20;
            this.tbxLog.Visible = false;
            this.tbxLog.WordWrap = false;
            this.tbxLog.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxLog_KeyDown);
            // 
            // UninsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 361);
            this.Controls.Add(this.tbxLog);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.tbxPath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblLong);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUninstall);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "UninsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Max 4ds Tools Uninstall";
            this.Load += new System.EventHandler(this.UninsForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblLong;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblShort;
        private System.Windows.Forms.Label lblPage;
        private System.Windows.Forms.TextBox tbxPath;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox tbxLog;
    }
}

