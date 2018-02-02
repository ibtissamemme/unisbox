using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SandBox.Periph_Sign_Screen
{

    public partial class Form_Sign_Screen : Form
    {

        private WebBrowser WebBrowser;
        private Uri urlapp;

        private Point? _Previous = null;
        private Pen _Pen = new Pen(Color.Black);

        private Form_Sign_Screen_clone clone;

        public bool toClose { get; set; }

        public Form_Sign_Screen(WebBrowser _WebBrowser, Uri _urlapp, Icon iconApp)
        {
            InitializeComponent();

            WebBrowser = _WebBrowser;
            urlapp = _urlapp;

            this.Icon = iconApp;

            this.Text = WebBrowser.DocumentTitle;

            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.AllowWebBrowserDrop = false;

            this.StartPosition = FormStartPosition.Manual;

            //this.label1.Text = SandBox.Properties.Settings.Default.LibelleSignFR;
            //this.label2.Text = SandBox.Properties.Settings.Default.LibelleSignEN;

            //this.Show();
            this.Disactive();
        }

        public void Active(string pdf, string message)
        {
            this.textBox1.Visible = true;

            this.label3.Visible = true;
            this.pictureBox1.Visible = true;
            this.btn_Init.Visible = true;
            this.btn_OK.Visible = true;
            this.webBrowser1.Visible = true;

            if (pdf == "")
            {
                //this.Size = new Size(470, 324);

                this.textBox1.Visible = false;

                this.webBrowser1.Visible = false;
            }
            else
            {
                webBrowser1.Url = new Uri(pdf);
            }

            this.textBox1.Text = message.Replace(SandBox.Properties.Settings.Default.CharNewLine, Environment.NewLine);

            this.picImage.Visible = false;

            this.Focus();

            clone = new Form_Sign_Screen_clone(this);
            clone.Text = WebBrowser.DocumentTitle;
            clone.Icon = this.Icon;
            clone.StartPosition = FormStartPosition.Manual;
            //clone.Location = new Point(Screen.AllScreens[0].Bounds.Location.X + (Screen.AllScreens[0].Bounds.Width - clone.Width) / 2, Screen.AllScreens[0].Bounds.Location.Y + (Screen.AllScreens[0].Bounds.Height - clone.Height) / 2);
            clone.Location = new Point(WebBrowser.Parent.Bounds.Location.X + (WebBrowser.Parent.Bounds.Width - clone.Width) / 2, WebBrowser.Parent.Bounds.Location.Y + (WebBrowser.Parent.Bounds.Height - clone.Height) / 2);

            clone.Show();
            clone.TopMost = true;
        }

        public void Disactive()
        {
            if (clone != null)
            {
                clone.Close();
            }

            this.textBox1.Visible = false;

            this.label3.Visible = false;
            this.pictureBox1.Visible = false;
            this.pictureBox1.Image = null;
            this.btn_Init.Visible = false;
            this.btn_OK.Visible = false;
            this.webBrowser1.Visible = false;
            this.picImage.Visible = true;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_Previous != null)
            {
                if (pictureBox1.Image == null)
                {
                    Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.White);
                    }
                    pictureBox1.Image = bmp;
                }

                //using (Graphics g = pictureBox1.CreateGraphics())
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawLine(_Pen, _Previous.Value.X, _Previous.Value.Y, e.X, e.Y);
                }
                pictureBox1.Invalidate();
                _Previous = new Point(e.X, e.Y);
                clone.pictureBox1.Image = this.pictureBox1.Image;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _Previous = null;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _Previous = new Point(e.X, e.Y);
            pictureBox1_MouseMove(sender, e);
        }

        private void btn_Init_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            clone.pictureBox1.Image = null;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            //pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            Envoi_image(pictureBox1.Image);
            this.Disactive();
            clone.Close();
        }

        //Fonction: Envoie de l'image
        public bool Envoi_image(System.Drawing.Image image)
        {
            Program.LogFich.Info("[Sign_Screen_Manager] Envoi_image() - Envoie des données");
            bool b = true;

            MemoryStream mstImage = new MemoryStream();
            image.Save(mstImage, System.Drawing.Imaging.ImageFormat.Jpeg);

            byte[] data = mstImage.GetBuffer();
            mstImage.Dispose();
            //StringBuilder dataSB = new StringBuilder();
            //IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
            /*
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = Marshal.ReadByte(dataPtr, i);
                dataSB.Append(data[i]);
            }
            */

            try
            {
                Uri url = new Uri(urlapp.ToString() + "devices/TOPAZ_SIGPLUS.asp");

                //StringBuilder sb = new StringBuilder();
                //sb.Append("type_peripherique=").Append(HttpUtility.UrlEncode("TOPAZ_SIGPLUS", Encoding.ASCII));
                //sb.Append("&machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                //sb.Append("data=").Append(HttpUtility.UrlEncode(data.ToString(), Encoding.ASCII));
                //sb.Append("&datalen=").Append(HttpUtility.UrlEncode(data.Length.ToString(), Encoding.ASCII));

                string boundary = DateTime.Now.Ticks.ToString("x");

                MemoryStream memStream = new System.IO.MemoryStream();

                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

                string headerTemplate = "Content-Disposition: form-data; name=\"signature\"; filename=\"signature.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";
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
                adr.Append(this.WebBrowser.Url.Query);
                Uri adresse = new Uri(adr.ToString());

                //CookieAwareWebClient wc = new CookieAwareWebClient();
                //wc.UseDefaultCredentials = true;
                //wc.UploadData(adresse.ToString(), postBytes);
                //wc.Dispose();

                WebBrowser.Navigate(adresse, "", postBytes, "Content-Type: multipart/form-data; boundary=" + boundary + "\r\n" + "Content-Length: " + postBytes.Length + "\r\n" + "\r\n");

                Program.LogFich.Info("[Sign_Screen_Manager] Envoi_image() - Envoie termine - Trame envoyee = " + postBytes.ToString());
            }
            catch (Exception e)
            {
                b = false;

                Program.LogFich.Error("[Sign_Screen_Manager] Envoi_image() - Erreur d'envoie = " + e.ToString());
            }
            return b;
        }

        private void Form_Sign_Screen_FormClosed(object sender, FormClosedEventArgs e)
        {
            clone.Close();
        }

        private void Form_Sign_Screen_Load(object sender, EventArgs e)
        {
            this.toClose = false;
            try
            {
                this.Location = Screen.AllScreens[SandBox.Properties.Settings.Default.indexScreenSign].Bounds.Location;
            }
            catch (Exception ex)
            {
                this.Location = Screen.AllScreens[0].Bounds.Location;

                Program.LogFich.Error("[Sign_Screen_Manager] Index screen not found = " + ex.ToString());
                this.toClose = true;

            }

            this.WindowState = FormWindowState.Maximized;

            if (!String.IsNullOrEmpty(SandBox.Properties.Settings.Default.UrlImageScreenSign))
            {
                try
                {
                    this.picImage.Image = Image.FromFile(SandBox.Properties.Settings.Default.UrlImageScreenSign);

                    picImage.Size = this.picImage.Image.Size;
                    picImage.Anchor = AnchorStyles.None;
                    picImage.Location = new Point((this.ClientSize.Width - picImage.Width) / 2, (this.ClientSize.Height - picImage.Height) / 2);

                    picImage.Refresh();
                }
                catch (Exception ex)
                {
                    Program.LogFich.Error(ex.ToString());
                }
            }
        }


    }
}
