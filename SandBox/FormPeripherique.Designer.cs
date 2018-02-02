namespace SandBox
{
	partial class FormPeripherique
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.label = new System.Windows.Forms.Label();
            this.boutonAnnuler = new System.Windows.Forms.Button();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label.AutoEllipsis = true;
            this.label.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label.Location = new System.Drawing.Point(12, 9);
            this.label.Name = "label";
            this.label.Padding = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.label.Size = new System.Drawing.Size(555, 20);
            this.label.TabIndex = 0;
            this.label.Text = "Veuillez passer le rondier...";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // boutonAnnuler
            // 
            this.boutonAnnuler.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonAnnuler.AutoSize = true;
            this.boutonAnnuler.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.boutonAnnuler.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.boutonAnnuler.Location = new System.Drawing.Point(104, 62);
            this.boutonAnnuler.Name = "boutonAnnuler";
            this.boutonAnnuler.Size = new System.Drawing.Size(369, 39);
            this.boutonAnnuler.TabIndex = 3;
            this.boutonAnnuler.Text = "Annuler";
            this.boutonAnnuler.UseVisualStyleBackColor = true;
            this.boutonAnnuler.Click += new System.EventHandler(this.boutonAnnuler_Click);
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(104, 33);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(369, 23);
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Visible = false;
            // 
            // FormPeripherique
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(579, 113);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label);
            this.Controls.Add(this.boutonAnnuler);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPeripherique";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " ";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLecture_FormClosed);
            this.Load += new System.EventHandler(this.FormLecture_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.Label label;
        private System.Windows.Forms.Button boutonAnnuler;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ProgressBar progressBar1;
	}
}