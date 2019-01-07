using System;
using System.Windows.Forms;
using log4net;

namespace SandBox
{
    public partial class Form_STID : Form
    {
        public WebBrowser webBrowser = null;
        public string port = null;
        public string idtechnostid = null;
        public Form_STID(WebBrowser _webBrowser, string _port,string _idtechnoStid)
        {
            InitializeComponent();

            this.webBrowser = _webBrowser;
            this.port = _port;
            this.idtechnostid = _idtechnoStid;
        }
        Timer formClose = new Timer();
        private void Form_STID_Load(object sender, EventArgs e)
        {
            label.Text = SandBox.Properties.Settings.Default.LibelleMessageLecteurPortSerie;

            STID.OpenConnexionSTID(this.port);
          
        }


        public void lecture(ILog logFichier)
        {
            logFichier.Debug("entrée lecture");
            string numeroBadge;
            logFichier.Debug("1 :" + this.idtechnostid == null ? "NULL" : this.idtechnostid);
            numeroBadge = STID.GetTagSTID(this.idtechnostid);
            logFichier.Debug("2");
            Application.DoEvents();

            if (!string.IsNullOrEmpty(numeroBadge))
            {
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(numeroBadge));
                    //webBrowser.Document.Window.Frames["ipopup"].Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                }
                else
                {
                    webBrowser.Document.GetElementById("lecteurPhysique").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(numeroBadge));
                    //webBrowser.Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                }
            }
            else
            {
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique").SetAttribute("value", "");
                }
                else
                {
                    webBrowser.Document.GetElementById("lecteurPhysique").SetAttribute("value", "");
                }
            }
            Close();
        }

        private void Form_STID_FormClosed(object sender, FormClosedEventArgs e)
        {
            STID.CloseConnexionSTID();
        }

      
    }
}
