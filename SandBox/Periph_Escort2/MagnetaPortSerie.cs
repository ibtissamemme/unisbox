using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;

namespace SandBox
{
    public class MagnetaPortSerie : PortSerie
    {
        private MagnetaManager magnetaManager;
        private StringBuilder sb;

        #region Constructeur
        public MagnetaPortSerie(string nomPort, int bitsSeconde, int bitsDonnees, string parite, string bitsArret, string controleFlux, int timeout, WebBrowser web)
            : base(nomPort, bitsSeconde, bitsDonnees, parite, bitsArret, controleFlux, timeout)
        {
            magnetaManager = new MagnetaManager(web);
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            serialPort.DiscardNull = true;
            sb = new StringBuilder();
        }
        #endregion

        #region Properties
        public MagnetaManager MagnetaManager
        {
            get { return magnetaManager; }
            set { magnetaManager = value; }
        }
        #endregion

        //Fonction: cf class PortSerie
        public override bool OpenConnection()
        {
            try
            {
                if (serialPort.IsOpen == true)
                {
                    serialPort.Close();
                }
                serialPort.Open();
                serialPort.DiscardOutBuffer();

                OnChangeLabelStatus("Status: Device opened...");
                Program.LogFich.Info("[MagnetaPortSerie] Ouverture du port " + serialPort.PortName);
                return true;
            }
            catch
            {
                OnChangeLabelStatus("Status: Device disconnected...");
                Program.LogFich.Error("[MagnetaPortSerie] Peripherique non branché " + serialPort.PortName);
                MessageBox.Show("Erreur, Périphérique non connecté. Veuillez le connecter puis redémarrer l'application.");
                return false;
            }
        }

        //Fonction: cf class PortSerie
        public override void CloseConnection()
        {
            serialPort.Close();
            OnChangeLabelStatus("Status: Device closed...[" + magnetaManager.NbrRondeLues + "]");
            Program.LogFich.Info("[MagnetaPortSerie] Fermeture du port " + serialPort.PortName);
        }

        //Event Réception données terminée
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            PeripheriqueDataReceivingEventArgs receiving = new PeripheriqueDataReceivingEventArgs();
            if (receiving != null)
            {
                base.OnDataReceiving(receiving);
            }
            try
            {
                // Ori
                //char[] buffer = new char[serialPort.ReadBufferSize];
                //bool b = true;
                //char c;

                //do
                //{
                //    try
                //    {
                //        c = Convert.ToChar(serialPort.ReadChar());
                //        sb.Append(c);
                //    }
                //    catch
                //    {
                //        b = false;
                //    }
                //} while (b);


                // Version 2
                char[] buffer = new char[serialPort.ReadBufferSize];
                bool termine = true;
                char c;

                do
                {
                    try
                    {
                        c = Convert.ToChar(serialPort.ReadChar());
                        sb.Append(c);
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(1000);

                        try
                        {
                            c = Convert.ToChar(serialPort.ReadChar());
                            sb.Append(c);
                        }
                        catch
                        {
                            termine = false;
                        }
                    }
                } while (termine);


                PeripheriqueDataReceivedEventArgs DataReceived = new PeripheriqueDataReceivedEventArgs(sb.ToString());
                if (DataReceived != null)
                {
                    base.OnDataReceived(DataReceived);
                }
                OnChangeLabelStatus("Status: Receiving data...[Succeeded]");
                //Program.LogFich.Info("[MagnetaPortSerie] Réception terminée = " + sb.ToString());
                Program.LogFich.Info("[MagnetaPortSerie] Réception terminée");
            }
            catch
            {
                OnChangeLabelStatus("Status: Receiving data...[Failed]");
                Program.LogFich.Error("[MagnetaPortSerie] Erreur de reception");
            }
        }
    }
}
