using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DirectX.Capture;

namespace SandBox.Periph_Webcam
{
    public partial class Form_webcam : Form
    {

        private Filters InputOptions = new DirectX.Capture.Filters();
        private Filter VideoInput = null;
        private Capture CaptureInfo = null;
        private Uri urlapp;
        private WebBrowser webBrowser;
        private Periph_Webcam_Manager perih_webcam;
        private Rectangle rect;
        private Pen pen = new Pen(Color.Red);
        private bool down = false;

        public Form_webcam(Uri _url, WebBrowser _webBrowser, Periph_Webcam_Manager _perih_webcam)
        {
            webBrowser = _webBrowser;
            urlapp = _url;
            perih_webcam = _perih_webcam;
            InitializeComponent();
            this.Text = webBrowser.Document.Title;

            this.pen.DashStyle = DashStyle.Dash;
        }

        private void Form_webcam_Load(object sender, EventArgs e)
        {
            PictureBox2.Visible = false;

            foreach (DirectX.Capture.Filter f in InputOptions.VideoInputDevices)
            {
                cb_video.Items.Add(f.Name);
            }
            if (cb_video.Items.Count > 0)
            {
                cb_video.SelectedIndex = 0;
            }

            cb_framesize.Sorted = false;

            cb_framesize.Items.Add("320 x 240");
            cb_framesize.Items.Add("640 x 480");
            cb_framesize.Items.Add("1280 x 720");
            cb_framesize.Items.Add("1920 x 1080");

            if (Program.webcam_resolution != -1)
            {
                cb_framesize.SelectedIndex = Program.webcam_resolution;
            }
            else
            {
                cb_framesize.SelectedIndex = 1;
            }

            btn_start.Enabled = true;
            btn_stop.Enabled = false;
            btn_capture.Enabled = false;
            btn_OK.Enabled = false;

            this.Configure();
            btn_start.Enabled = false;
            btn_stop.Enabled = true;
            btn_capture.Enabled = true;
            btn_OK.Enabled = false;
            btn_capture.Focus();
        }

        private void Configure()
        {
            try
            {
                if (cb_video.Items.Count < 1)
                    throw new Exception("Il n'y a pas de périphérique de capture vidéo branché au PC");
                cb_video.Enabled = false;
                cb_framesize.Enabled = false;

                this.VideoInput = this.InputOptions.VideoInputDevices[cb_video.SelectedIndex];

                this.CaptureInfo = new Capture(this.VideoInput, null);

                this.CaptureInfo.PreviewWindow = PictureBox1;

                string[] s = cb_framesize.SelectedItem.ToString().Split('x');
                Size size = new Size(int.Parse(s[0]), int.Parse(s[1]));
                this.CaptureInfo.FrameSize = size;
                PictureBox1.Size = size;
                PictureBox2.Size = size;

                cb_framesize.Enabled = false;

                this.Size = new Size(int.Parse(s[0]) + 30, int.Parse(s[1]) + 118);
                this.CenterToParent();

                this.CaptureInfo.RenderPreview();

                this.CaptureInfo.FrameCaptureComplete += new Capture.FrameCapHandler(CaptureInfo_FrameCaptureComplete);

                btn_start.Enabled = false;
                btn_stop.Enabled = true;
                btn_capture.Enabled = true;
                btn_OK.Enabled = false;
                btn_capture.Focus();
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[Form_webcam] Configure() - " + e.ToString());
                MessageBox.Show("Résolution non compatible");
                this.Stop();
            }
        }

        /// <summary>
        /// Is Handled when a capture has been successfully taken
        /// </summary>
        /// <param name="Frame">The picturebox that has been generated</param>
        void CaptureInfo_FrameCaptureComplete(PictureBox Frame)
        {
            PictureBox2.BackgroundImage = Frame.Image;

            PictureBox1.Visible = false;
            PictureBox2.Visible = true;

            rect = new Rectangle(new Point(105, 50), new Size(105, 135));

            PictureBox2.Refresh();
            CaptureInfo.PreviewWindow = null;
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            this.Stop();

            Program.webcam_resolution = cb_framesize.SelectedIndex;

            this.Configure();
        }

        private void Stop()
        {
            if (CaptureInfo != null)
            {
                CaptureInfo.Stop();
                CaptureInfo.Close();
                CaptureInfo.DisposeCapture();
                CaptureInfo.Dispose();
            }

            PictureBox1.Visible = true;
            PictureBox2.Visible = false;

            btn_start.Enabled = true;
            btn_stop.Enabled = false;
            btn_capture.Enabled = false;
            btn_OK.Enabled = false;
            btn_start.Focus();

            cb_video.Enabled = true;
            cb_framesize.Enabled = true;
        }

        private void btn_capture_Click(object sender, EventArgs e)
        {
            btn_start.Enabled = true;
            btn_stop.Enabled = true;
            btn_capture.Enabled = false;
            btn_OK.Enabled = true;
            btn_OK.Focus();

            try
            {
                CaptureInfo.CaptureFrame();
                //CaptureInfo.Stop();
            }
            catch (Exception exept)
            {
                Program.LogFich.Error("[Form_webcam] SnapShot_click() - " + exept.ToString());
            }
        }

        void PictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            //base.OnMouseUp(e);
            down = false;
            Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        void PictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (down == true && e.Button == MouseButtons.Left)
            {
                rect.Location = new Point(e.X - (rect.Width / 2), e.Y - (rect.Height / 2));
            }
            if (down == true && e.Button == MouseButtons.Right && (e.X - rect.Left) >= 10)
            {
                rect.Width = (e.X - rect.Left);
                rect.Height = (135) * (e.X - rect.Left) / 105;
            }
        }

        void PictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            //base.OnMouseDown(e);
            down = true;
            if (e.Button == MouseButtons.Left)
            {
                Cursor.Current = System.Windows.Forms.Cursors.SizeAll;
                rect.Location = new Point(e.X - (rect.Width / 2), e.Y - (rect.Height / 2));
            }
            if (e.Button == MouseButtons.Right)
            {
                Cursor.Current = System.Windows.Forms.Cursors.SizeNWSE;
            }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            //Création de la nouvelle image (x et y représente les dimensions)
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            //Utilisation d'un objet Graphics
            Graphics g = Graphics.FromImage(bmp);
            //Dimension et positionnement dans la nouvelle image
            Rectangle destRect = new Rectangle(0, 0, rect.Width, rect.Height);
            //Dessin de l'image avec les ... représentant l'image Bitmap source            
            g.DrawImage(PictureBox2.BackgroundImage, destRect, rect, GraphicsUnit.Pixel);
            //PictureBox2.Image = bmp;

            //envoi de l'image
            Envoi_image((Image)bmp);
            this.Close();
        }

        public bool Envoi_image(System.Drawing.Image image)
        {
            Program.LogFich.Info("[Form_webcam] Envoi_image() - Envoie des données");
            bool b = true;

            MemoryStream mstImage = new MemoryStream();
            image.Save(mstImage, System.Drawing.Imaging.ImageFormat.Jpeg);

            byte[] data = mstImage.GetBuffer();
            mstImage.Dispose();

            try
            {
                Uri url = new Uri(urlapp.ToString() + "devices/PERIPH_WEBCAM.asp");

                string boundary = DateTime.Now.Ticks.ToString("x");

                MemoryStream memStream = new System.IO.MemoryStream();

                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                string headerTemplate = "Content-Disposition: form-data; name=\"webcam\"; filename=\"webcam.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
                byte[] headerbytes = System.Text.Encoding.ASCII.GetBytes(headerTemplate);

                memStream.Write(boundarybytes, 0, boundarybytes.Length);
                memStream.Write(headerbytes, 0, headerbytes.Length);
                memStream.Write(data, 0, data.Length);
                memStream.Write(boundarybytes, 0, boundarybytes.Length);

                memStream.Position = 0;
                byte[] postBytes = memStream.GetBuffer();
                memStream.Dispose();

                string s = System.Text.Encoding.ASCII.GetString(postBytes);

                StringBuilder adr = new StringBuilder();
                adr.Append(url.ToString());
                adr.Append(this.webBrowser.Url.Query);

                string param;
                if (webBrowser.Document.GetElementById("ipopup") != null)
                {
                    if (this.webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("webcam_type").GetAttribute("value").ToString() == "1")
                    {
                        param = "&type=visiteur&id=" + this.webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("webcam_visiteurid").GetAttribute("value").ToString();
                    }
                    else
                    {
                        param = "&type=residant&id=" + this.webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("webcam_residantid").GetAttribute("value").ToString();
                    }
                }
                else
                {
                    if (this.webBrowser.Document.GetElementById("webcam_type").GetAttribute("value").ToString() == "1")
                    {
                        param = "&type=visiteur&id=" + this.webBrowser.Document.GetElementById("webcam_visiteurid").GetAttribute("value").ToString();
                    }
                    else
                    {
                        param = "&type=residant&id=" + this.webBrowser.Document.GetElementById("webcam_residantid").GetAttribute("value").ToString();
                    }
                }
                adr.Append(param);
                Uri adresse = new Uri(adr.ToString());
                webBrowser.Navigate(adresse, "", postBytes, "Content-Type: multipart/form-data; boundary=" + boundary + "\r\n" + "Content-Length: " + postBytes.Length + "\r\n" + "\r\n");

                Program.LogFich.Info("[Form_webcam] Envoi_image() - Envoie termine - Trame envoyee = " + postBytes.ToString());
            }
            catch (Exception e)
            {
                b = false;

                Program.LogFich.Error("[Form_webcam] Envoi_image() - Erreur d'envoi = " + e.ToString());
            }
            return b;
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void PictureBox2_Paint(object sender, PaintEventArgs e)
        {
            PictureBox2.Invalidate();
            e.Graphics.DrawRectangle(pen, rect);
        }

        private void Form_webcam_FormClosing(object sender, FormClosingEventArgs e)
        {
            //CaptureInfo.DisposeCapture();
            this.Stop();
            perih_webcam.Close();
        }
    }
}
