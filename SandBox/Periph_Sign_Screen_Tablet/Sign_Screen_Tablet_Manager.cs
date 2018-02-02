using System;
using System.Windows.Forms;
using System.Drawing;

namespace SandBox.Periph_Sign_Screen_Tablet
{
    public class Sign_Screen_Tablet_Manager
    {
        private WebBrowser WebBrowser;
        private Uri Url;
        private Form_Sign_Screen_Tablet Sign_Screen;
        private Icon iconApp;
        public event ChangeLabelStatusEventArgs ChangeLabelStatus;

        public Sign_Screen_Tablet_Manager(WebBrowser _webBrowser, Uri _url, Icon _iconApp)
        {
            WebBrowser = _webBrowser;
            Url = _url;
            iconApp = _iconApp;
        }

        public void Lecture_signature(string message)
        {
            Program.LogFich.Info("[Sign_Screen_Tablet_Manager] Debut => Lecture_signature()");
            if (Sign_Screen != null)
            {
                Sign_Screen.Active(message);
            }
            else
            {
                this.Show();
                Sign_Screen.Active(message);
            }

            Program.LogFich.Info("[Sign_Screen_Tablet_Manager] Fin => Lecture_signature()");

        }

        public void delete_sigplus()
        {
            this.ChangeLabelStatus("Status : None");
            if (Sign_Screen != null)
            {
                Sign_Screen.Dispose();
            }
        }

        public void Show()
        {
            Sign_Screen = new Form_Sign_Screen_Tablet(WebBrowser, Url, iconApp);

            Sign_Screen.Show();
        }
    }
}
