namespace SandBox
{
    partial class Form_STID
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
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label.AutoEllipsis = true;
            this.label.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label.Location = new System.Drawing.Point(12, 9);
            this.label.Name = "label";
            this.label.Padding = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.label.Size = new System.Drawing.Size(555, 30);
            this.label.TabIndex = 5;
            this.label.Text = "Veuillez passer un badge...";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            
            // 
            // Form_STID
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 55);
            this.ControlBox = false;
            this.Controls.Add(this.label);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_STID";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_STID_FormClosed);
            this.Load += new System.EventHandler(this.Form_STID_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label label;
    }
}