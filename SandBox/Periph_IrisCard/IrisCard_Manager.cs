using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Drawing;

namespace SandBox.Periph_IrisCard
{
    public class IrisCard_Manager
    {

        Uri urlapp;
        WebBrowser WebBrowser;
        Form_Iriscan form;

        public event ChangeLabelStatusEventArgs ChangeLabelStatus;

        public IrisCard_Manager(Uri _urlapp, WebBrowser _WebBrowser)
        {
            urlapp = _urlapp;
            WebBrowser = _WebBrowser;
            //form.FormClosed += new FormClosedEventHandler(close);
        }

        public void Open()
        {
            if (form != null)
            {
                form = null;
            }
            form = new Form_Iriscan(urlapp, WebBrowser, this);
            form.ShowDialog();
        }

        public void Close()
        {
            this.ChangeLabelStatus("Status : None");
            //form.Close();
            //form = null;
        }
    }
}
