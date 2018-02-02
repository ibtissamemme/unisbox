using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace SandBox
{
    public class DeisterPortSerie : PortSerie
    {
        private DeisterManager deisterManager;
        private Thread communiquer = null;
        private bool enMarche;

        #region Constructeur
        public DeisterPortSerie(string nomPort, int bitsSeconde, int bitsDonnees, string parite, string bitsArret, string controleFlux, int timeout, WebBrowser web)
            : base(nomPort, bitsSeconde, bitsDonnees, parite, bitsArret, controleFlux, timeout)
        {
            deisterManager = new DeisterManager(web);
            communiquer = new Thread(Echange);
            enMarche = true;
        }
        #endregion

        #region Properties
        public DeisterManager DeisterManager
        {
            get { return deisterManager; }
            set { deisterManager = value; }
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
                Program.LogFich.Info("[DeisterPortSerie] Ouverture du port " + serialPort.PortName);
                communiquer.Start();
                return true;
            }
            catch
            {
                OnChangeLabelStatus("Status: Device disconnected...");
                Program.LogFich.Error("[DeisterPortSerie] Peripherique non branché " + serialPort.PortName);
                MessageBox.Show("Erreur, Périphérique non connecté. Veuillez le connecter puis redémarrer l'application.");
                return false;
            }
        }

        //========================================================================================
        //Fonction: cf class PortSerie
        public override void CloseConnection()
        {
            serialPort.Close();
            OnChangeLabelStatus("Status: Device closed...[" + deisterManager.NbrRondeLues + "]");
            Program.LogFich.Info("[DeisterPortSerie] Fermeture du port " + serialPort.PortName);
        }

        //========================================================================================
        //Fonction: Vider le Deister
        public override void Nettoyer()
        {
            byte[] clearTrame = { 0x4C, 0x03 };
            SendMessage(clearTrame);
            OnChangeLabelStatus("Status: Device cleared...");
            Program.LogFich.Info("[DeisterPortSerie] Nettoyage Fin");

            byte[] closeTrame = { 0x5A, 0x03 };
            SendMessage(closeTrame);
            OnChangeLabelStatus("Status: Device disconnected...");
            Program.LogFich.Info("[DeisterPortSerie] Arret de connection");
        }

        //========================================================================================
        //Fonction: Réception de données
        public override bool ReceiveMessage()
        {
            try
            {
                byte[] lecture = new byte[serialPort.ReadBufferSize];
                int i = 0;
                byte b;

                do
                {
                    b = Convert.ToByte(serialPort.ReadByte());
                    lecture[i] = b;
                    i++;
                }
                while (b != Byte.Parse("03"));

                //On récupère le CRC à part
                for (int j = 0; j < 4; j++)
                {
                    b = Convert.ToByte(serialPort.ReadByte());
                    lecture[i] = b;
                    i++;
                }
                //On termine la trame Brut (avec CRC)
                byte[] trame = deisterManager.CorrectionTailleTrame(i, lecture);
                bool termine = deisterManager.AnalyseTrame(trame);
                Program.LogFich.Info("[DeisterPortSerie] Trame RECU(hex):" + deisterManager.ByteToHex(trame));
                Program.LogFich.Info("[DeisterPortSerie] Trame RECU(str):" + deisterManager.ByteToString(trame));
                return termine;
            }
            catch (Exception e)
            {
                OnChangeLabelStatus("Status: Data processing...[Failed]");
                Program.LogFich.Error("[DeisterPortSerie] Erreur de reception= " + e.ToString());
            }
            return false;
        }

        //========================================================================================
        //Fonction: Envoie de données
        public override void SendMessage(byte[] message)
        {
            byte[] crc = deisterManager.CalculCrc(message);
            byte[] newMsg = new byte[message.Length + 4];

            int z = 0;
            foreach (byte bb in message)
            {
                newMsg[z] = bb;
                z++;
            }
            foreach (byte bb in crc)
            {
                newMsg[z] = bb;
                z++;
            }
            if (serialPort.IsOpen == true)
            {
                try
                {
                    serialPort.Write(newMsg, 0, newMsg.Length);    //send the message to the port
                    OnChangeLabelStatus("Status: In Communication...");
                    Program.LogFich.Info("[DeisterPortSerie] Trame SEND(hex): " + deisterManager.ByteToHex(newMsg));
                    Program.LogFich.Info("[DeisterPortSerie] Trame SEND(str): " + deisterManager.ByteToString(newMsg));
                }
                catch (Exception e)
                {
                    OnChangeLabelStatus("Status: Communication...[Failed]");
                    Program.LogFich.Error("[DeisterPortSerie] Erreur d'envoie = " + e.ToString());
                }
            }
        }
        //========================================================================================
        //Fonction: Gère la Réception/Envoie
        public override void Echange()
        {
            OnChangeLabelStatus("Status: Waiting for the device...");

            bool termine = false;
            while (serialPort.IsOpen == true && termine == false)
            {
                SendMessage(deisterManager.ACK);
                termine = ReceiveMessage();
            }
            try
            {
                if (deisterManager.Trame.Rondes.Count != 0)
                {
                    deisterManager.Trame.AfficherListeRonde();
                }
            }
            catch
            {
                Program.LogFich.Error("[DeisterPortSerie] Aucune ronde à afficher");
            }
            if (enMarche)
            {
                PeripheriqueDataReceivedEventArgs received = new PeripheriqueDataReceivedEventArgs(deisterManager.Trame);
                if (received != null) base.OnDataReceived(received);
            }
        }

        //Fonction: Lorsqu'on appuie sur le bouton annulé, on annule le port
        public override void Annuler()
        {
            serialPort.DiscardOutBuffer();
            enMarche = false;
        }
    }
}
