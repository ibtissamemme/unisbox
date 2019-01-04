namespace SandBox.Periph_Topaz
{
    partial class Form_Topaz
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Topaz));
            this.button1 = new System.Windows.Forms.Button();
            this.cmdCapture = new System.Windows.Forms.Button();
            this.cmdClear = new System.Windows.Forms.Button();
            this.sigPlusNET1 = new Topaz.SigPlusNET();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(233, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(30, 23);
            this.button1.TabIndex = 8;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cmdCapture
            // 
            this.cmdCapture.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdCapture.BackgroundImage")));
            this.cmdCapture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.cmdCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdCapture.Location = new System.Drawing.Point(233, 41);
            this.cmdCapture.Name = "cmdCapture";
            this.cmdCapture.Size = new System.Drawing.Size(30, 23);
            this.cmdCapture.TabIndex = 7;
            this.cmdCapture.UseVisualStyleBackColor = true;
            this.cmdCapture.Click += new System.EventHandler(this.cmdCapture_Click);
            // 
            // cmdClear
            // 
            this.cmdClear.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdClear.BackgroundImage")));
            this.cmdClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.cmdClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdClear.Location = new System.Drawing.Point(233, 12);
            this.cmdClear.Name = "cmdClear";
            this.cmdClear.Size = new System.Drawing.Size(30, 23);
            this.cmdClear.TabIndex = 6;
            this.cmdClear.UseVisualStyleBackColor = true;
            this.cmdClear.Click += new System.EventHandler(this.cmdClear_Click);
            // 
            // sigPlusNET1
            // 
            this.sigPlusNET1.Location = new System.Drawing.Point(12, 12);
            this.sigPlusNET1.Name = "sigPlusNET1";
            this.sigPlusNET1.Size = new System.Drawing.Size(215, 80);
            this.sigPlusNET1.TabIndex = 9;
            this.sigPlusNET1.Text = "sigPlusNET1";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form_Topaz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 104);
            this.Controls.Add(this.sigPlusNET1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmdCapture);
            this.Controls.Add(this.cmdClear);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Topaz";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_Topaz";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Topaz_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button cmdCapture;
        private System.Windows.Forms.Button cmdClear;
        private Topaz.SigPlusNET sigPlusNET1;
        private System.Windows.Forms.Timer timer1;
    }
}