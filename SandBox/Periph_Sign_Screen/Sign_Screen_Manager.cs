using System;
using System.Windows.Forms;
using System.Drawing;

namespace SandBox.Periph_Sign_Screen
{
    public class Sign_Screen_Manager
    {
        private WebBrowser WebBrowser;
        private Uri Url;
        public Form_Sign_Screen Sign_Screen;
        private Icon iconApp;
        public event ChangeLabelStatusEventArgs ChangeLabelStatus;

        public Sign_Screen_Manager(WebBrowser _webBrowser, Uri _url, Icon _iconApp)
        {
            WebBrowser = _webBrowser;
            Url = _url;
            iconApp = _iconApp;
        }

        public void Lecture_signature(string pdf, string message)
        {
            Program.LogFich.Info("[Sign_Screen_Manager] Debut => Lecture_signature()");
            if (Sign_Screen != null)
            {
                Sign_Screen.Active(pdf, message);
            }
            else
            {
                /*this.Show();
                Sign_Screen.Active(pdf, message);*/
            }

            Program.LogFich.Info("[Sign_Screen_Manager] Fin => Lecture_signature()");

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
            Program.LogFich.Debug("[Sign_Screen_Manager] Debut => Show()");
            Sign_Screen = new Form_Sign_Screen(WebBrowser, Url, iconApp);

            Program.LogFich.Debug("[Sign_Screen_Manager] toClose=" + Sign_Screen.toClose.ToString());
            if (Sign_Screen.toClose == true)
            {
                Program.LogFich.Debug("[Sign_Screen_Manager] toClose enter");
                Sign_Screen.Dispose();
                Sign_Screen = null;
            }
            else
            {
                Sign_Screen.Show();
                if (Sign_Screen.toClose == true)
                {
                    Program.LogFich.Debug("[Sign_Screen_Manager] toClose enter");
                    Sign_Screen.Dispose();
                    Sign_Screen = null;
                }
            }
            Program.LogFich.Debug("[Sign_Screen_Manager] Fin => Show()");
        }
    }
}
