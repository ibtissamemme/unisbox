namespace SandBox
{
	partial class Main
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.LabelProductName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.LabelVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.LabelCopyright = new System.Windows.Forms.ToolStripStatusLabel();
            this.LabelStatusMachineName = new System.Windows.Forms.ToolStripStatusLabel();
            this.logoPeriph = new System.Windows.Forms.ToolStripStatusLabel();
            this.labelStatusApplication = new System.Windows.Forms.ToolStripStatusLabel();
            this.LabelSiteName = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.LabelSiteVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LabelProductName,
            this.toolStripStatusLabel1,
            this.LabelVersion,
            this.LabelCopyright,
            this.LabelStatusMachineName,
            this.logoPeriph,
            this.labelStatusApplication,
            this.LabelSiteName,
            this.toolStripStatusLabel2,
            this.LabelSiteVersion});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new System.Drawing.Size(2688, 50);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip";
            // 
            // LabelProductName
            // 
            this.LabelProductName.Margin = new System.Windows.Forms.Padding(10, 3, 10, 2);
            this.LabelProductName.Name = "LabelProductName";
            this.LabelProductName.Size = new System.Drawing.Size(125, 45);
            this.LabelProductName.Text = "UnisBox";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(30, 45);
            this.toolStripStatusLabel1.Text = "-";
            // 
            // LabelVersion
            // 
            this.LabelVersion.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.LabelVersion.Margin = new System.Windows.Forms.Padding(10, 3, 10, 2);
            this.LabelVersion.Name = "LabelVersion";
            this.LabelVersion.Size = new System.Drawing.Size(120, 45);
            this.LabelVersion.Text = "Version";
            // 
            // LabelCopyright
            // 
            this.LabelCopyright.AutoSize = false;
            this.LabelCopyright.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.LabelCopyright.Name = "LabelCopyright";
            this.LabelCopyright.Size = new System.Drawing.Size(200, 45);
            this.LabelCopyright.Text = "Copyright © SafeWare 2010";
            // 
            // LabelStatusMachineName
            // 
            this.LabelStatusMachineName.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.LabelStatusMachineName.Name = "LabelStatusMachineName";
            this.LabelStatusMachineName.Size = new System.Drawing.Size(214, 45);
            this.LabelStatusMachineName.Text = "MachineName";
            this.LabelStatusMachineName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // logoPeriph
            // 
            this.logoPeriph.ForeColor = System.Drawing.Color.Blue;
            this.logoPeriph.Name = "logoPeriph";
            this.logoPeriph.Size = new System.Drawing.Size(0, 45);
            this.logoPeriph.Tag = "";
            // 
            // labelStatusApplication
            // 
            this.labelStatusApplication.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.labelStatusApplication.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.labelStatusApplication.Name = "labelStatusApplication";
            this.labelStatusApplication.Padding = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.labelStatusApplication.Size = new System.Drawing.Size(238, 45);
            this.labelStatusApplication.Text = "Status : None";
            // 
            // LabelSiteName
            // 
            this.LabelSiteName.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LabelSiteName.Name = "LabelSiteName";
            this.LabelSiteName.Size = new System.Drawing.Size(216, 45);
            this.LabelSiteName.Text = "LabelSiteName";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(30, 45);
            this.toolStripStatusLabel2.Text = "-";
            // 
            // LabelSiteVersion
            // 
            this.LabelSiteVersion.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LabelSiteVersion.Name = "LabelSiteVersion";
            this.LabelSiteVersion.Size = new System.Drawing.Size(235, 45);
            this.LabelSiteVersion.Text = "LabelSiteVersion";
            // 
            // webBrowser
            // 
            this.webBrowser.AllowWebBrowserDrop = false;
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.Location = new System.Drawing.Point(0, 0);
            this.webBrowser.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.webBrowser.MinimumSize = new System.Drawing.Size(2133, 1431);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(2688, 1698);
            this.webBrowser.TabIndex = 0;
            this.webBrowser.Url = new System.Uri("", System.UriKind.Relative);
            this.webBrowser.WebBrowserShortcutsEnabled = false;
            this.webBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser_DocumentCompleted);
            this.webBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webBrowser_Navigated);
            this.webBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.webBrowser_Navigating);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.webBrowser);
            this.toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(2688, 1691);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(2688, 1741);
            this.toolStripContainer1.TabIndex = 2;
            this.toolStripContainer1.Text = "toolStripContainer1";
            this.toolStripContainer1.TopToolStripPanelVisible = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(2688, 1741);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.MinimumSize = new System.Drawing.Size(2677, 1609);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Tag = "0";
            this.Text = "Safeware";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel LabelCopyright;
        private System.Windows.Forms.ToolStripStatusLabel LabelVersion;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel LabelSiteName;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel LabelSiteVersion;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.ToolStripStatusLabel LabelProductName;
        private System.Windows.Forms.ToolStripStatusLabel labelStatusApplication;
		private System.Windows.Forms.ToolStripStatusLabel logoPeriph;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripStatusLabel LabelStatusMachineName;
	}
}

