using System.Windows.Forms;
using System.IO;
using System;
using System.Text;
using System.Drawing;

namespace SandBox.Periph_Sign_Screen_Tablet
{
    public partial class Form_Sign_Screen_Tablet : Form
    {
        private WebBrowser WebBrowser;
        private Uri urlapp;

        private Point? _Previous = null;
        private Pen _Pen = new Pen(Color.Black);

        public Form_Sign_Screen_Tablet(WebBrowser _WebBrowser, Uri _urlapp, Icon iconApp)
        {
            WebBrowser = _WebBrowser;
            urlapp = _urlapp;

            this.Icon = iconApp;

            InitializeComponent();

        }




        private void btn_Close_Click(object sender, System.EventArgs e)
        {
            this.Close();

        }

        private void btn_Init_Click(object sender, System.EventArgs e)
        {
            pictureBox1.Image = null;
        }

        private void btn_OK_Click(object sender, System.EventArgs e)
        {
            Envoi_image(pictureBox1.Image);
            //this.Disactive();
            Close();
        }

        public void Active(string message)
        {

            this.pictureBox1.Visible = true;
            this.btn_Init.Visible = true;
            this.btn_OK.Visible = true;

            //this.textBox1.Text = message.Replace(SandBox.Properties.Settings.Default.CharNewLine, Environment.NewLine);

            //this.picImage.Visible = false;

            this.Focus();
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

        private void Form_Sign_Screen_Tablet_Load(object sender, EventArgs e)
        {         

            //this.WindowState = FormWindowState.Maximized;

            this.CenterToScreen();
        }

        private void pictureBox1_MouseDown_1(object sender, MouseEventArgs e)
        {
            _Previous = new Point(e.X, e.Y);
            pictureBox1_MouseMove_1(sender, e);
        }

        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
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
                //clone.pictureBox1.Image = this.pictureBox1.Image;
            }
        }

        private void pictureBox1_MouseUp_1(object sender, MouseEventArgs e)
        {
            _Previous = null;
        }
    }
}
