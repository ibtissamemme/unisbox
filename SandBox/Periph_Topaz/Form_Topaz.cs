using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SandBox.Periph_Topaz
{
    public partial class Form_Topaz : Form
    {
        private String messageTitre;
        public bool estconnecte;
        private Uri urlapp;
        private WebBrowser WebBrowser;
        private Bitmap please;
        private Topaz_Manager topaz;

        //if mode == 0 mode : validation coté ordinateur
        //if mode == 1 mode : validation coté tablette
        public Form_Topaz(String _message, Uri _url, WebBrowser _WebBrowser, short mode, Topaz_Manager _topaz)
        {

            WebBrowser = _WebBrowser;
            urlapp = _url;
            topaz = _topaz;

            InitializeComponent();

            this.Text = WebBrowser.Document.Title;

            estconnecte = false;
            if (isConnected(sigPlusNET1))
            {
                estconnecte = true;
            }
            else
            {
                MessageBox.Show("La tablette a signature n'est pas connectée, connectez-la, et recommencez l'operation.");
                this.Close();
            }

            if (estconnecte)
            {
                if (mode == 0)
                {
                    messageTitre = _message;

                    Text = messageTitre;

                    //----------------------------------------------
                    //The following parameters are set in case the user's INI file is not correctly set up for an LCD 1X5 tablet
                    //Otherwise, if the INI is correctly set up, these parameters do not need to be set
                    sigPlusNET1.SetTabletXStart(400);
                    sigPlusNET1.SetTabletXStop(2400);
                    sigPlusNET1.SetTabletYStart(350);
                    sigPlusNET1.SetTabletYStop(1050);
                    sigPlusNET1.SetTabletLogicalXSize(2100);
                    sigPlusNET1.SetTabletLogicalYSize(700);
                    //******************************************************************'
                    //sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
                    //sigPlusNET1.LCDSetWindow(0, 0, 240, 64);
                    //sigPlusNET1.SetSigWindow(1, 0, 0, 240, 64);
                    //sigPlusNET1.KeyPadClearHotSpotList();
                    //sigPlusNET1.SetLCDCaptureMode(1);
                    //sigPlusNET1.SetTabletState(0);

                    Font f = new System.Drawing.Font("Arial", 9.0F, System.Drawing.FontStyle.Regular);

                    sigPlusNET1.ClearTablet();
                    sigPlusNET1.SetTabletState(1); //Turns tablet on to collect signature
                    sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
                    sigPlusNET1.LCDWriteString(0, 2, 0, 0, f, messageTitre);
                    sigPlusNET1.ClearSigWindow(1);
                    sigPlusNET1.LCDSetWindow(0, 22, 240, 40);
                    sigPlusNET1.SetSigWindow(1, 0, 22, 240, 40);
                    sigPlusNET1.SetLCDCaptureMode(2);
                    //
                    //-------------------------------------------------------------------------
                }

                if (mode == 1)
                {

                    this.cmdCapture.Visible = false;
                    this.cmdClear.Visible = false;

                    //----------------------------------------------
                    //The following parameters are set in case the user's INI file is not correctly set up for an LCD 1X5 tablet
                    //Otherwise, if the INI is correctly set up, these parameters do not need to be set
                    sigPlusNET1.SetTabletXStart(400);
                    sigPlusNET1.SetTabletXStop(2400);
                    sigPlusNET1.SetTabletYStart(350);
                    sigPlusNET1.SetTabletYStop(1050);
                    sigPlusNET1.SetTabletLogicalXSize(2100);
                    sigPlusNET1.SetTabletLogicalYSize(700);
                    //******************************************************************'

                    //The following code will write BMP images out to the LCD 1X5 screen

                    /*sign = new System.Drawing.Bitmap(SandBox.Properties.Resources.Sign);
                    ok = new System.Drawing.Bitmap(SandBox.Properties.Resources.OK);
                    clear = new System.Drawing.Bitmap(SandBox.Properties.Resources.CLEAR);*/
                    please = new System.Drawing.Bitmap(SandBox.Properties.Resources.please);

                    sigPlusNET1.SetTabletState(1); //Turns tablet on to collect signature
                    sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);

                    //Demo text
                    Font f = new System.Drawing.Font("verdana", 10.0F, System.Drawing.FontStyle.Regular);

                    //Images sent to the background
                    /*sigPlusNET1.LCDSendGraphic(1, 2, 0, 20, sign);
                    sigPlusNET1.LCDSendGraphic(1, 2, 207, 4, ok);
                    sigPlusNET1.LCDSendGraphic(1, 2, 15, 4, clear);*/

                    sigPlusNET1.LCDWriteString(1, 2, 202, 4, f, "Ok");
                    sigPlusNET1.LCDWriteString(1, 2, 15, 4, f, "Clear");

                    /*sigPlusNET1.LCDWriteString(0, 2, 45, 0, f, "Tap Continue to try signing.");
                    sigPlusNET1.LCDWriteString(0, 2, 15, 45, f, "Continue");
                    sigPlusNET1.LCDWriteString(0, 2, 200, 45, f, "Exit");

                    //Create the hot spots for the Continue and Exit buttons
                    sigPlusNET1.KeyPadAddHotSpot(0, 1, 12, 40, 40, 15); //For Continue button
                    sigPlusNET1.KeyPadAddHotSpot(1, 1, 195, 40, 20, 15); //For Exit button
                    */
                    sigPlusNET1.ClearTablet();

                    sigPlusNET1.LCDSetWindow(0, 0, 1, 1);
                    sigPlusNET1.SetSigWindow(1, 0, 0, 1, 1); //Sets the area where ink is permitted in the SigPlus object
                    sigPlusNET1.SetLCDCaptureMode(2);   //Sets mode so ink will not disappear after a few seconds
                    //-------------------------------------------------------------------------

                    sigPlusNET1.ClearSigWindow(1);
                    sigPlusNET1.LCDRefresh(1, 16, 45, 50, 15); //Refresh LCD at 'Continue' to indicate to user that this option has been sucessfully chosen
                    sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64); //Brings the background image already loaded into foreground
                    sigPlusNET1.ClearTablet();
                    sigPlusNET1.KeyPadClearHotSpotList();
                    sigPlusNET1.KeyPadAddHotSpot(2, 1, 10, 5, 53, 17); //For CLEAR button
                    sigPlusNET1.KeyPadAddHotSpot(3, 1, 200, 4, 40, 17); //For OK button sigPlusNET1.KeyPadAddHotSpot(3, 1, 197, 5, 19, 17);
                    sigPlusNET1.LCDSetWindow(0, 22, 238, 40);
                    sigPlusNET1.SetSigWindow(1, 0, 22, 240, 40); //Sets the area where ink is permitted in the SigPlus object
                    sigPlusNET1.SetLCDCaptureMode(2);   //Sets mode so ink will not disappear after a few seconds
                    timer1.Enabled = true;
                }
            }
        }

        /*Cette fonction retourn vrai si le sigplusNET est connecté et faux sinon*/
        /*Use the TabletComTest = 1. This is the preferred method in version 3.00 and above instead of the older TabletMode = Add 128 mode. In this mode, If a Topaz tablet is plugged into the selected COM port, the TabletState can be set to 1. If tablet is not plugged into serial port, TabletState cannot be set to 1. A tablet plug-in detection scheme is shown below and assumes that the COM port was set in the Sigplus.ini file during install. */
        private bool isConnected(Topaz.SigPlusNET sigplus)
        {

            sigplus.SetTabletState(0);
            sigplus.SetTabletComTest(1);
            sigplus.SetTabletState(1);
            int TabletConnect = sigplus.GetTabletState();

            if (TabletConnect == 1)
                return true;
            return false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (estconnecte)
            {
                //reset hardware
                sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
                sigPlusNET1.LCDSetWindow(0, 0, 240, 64);
                sigPlusNET1.SetSigWindow(1, 0, 0, 240, 64);
                sigPlusNET1.KeyPadClearHotSpotList();
                sigPlusNET1.SetLCDCaptureMode(1);
                sigPlusNET1.SetTabletState(0);
                sigPlusNET1.ClearTablet();
            }
            else
            {
                //Exception pasconnecte = new Exception();
                //throw pasconnecte;
            }
        }

        private void cmdClear_Click(object sender, EventArgs e)
        {
            sigPlusNET1.ClearSigWindow(1);
            sigPlusNET1.ClearTablet();
            sigPlusNET1.LCDRefresh(0, 0, 22, 240, 64);
        }

        private void enregistrer_et_quitter()
        {

            string strSig;

            sigPlusNET1.ClearSigWindow(1);
            strSig = sigPlusNET1.GetSigString();

            sigPlusNET1.SetLCDCaptureMode(1);
            sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);

            try
            {
                sigPlusNET1.SetTabletState(0);
                sigPlusNET1.SetImageFileFormat(4);
                sigPlusNET1.SetJustifyMode(5);
                sigPlusNET1.SetImageXSize(600);
                sigPlusNET1.SetImageYSize(200);
                sigPlusNET1.SetImagePenWidth(8);
                sigPlusNET1.SetDisplayPenWidth(11);
                sigPlusNET1.SetJustifyX(10);
                sigPlusNET1.SetJustifyY(10);
                //sigPlusNET1.GetSigImage().Save(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "signature.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                Envoi_image(sigPlusNET1.GetSigImage());
                //sigPlusNET1.GetSigImage().Save("TUTU.BMP",ImageFormat.Jpeg);
                //sigPlusNET1.GetSigImage().Save("TUTU.JPG", System.Drawing.Imaging.ImageFormat.Jpeg);
                //sigPlusNET1.GetSigImage().Save("TBTB.BMP", System.Drawing.Imaging.ImageFormat.Bmp);
                this.Close();
            }
            catch (Exception err)
            {
                Program.LogFich.Error(err.ToString());
                MessageBox.Show(err.ToString());

            }

            //reset hardware
            sigPlusNET1.SetTabletState(0);
            //Program.retourAppli = "-1";
            this.Close();
        }

        private void cmdCapture_Click(object sender, EventArgs e)
        {
            enregistrer_et_quitter();
        }

        private void button1_Click(object sender, EventArgs e)
        {   //reset hardware
            sigPlusNET1.ClearTablet();
            sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
            sigPlusNET1.SetTabletState(0);
            this.Close();

        }

        //Fonction: Envoie de l'image
        public bool Envoi_image(System.Drawing.Image image)
        {
            Program.LogFich.Info("[Topaz_Manager] Envoi_image() - Envoie des données");
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
                WebBrowser.Navigate(adresse, "", postBytes, "Content-Type: multipart/form-data; boundary=" + boundary + "\r\n" + "Content-Length: " + postBytes.Length + "\r\n" + "\r\n");

                Program.LogFich.Info("[Topaz_Manager] Envoi_image() - Envoie termine - Trame envoyee = " + postBytes.ToString());
            }
            catch (Exception e)
            {
                b = false;

                Program.LogFich.Error("[Topaz_Manager] Envoi_image() - Erreur d'envoie = " + e.ToString());
            }
            return b;
        }

        private void Form_Topaz_FormClosed(object sender, FormClosedEventArgs e)
        {
            topaz.delete_sigplus();
            topaz = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string strSig;

            if (sigPlusNET1.KeyPadQueryHotSpot(2) > 0) //If the CLEAR hotspot is tapped, then...
            {
                sigPlusNET1.ClearSigWindow(1);
                timer1.Enabled = false;
                sigPlusNET1.LCDRefresh(1, 10, 0, 53, 17); //Refresh LCD at 'CLEAR' to indicate to user that this option has been sucessfully chosen
                sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64); //Brings the background image already loaded into foreground
                sigPlusNET1.ClearTablet();
                sigPlusNET1.ClearSigWindow(1);
                timer1.Enabled = true;
            }

            if (sigPlusNET1.KeyPadQueryHotSpot(3) > 0) //If the OK hotspot is tapped, then...
            {
                sigPlusNET1.ClearSigWindow(1);
                timer1.Enabled = false;

                /*********************Two ways to save the signature*
                *************************************'
                Method 1--storing as an ASCII string value*/
                strSig = sigPlusNET1.GetSigString();
                /*the strSig String variable now holds the signature as a long ASCII string.
                this can be stored as desired, in a database, etc.

                Method 2--storing as a SIG file on the hard drive
                sigPlusNET1.ExportSigFile "C:\SigFile1.sig"
                The commented-out function above will export the signature to the SIG file
                specified (in this case C:\SigFile1.sig, saving the signature as a file on your hardrive
                *****************************************************************************************/

                sigPlusNET1.LCDRefresh(1, 210, 3, 14, 14); //Refresh LCD at 'OK' to indicate to user that this option has been sucessfully chosen

                if (sigPlusNET1.NumberOfTabletPoints() > 10)
                {
                    //MessageBox.Show("Thank you for signing.");

                    enregistrer_et_quitter();
                }
                else
                {
                    sigPlusNET1.LCDRefresh(0, 0, 0, 240, 64);
                    sigPlusNET1.LCDSendGraphic(0, 2, 4, 20, please);
                    System.Threading.Thread.Sleep(750);
                    sigPlusNET1.ClearTablet();
                    sigPlusNET1.LCDRefresh(2, 0, 0, 240, 64);
                    sigPlusNET1.SetLCDCaptureMode(2);   //Sets mode so ink will not disappear after a few seconds
                    timer1.Enabled = true;
                    sigPlusNET1.ClearSigWindow(1);
                }
            }

        }

        //Désactivation de la croix
        private const int CS_NOCLOSE = 0x0200;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }
    }
}
