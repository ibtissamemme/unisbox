namespace SandBox.Periph_Webcam
{
	partial class Form_webcam
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_webcam));
            this.label_video = new System.Windows.Forms.Label();
            this.cb_video = new System.Windows.Forms.ComboBox();
            this.cb_framesize = new System.Windows.Forms.ComboBox();
            this.label_framesize = new System.Windows.Forms.Label();
            this.btn_stop = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.PictureBox2 = new System.Windows.Forms.PictureBox();
            this.btn_capture = new System.Windows.Forms.Button();
            this.btn_start = new System.Windows.Forms.Button();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label_video
            // 
            this.label_video.AutoSize = true;
            this.label_video.Location = new System.Drawing.Point(12, 9);
            this.label_video.Name = "label_video";
            this.label_video.Size = new System.Drawing.Size(75, 13);
            this.label_video.TabIndex = 13;
            this.label_video.Text = "Video source :";
            // 
            // cb_video
            // 
            this.cb_video.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_video.FormattingEnabled = true;
            this.cb_video.Location = new System.Drawing.Point(12, 25);
            this.cb_video.Name = "cb_video";
            this.cb_video.Size = new System.Drawing.Size(183, 21);
            this.cb_video.Sorted = true;
            this.cb_video.TabIndex = 12;
            // 
            // cb_framesize
            // 
            this.cb_framesize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_framesize.FormattingEnabled = true;
            this.cb_framesize.Location = new System.Drawing.Point(201, 25);
            this.cb_framesize.Name = "cb_framesize";
            this.cb_framesize.Size = new System.Drawing.Size(183, 21);
            this.cb_framesize.Sorted = true;
            this.cb_framesize.TabIndex = 19;
            // 
            // label_framesize
            // 
            this.label_framesize.AutoSize = true;
            this.label_framesize.Location = new System.Drawing.Point(198, 9);
            this.label_framesize.Name = "label_framesize";
            this.label_framesize.Size = new System.Drawing.Size(90, 13);
            this.label_framesize.TabIndex = 20;
            this.label_framesize.Text = "Video frame size :";
            // 
            // btn_stop
            // 
            this.btn_stop.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_stop.Image = ((System.Drawing.Image)(resources.GetObject("btn_stop.Image")));
            this.btn_stop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_stop.Location = new System.Drawing.Point(247, 538);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(76, 24);
            this.btn_stop.TabIndex = 1;
            this.btn_stop.Text = "Stop";
            this.btn_stop.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_OK.Image = ((System.Drawing.Image)(resources.GetObject("btn_OK.Image")));
            this.btn_OK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_OK.Location = new System.Drawing.Point(411, 538);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(74, 24);
            this.btn_OK.TabIndex = 3;
            this.btn_OK.Text = "OK";
            this.btn_OK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // PictureBox2
            // 
            this.PictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBox2.Location = new System.Drawing.Point(12, 52);
            this.PictureBox2.Name = "PictureBox2";
            this.PictureBox2.Size = new System.Drawing.Size(640, 480);
            this.PictureBox2.TabIndex = 18;
            this.PictureBox2.TabStop = false;
            this.PictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox2_Paint);
            this.PictureBox2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox2_MouseDown);
            this.PictureBox2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox2_MouseMove);
            this.PictureBox2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox2_MouseUp);
            // 
            // btn_capture
            // 
            this.btn_capture.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_capture.Image = ((System.Drawing.Image)(resources.GetObject("btn_capture.Image")));
            this.btn_capture.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_capture.Location = new System.Drawing.Point(329, 538);
            this.btn_capture.Name = "btn_capture";
            this.btn_capture.Size = new System.Drawing.Size(76, 24);
            this.btn_capture.TabIndex = 2;
            this.btn_capture.Text = "Capture";
            this.btn_capture.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_capture.UseVisualStyleBackColor = true;
            this.btn_capture.Click += new System.EventHandler(this.btn_capture_Click);
            // 
            // btn_start
            // 
            this.btn_start.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_start.Image = ((System.Drawing.Image)(resources.GetObject("btn_start.Image")));
            this.btn_start.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btn_start.Location = new System.Drawing.Point(165, 538);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new System.Drawing.Size(76, 24);
            this.btn_start.TabIndex = 0;
            this.btn_start.Text = "Start";
            this.btn_start.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.PictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBox1.Location = new System.Drawing.Point(12, 52);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(640, 480);
            this.PictureBox1.TabIndex = 11;
            this.PictureBox1.TabStop = false;
            // 
            // Form_webcam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 570);
            this.Controls.Add(this.label_framesize);
            this.Controls.Add(this.cb_framesize);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.PictureBox2);
            this.Controls.Add(this.btn_capture);
            this.Controls.Add(this.btn_start);
            this.Controls.Add(this.label_video);
            this.Controls.Add(this.cb_video);
            this.Controls.Add(this.PictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 598);
            this.Name = "Form_webcam";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form_webcam";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_webcam_FormClosing);
            this.Load += new System.EventHandler(this.Form_webcam_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btn_OK;
		private System.Windows.Forms.PictureBox PictureBox2;
        private System.Windows.Forms.Button btn_capture;
		private System.Windows.Forms.Button btn_start;
		private System.Windows.Forms.Label label_video;
		private System.Windows.Forms.ComboBox cb_video;
		private System.Windows.Forms.PictureBox PictureBox1;		
        private System.Windows.Forms.Button btn_stop;
        private System.Windows.Forms.ComboBox cb_framesize;
        private System.Windows.Forms.Label label_framesize;
	}
}