using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using System.Drawing;

namespace SandBox.Periph_Wacom_STU
{
    public class Wacom_STU_Manager
    {
        private Uri url;
        private WebBrowser WebBrowser;
        private Form_Wacom_STU STU;
        public event ChangeLabelStatusEventArgs ChangeLabelStatus;

        public Wacom_STU_Manager(Uri _url, WebBrowser _webBrowser)
        {
            url = _url;
            WebBrowser = _webBrowser;
        }

        public void Lecture_signature()
        {
            Program.LogFich.Info("[Wacom_STU] Debut => Lecture_signature()");
            if (STU != null)
            {
                STU = null;
            }
            try
            {
                wgssSTU.UsbDevices usbDevices = new wgssSTU.UsbDevices();
                if (usbDevices.Count != 0)
                {
                    try
                    {
                        wgssSTU.IUsbDevice usbDevice = usbDevices[0]; // select a device

                        STU = new Form_Wacom_STU("Saisie de la signature", url, WebBrowser, this, usbDevice);

                        STU.ShowDialog();
                        Program.LogFich.Info("[Wacom_STU] Fin => Lecture_signature()");
                        this.ChangeLabelStatus("Status : None");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Program.LogFich.Error("[Wacom_STU] Lecture_signature() = " + ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("No STU devices attached");
                    this.ChangeLabelStatus("Status : None");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No STU devices attached");
                Program.LogFich.Error("[Wacom_STU] Lecture_signature() (usbDevices.Count) = " + ex.ToString());
            }

        }

        public void delete_sigplus()
        {
            this.ChangeLabelStatus("Status : None");
            if (STU != null) STU = null;
        }
    }
}
