using System;
using System.Windows.Forms;

namespace SandBox
{
    public partial class Form_MIL100 : Form
    {
        public WebBrowser webBrowser = null;
        public string port = null;

        public Form_MIL100(WebBrowser _webBrowser, string _port)
        {
            InitializeComponent();

            this.webBrowser = _webBrowser;
            this.port = _port;
        }

        private void Form_MIL100_Load(object sender, EventArgs e)
        {
            label.Text = SandBox.Properties.Settings.Default.LibelleMessageLecteurPortSerie;

            MIL100.OpenConnexionMIL100(this.port);
        }

        public void lecture()
        {
            string numeroBadge;
            numeroBadge = MIL100.GetTagMIL100();
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

        private void Form_MIL100_FormClosed(object sender, FormClosedEventArgs e)
        {
            MIL100.CloseConnexionMIL100();
        }
    }
}
