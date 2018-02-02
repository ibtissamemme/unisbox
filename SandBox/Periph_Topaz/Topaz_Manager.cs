using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using System.Drawing;

namespace SandBox.Periph_Topaz
{
    public class Topaz_Manager
    {
        private Uri url;
        private WebBrowser WebBrowser;
        private Form_Topaz sigplus;
        public event ChangeLabelStatusEventArgs ChangeLabelStatus;

        public Topaz_Manager(Uri _url, WebBrowser _webBrowser)
        {
            url = _url;
            WebBrowser = _webBrowser;
        }

        public void Lecture_signature(short mode)
        {
            Program.LogFich.Info("[Topaz_Manager] Debut => Lecture_signature()");
            if (sigplus != null)
            {
                sigplus = null;
            }
            sigplus = new Form_Topaz("Saisie de la signature", url, WebBrowser, mode, this);
            if (sigplus != null && sigplus.estconnecte)
            {
                sigplus.ShowDialog();
                Program.LogFich.Info("[Topaz_Manager] Fin => Lecture_signature()");
            }
        }

        public void delete_sigplus()
        {
            this.ChangeLabelStatus("Status : None");
            if (sigplus != null) sigplus = null;
        }
    }
}
