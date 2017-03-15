namespace MasterFudgeMk2
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.ofdOpenRom = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bootWithoutMediaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslFps = new System.Windows.Forms.ToolStripStatusLabel();
            this.scScreen = new MasterFudgeMk2.Common.ScreenControl();
            this.takeScreenshotToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.pauseToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.resetToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.infoToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.limitFPSToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.muteSoundToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.keepAspectRatioToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.autoResizeWindowToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.configToolStripMenuItem = new MasterFudgeMk2.Common.BindableToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ofdOpenRom
            // 
            this.ofdOpenRom.Title = "Open ROM";
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.emulationToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(512, 24);
            this.menuStrip.TabIndex = 6;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openROMToolStripMenuItem,
            this.bootWithoutMediaToolStripMenuItem,
            this.toolStripMenuItem6,
            this.recentFilesToolStripMenuItem,
            this.toolStripMenuItem1,
            this.takeScreenshotToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openROMToolStripMenuItem
            // 
            this.openROMToolStripMenuItem.Name = "openROMToolStripMenuItem";
            this.openROMToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openROMToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.openROMToolStripMenuItem.Text = "&Open ROM";
            this.openROMToolStripMenuItem.Click += new System.EventHandler(this.openROMToolStripMenuItem_Click);
            // 
            // bootWithoutMediaToolStripMenuItem
            // 
            this.bootWithoutMediaToolStripMenuItem.Name = "bootWithoutMediaToolStripMenuItem";
            this.bootWithoutMediaToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.bootWithoutMediaToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.bootWithoutMediaToolStripMenuItem.Text = "&Boot Without Media...";
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(230, 6);
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearListToolStripMenuItem,
            this.toolStripMenuItem7});
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            this.recentFilesToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.recentFilesToolStripMenuItem.Text = "&Recent Files...";
            // 
            // clearListToolStripMenuItem
            // 
            this.clearListToolStripMenuItem.Name = "clearListToolStripMenuItem";
            this.clearListToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.clearListToolStripMenuItem.Text = "&Clear List";
            this.clearListToolStripMenuItem.Click += new System.EventHandler(this.clearListToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(149, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(230, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(230, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // emulationToolStripMenuItem
            // 
            this.emulationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pauseToolStripMenuItem,
            this.resetToolStripMenuItem,
            this.toolStripMenuItem5,
            this.infoToolStripMenuItem});
            this.emulationToolStripMenuItem.Name = "emulationToolStripMenuItem";
            this.emulationToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.emulationToolStripMenuItem.Text = "&Emulation";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(145, 6);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.limitFPSToolStripMenuItem,
            this.muteSoundToolStripMenuItem,
            this.toolStripMenuItem3,
            this.keepAspectRatioToolStripMenuItem,
            this.autoResizeWindowToolStripMenuItem,
            this.toolStripMenuItem4,
            this.configToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(181, 6);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(181, 6);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslStatus,
            this.tsslFps});
            this.statusStrip.Location = new System.Drawing.Point(0, 408);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(512, 22);
            this.statusStrip.TabIndex = 8;
            // 
            // tsslStatus
            // 
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Size = new System.Drawing.Size(475, 17);
            this.tsslStatus.Spring = true;
            this.tsslStatus.Text = "---";
            this.tsslStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsslFps
            // 
            this.tsslFps.Name = "tsslFps";
            this.tsslFps.Size = new System.Drawing.Size(22, 17);
            this.tsslFps.Text = "---";
            // 
            // scScreen
            // 
            this.scScreen.BackColor = System.Drawing.Color.Black;
            this.scScreen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scScreen.Location = new System.Drawing.Point(0, 24);
            this.scScreen.Name = "scScreen";
            this.scScreen.Size = new System.Drawing.Size(512, 384);
            this.scScreen.TabIndex = 0;
            this.scScreen.TabStop = false;
            this.scScreen.Viewport = new System.Drawing.Rectangle(0, 0, 0, 0);
            // 
            // takeScreenshotToolStripMenuItem
            // 
            this.takeScreenshotToolStripMenuItem.Name = "takeScreenshotToolStripMenuItem";
            this.takeScreenshotToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.takeScreenshotToolStripMenuItem.Text = "&Take Screenshot";
            this.takeScreenshotToolStripMenuItem.Click += new System.EventHandler(this.takeScreenshotToolStripMenuItem_Click);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.CheckOnClick = true;
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.pauseToolStripMenuItem.Text = "&Pause";
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.resetToolStripMenuItem.Text = "&Reset";
            this.resetToolStripMenuItem.Click += new System.EventHandler(this.resetToolStripMenuItem_Click);
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.infoToolStripMenuItem.Text = "&Info";
            this.infoToolStripMenuItem.Click += new System.EventHandler(this.infoToolStripMenuItem_Click);
            // 
            // limitFPSToolStripMenuItem
            // 
            this.limitFPSToolStripMenuItem.CheckOnClick = true;
            this.limitFPSToolStripMenuItem.Name = "limitFPSToolStripMenuItem";
            this.limitFPSToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.limitFPSToolStripMenuItem.Text = "&Limit FPS";
            // 
            // muteSoundToolStripMenuItem
            // 
            this.muteSoundToolStripMenuItem.CheckOnClick = true;
            this.muteSoundToolStripMenuItem.Name = "muteSoundToolStripMenuItem";
            this.muteSoundToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.muteSoundToolStripMenuItem.Text = "&Mute Sound";
            // 
            // keepAspectRatioToolStripMenuItem
            // 
            this.keepAspectRatioToolStripMenuItem.CheckOnClick = true;
            this.keepAspectRatioToolStripMenuItem.Name = "keepAspectRatioToolStripMenuItem";
            this.keepAspectRatioToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.keepAspectRatioToolStripMenuItem.Text = "&Keep Aspect Ratio";
            // 
            // autoResizeWindowToolStripMenuItem
            // 
            this.autoResizeWindowToolStripMenuItem.CheckOnClick = true;
            this.autoResizeWindowToolStripMenuItem.Name = "autoResizeWindowToolStripMenuItem";
            this.autoResizeWindowToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.autoResizeWindowToolStripMenuItem.Text = "Auto-&Resize Window";
            // 
            // configToolStripMenuItem
            // 
            this.configToolStripMenuItem.Name = "configToolStripMenuItem";
            this.configToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.configToolStripMenuItem.Text = "&Configuration...";
            this.configToolStripMenuItem.Click += new System.EventHandler(this.configToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 430);
            this.Controls.Add(this.scScreen);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "---";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.ScreenControl scScreen;
        private System.Windows.Forms.OpenFileDialog ofdOpenRom;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openROMToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private Common.BindableToolStripMenuItem takeScreenshotToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private Common.BindableToolStripMenuItem limitFPSToolStripMenuItem;
        private Common.BindableToolStripMenuItem keepAspectRatioToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private Common.BindableToolStripMenuItem configToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel tsslFps;
        private Common.BindableToolStripMenuItem muteSoundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emulationToolStripMenuItem;
        private Common.BindableToolStripMenuItem pauseToolStripMenuItem;
        private Common.BindableToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
        private Common.BindableToolStripMenuItem infoToolStripMenuItem;
        private Common.BindableToolStripMenuItem autoResizeWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem bootWithoutMediaToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
    }
}

