namespace setup
{
    partial class SetupForm
    {
        /// <summary>
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód generovaný Návrhářem Windows Form

        /// <summary>
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupForm));
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.tbxDestination = new System.Windows.Forms.TextBox();
            this.grpDestination = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblShort = new System.Windows.Forms.Label();
            this.lblPage = new System.Windows.Forms.Label();
            this.lblLong = new System.Windows.Forms.Label();
            this.lblReq = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbxLog = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblSize = new System.Windows.Forms.Label();
            this.grpDestination.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnInstall
            // 
            this.btnInstall.Location = new System.Drawing.Point(328, 326);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(75, 23);
            this.btnInstall.TabIndex = 0;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(414, 326);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(360, 27);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(90, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // tbxDestination
            // 
            this.tbxDestination.Location = new System.Drawing.Point(15, 28);
            this.tbxDestination.Name = "tbxDestination";
            this.tbxDestination.Size = new System.Drawing.Size(333, 21);
            this.tbxDestination.TabIndex = 3;
            this.tbxDestination.Text = "D:\\max4ds_d";
            // 
            // grpDestination
            // 
            this.grpDestination.Controls.Add(this.tbxDestination);
            this.grpDestination.Controls.Add(this.btnBrowse);
            this.grpDestination.Location = new System.Drawing.Point(18, 177);
            this.grpDestination.Name = "grpDestination";
            this.grpDestination.Size = new System.Drawing.Size(467, 63);
            this.grpDestination.TabIndex = 4;
            this.grpDestination.TabStop = false;
            this.grpDestination.Text = "Destination Folder";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.lblShort);
            this.panel1.Controls.Add(this.lblPage);
            this.panel1.Location = new System.Drawing.Point(-4, -5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(507, 64);
            this.panel1.TabIndex = 5;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(449, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(55, 55);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // lblShort
            // 
            this.lblShort.AutoSize = true;
            this.lblShort.Location = new System.Drawing.Point(25, 31);
            this.lblShort.Name = "lblShort";
            this.lblShort.Size = new System.Drawing.Size(252, 13);
            this.lblShort.TabIndex = 1;
            this.lblShort.Text = "Choose the folder in which to install Max 4ds Tools.";
            // 
            // lblPage
            // 
            this.lblPage.AutoSize = true;
            this.lblPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPage.Location = new System.Drawing.Point(19, 14);
            this.lblPage.Name = "lblPage";
            this.lblPage.Size = new System.Drawing.Size(140, 13);
            this.lblPage.TabIndex = 0;
            this.lblPage.Text = "Choose Install Location";
            // 
            // lblLong
            // 
            this.lblLong.Location = new System.Drawing.Point(21, 74);
            this.lblLong.Name = "lblLong";
            this.lblLong.Size = new System.Drawing.Size(447, 34);
            this.lblLong.TabIndex = 6;
            this.lblLong.Text = "Setup will install Max 4ds Tools in the following folder. To install in a differe" +
    "nt folder, click Browse and select another folder. Click Install to start the in" +
    "stalation.";
            // 
            // lblReq
            // 
            this.lblReq.AutoSize = true;
            this.lblReq.Location = new System.Drawing.Point(21, 254);
            this.lblReq.Name = "lblReq";
            this.lblReq.Size = new System.Drawing.Size(113, 13);
            this.lblReq.TabIndex = 7;
            this.lblReq.Text = "Space required: 46 KB";
            this.lblReq.Visible = false;
            // 
            // label5
            // 
            this.label5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label5.Location = new System.Drawing.Point(-1, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(506, 2);
            this.label5.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label6.Location = new System.Drawing.Point(11, 314);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(480, 2);
            this.label6.TabIndex = 8;
            // 
            // tbxLog
            // 
            this.tbxLog.Location = new System.Drawing.Point(24, 116);
            this.tbxLog.Multiline = true;
            this.tbxLog.Name = "tbxLog";
            this.tbxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxLog.Size = new System.Drawing.Size(449, 181);
            this.tbxLog.TabIndex = 22;
            this.tbxLog.Visible = false;
            this.tbxLog.WordWrap = false;
            this.tbxLog.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbxLog_KeyDown);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(24, 92);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(449, 18);
            this.progressBar.TabIndex = 21;
            this.progressBar.Visible = false;
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(21, 284);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(225, 13);
            this.lblSize.TabIndex = 23;
            this.lblSize.Text = "At least 220 KB of free disk space is required.";
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 361);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.tbxLog);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblReq);
            this.Controls.Add(this.lblLong);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.grpDestination);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnInstall);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SetupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Max 4ds Tools Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetupForm_FormClosing);
            this.Load += new System.EventHandler(this.SetupForm_Load);
            this.grpDestination.ResumeLayout(false);
            this.grpDestination.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox tbxDestination;
        private System.Windows.Forms.GroupBox grpDestination;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblShort;
        private System.Windows.Forms.Label lblPage;
        private System.Windows.Forms.Label lblLong;
        private System.Windows.Forms.Label lblReq;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbxLog;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblSize;
    }
}

