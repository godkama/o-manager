namespace o_manager
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnExport = new Button();
            btnLoad = new Button();
            btnRestore = new Button();
            txtLog = new TextBox();
            cmbConfigs = new ComboBox();
            SuspendLayout();
            // 
            // btnExport
            // 
            btnExport.Location = new Point(12, 148);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(259, 39);
            btnExport.TabIndex = 0;
            btnExport.Text = "Export Current Configuration";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(12, 193);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(259, 39);
            btnLoad.TabIndex = 0;
            btnLoad.Text = "Load Configuration";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnRestore
            // 
            btnRestore.Location = new Point(12, 238);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(259, 39);
            btnRestore.TabIndex = 0;
            btnRestore.Text = "Restore a Backup";
            btnRestore.UseVisualStyleBackColor = true;
            btnRestore.Click += btnRestore_Click;
            // 
            // txtLog
            // 
            txtLog.AcceptsReturn = true;
            txtLog.Location = new Point(12, 12);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(259, 72);
            txtLog.TabIndex = 1;
            txtLog.TextChanged += txtLog_TextChanged;
            // 
            // cmbConfigs
            // 
            cmbConfigs.FormattingEnabled = true;
            cmbConfigs.Location = new Point(12, 119);
            cmbConfigs.Name = "cmbConfigs";
            cmbConfigs.Size = new Size(259, 23);
            cmbConfigs.TabIndex = 2;
            cmbConfigs.SelectedIndexChanged += cmbConfigs_SelectedIndexChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(283, 289);
            Controls.Add(cmbConfigs);
            Controls.Add(txtLog);
            Controls.Add(btnRestore);
            Controls.Add(btnLoad);
            Controls.Add(btnExport);
            Name = "MainForm";
            Text = "o!manager";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnExport;
        private Button btnLoad;
        private Button btnRestore;
        private TextBox txtLog;
        private ComboBox cmbConfigs;
    }
}
