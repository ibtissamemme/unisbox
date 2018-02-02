using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Net;
using System.IO;

namespace SandBox
{
    public class DeisterManager : IData
    {
        public event ChangeLabelStatusIDataEventArgs ChangeLabelStatusIData;
        private WebBrowser webBrowser;
        private static string[] ctrl = { "NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL", "BS", "TAB", "LF", "VT", "FF", "CR", "SO", "SI", "DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB", "CAN", "EM", "SUB", "ESC", "FS", "GS" };
        private byte[] ack = { 0x06, 0x03 };
        private Trame trame;
        private bool status;
        private int nbrRondeLues;

        #region Constructeur
        public DeisterManager(WebBrowser web)
        {
            nbrRondeLues = 0;
            this.webBrowser = web;
            status = false;
        }
        #endregion

        #region Properties
        WebBrowser IData.webBrowser
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public int NbrRondeLues
        {
            get { return this.nbrRondeLues; }
        }
        public Trame Trame
        {
            get { return trame; }
            set { trame = value; }
        }
        public WebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { webBrowser = value; }
        }
        public byte[] ACK
        {
            get { return ack; }
            set { ack = value; }
        }
        #endregion

        //Fonction: cf classe IDATA
        public void traiter(string s)
        {
            ChangeLabelStatusIData("Status: Processing data...");
            Program.LogFich.Info("[DeisterManager]: Traitement en cours = Aucun (fonction traiter() non utilisé) ");
        }

        //Fonction: cf classe IDATA
        public bool envoyer(Uri url, string nomMachine)
        {
            ChangeLabelStatusIData("Status: Sending data...");
            Program.LogFich.Info("[DeisterManager]: Envoie de données (debut)");
            bool b = true;

            //Il y a des rondes

            StringBuilder param = new StringBuilder();
            param.Append("type_peripherique=").Append(HttpUtility.UrlEncode("deister", Encoding.ASCII));
            param.Append("&ronde_rondier=").Append(HttpUtility.UrlEncode(trame.DeviceNumber.ToString(), Encoding.ASCII));
            param.Append("&machine=").Append(HttpUtility.UrlEncode(nomMachine, Encoding.ASCII));
            param.Append("&status_ram=").Append(HttpUtility.UrlEncode(trame.Status_bit0.ToString(), Encoding.ASCII));
            param.Append("&status_memory_empty=").Append(HttpUtility.UrlEncode(trame.Status_bit1.ToString(), Encoding.ASCII));
            param.Append("&status_battery=").Append(HttpUtility.UrlEncode(trame.Status_bit2.ToString(), Encoding.ASCII));
            param.Append("&status_memory_overflow=").Append(HttpUtility.UrlEncode(trame.Status_bit3.ToString(), Encoding.ASCII));

            foreach (DeisterRonde r in trame.Rondes)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(param);
                    sb.Append("&tag_date=").Append(HttpUtility.UrlEncode(r.DateHeure.ToString(), Encoding.ASCII));
                    sb.Append("&tag_num=").Append(HttpUtility.UrlEncode(r.Data.ToString(), Encoding.ASCII));

                    byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());
                    WebBrowser.Navigate(url, "", postBytes, "Content-Type: application/x-www-form-urlencoded");
                    nbrRondeLues++;
                    ChangeLabelStatusIData("DeisterManager: Sending data...[Succeeded]");
                    Program.LogFich.Info("[DeisterManager] Envoie termine - Trame envoyee = " + sb.ToString());
                    Program.LogFich.Info("Envoi OK - Tag:" + r.Data.ToString() + " DateHeure:" + r.DateHeure.ToString());
                }
                catch (Exception e)
                {
                    b = false;
                    ChangeLabelStatusIData("Status: Sending data...[Failed]");
                    Program.LogFich.Error("[DeisterManager]: Erreur d'envoie" + e.ToString());
                    Program.LogFich.Error("Envoi ECHEC - " + e.ToString());
                }
            }
            if (trame.Rondes.Count > 0)
            {
                StringBuilder sbFin = new StringBuilder();
                sbFin.Append(param);
                sbFin.Append("&end=true");

                byte[] postBytesFin = Encoding.ASCII.GetBytes(sbFin.ToString());
                WebBrowser.Navigate(url, "", postBytesFin, "Content-Type: application/x-www-form-urlencoded");
            }
            else
            {
                ChangeLabelStatusIData("Status: Device is empty...");
            }
            return b;
        }

        #region Gestion de trame :  CalculCrc() - AnalyseTrame() - CorrectionTailleTrame()
        //Calcul le CRC d'une trame, renvoie un byte[]
        public byte[] CalculCrc(byte[] s)
        {
            int CRC = 0xFFFF;
            int crcL, crcH, t;
            foreach (int bb in s)
            {
                crcH = (int)CRC;
                crcL = (int)(CRC >> 8);
                t = (int)((bb ^ crcH) & 0xFF);
                t ^= (int)((t << 4) & 0xFF);
                crcL ^= (int)(t >> 4) & 0xFF;
                crcL ^= (int)(t << 3) & 0xFF;
                crcH = (int)((t ^ (t >> 5))) & 0xFF;
                CRC = (int)((int)((crcH << 8) & 0xFF00) | (int)crcL);
            }
            int valeur = (int)~CRC & (0xFFFF);
            string crcString = Convert.ToString(valeur, 16);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] crcCalcule = enc.GetBytes(crcString);
            string st = ByteToString(crcCalcule);
            return crcCalcule;
        }

        //========================================================================================
        //Fonction qui analyse la trame
        public bool AnalyseTrame(byte[] trameRecu)
        {
            if (trameRecu.GetValue(0).Equals(Convert.ToByte(0x67)))
            {
                if (status == false)
                {
                    Program.LogFich.Info("[DeisterManager]: Analyse de trame 'g'");
                    this.Trame = new Trame(trameRecu);
                    this.Trame.AfficherTrameInfo();
                    status = true;
                }
                else
                {
                    Program.LogFich.Info("[DeisterManager]: Fin de l'analyse (on est revenu à une trame 'g')");
                    return true;
                }
            }
            else if (trameRecu.GetValue(0).Equals(Convert.ToByte(0x64)))
            {
                Program.LogFich.Info("[DeisterManager]:Analyse de trame 'd'");
                this.Trame.AddRonde(trameRecu);
            }
            else
            {
                Program.LogFich.Info("[DeisterManager]: Analyse de trame 'sans lettre'");
                this.Trame.AddRonde(trameRecu);
            }
            return false;
        }

        //========================================================================================
        //Fonction qui corrige la taille d'une trame    
        public byte[] CorrectionTailleTrame(int i, byte[] lecture)
        {
            byte[] trame = new byte[i];
            for (int a = 0; a < i; a++)
            {
                trame[a] = lecture[a];
            }
            return trame;
        }
        #endregion

        #region Conversion : ByteToString() - ByteToHex()

        //Fonction qui convertit une trame Byte en String
        public string ByteToString(byte[] b)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte bb in b)
            {
                if (bb > Byte.Parse("29"))
                    sb.Append(Convert.ToChar(bb));
                else
                    sb.Append('<').Append(ctrl[bb]).Append('>');
            }
            return sb.ToString();
        }

        //========================================================================================
        //Fonction qui convertit une trame Byte en String
        public string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            return builder.ToString().ToUpper();
        }
        //========================================================================================

        #endregion
    }
}
