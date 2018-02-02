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
    public class DataLogicManager : IData
    {
        public event ChangeLabelStatusIDataEventArgs ChangeLabelStatusIData;
        private WebBrowser webBrowser;
        private static string[] ctrl = { "NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL", "BS", "TAB", "LF", "VT", "FF", "CR", "SO", "SI", "DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB", "CAN", "EM", "SUB", "ESC", "FS", "GS" };
        private string numeroBadgeString;
        private byte[] numeroBadgeBrut;
        private string avant;
        private string apres;
        private byte avantByte;
        private byte apresByte;
        private int debut;
        private int longueur;
        private int longueur_max;
        private int decoupe;
        private int conversion;

        public string messageOK = "<ESC>E<ESC>   ---> OK <--- <ESC>E<ESC>    #POS <ESC>[6q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[7q<CR>";
        public string messageNOK = "<ESC>E<ESC>  --NON VALIDE--<ESC>E<ESC>    #POS <ESC>[7q<ESC>[8q<ESC>[4q<ESC>[5q<ESC>[8q<ESC>[4q<ESC>[5q<ESC>[8q<ESC>[4q<ESC>[5q<ESC>[5q<ESC>[5q<ESC>[5q<ESC>[9q<CR>";
        public string messageEND = "<ESC>[7m<ESC>E<ESC>  --TERMINER--  <ESC>[0m<ESC>E<ESC>    #POS <ESC>E<ESC> RETOUR AU POSTE<ESC>[6q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[7q<ESC>[6q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[7q<ESC>[6q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[7q<CR>";

        public string DouchetteRetour = "";
        public bool ScanEncours;
        public bool scrutation;

        #region Constructeur
        public DataLogicManager(WebBrowser web, string avant, string apres, int debut, int longueur, int longueur_max, int decoupe, int conversion)
        {
            this.webBrowser = web;
            this.avant = avant;
            this.apres = apres;
            this.debut = debut;
            this.longueur = longueur;
            this.longueur_max = longueur_max;
            this.decoupe = decoupe;
            this.conversion = conversion;
            if (avant != null)
            {
                this.avantByte = StringToByteSpecial(avant);
            }
            else
            {
                this.avantByte = StringToByteSpecial("NUL");
            }
            if (apres != null)
            {
                this.apresByte = StringToByteSpecial(apres);
            }
            else
            {
                this.apresByte = StringToByteSpecial("NUL");
            }
        }
        #endregion

        #region Properties
        public string NumeroBadgeString
        {
            get { return this.numeroBadgeString; }
            set { numeroBadgeString = value; }
        }
        public byte[] NumeroBadgeBrut
        {
            get { return this.numeroBadgeBrut; }
            set { numeroBadgeBrut = value; }
        }
        public string Avant
        {
            get { return this.avant; }
            set { avant = value; }
        }
        public string Apres
        {
            get { return this.apres; }
            set { apres = value; }
        }
        public byte AvantByte
        {
            get { return this.avantByte; }
            set { avantByte = value; }
        }
        public byte ApresByte
        {
            get { return this.apresByte; }
            set { apresByte = value; }
        }
        public int Debut
        {
            get { return this.debut; }
            set { debut = value; }
        }
        public int Longueur
        {
            get { return this.longueur; }
            set { longueur = value; }
        }
        public int Longueur_max
        {
            get { return this.longueur_max; }
            set { longueur_max = value; }
        }
        public int Decoupe
        {
            get { return this.decoupe; }
            set { decoupe = value; }
        }
        public int Conversion
        {
            get { return this.conversion; }
            set { conversion = value; }
        }

        public WebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { webBrowser = value; }
        }
        WebBrowser IData.webBrowser
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public DataLogicPortSerie dataLogicPortSerie
        {
            get { return dataLogicPortSerie; }
            set { dataLogicPortSerie = value; }
        }

        #endregion

        //Fonction: cf classe IDATA
        public void traiter(string s)
        {
            //ChangeLabelStatusIData("Status: Data processing...");
            Program.LogFich.Info("[DataLogicManager]: Traitement en cours (debut)");

            try
            {
                // nettoyage du code retourné
                string tempoEntree = s;

                tempoEntree = tempoEntree.Replace("<CR>", null);
                tempoEntree = tempoEntree.Replace("<LF>", null);
                // Extration douchette et code
                string lecteur = tempoEntree.Substring(0, 4);
                numeroBadgeString = tempoEntree.Substring(4, tempoEntree.Length - 4);

                // Mise à disposition du retour pour envoie à la douchette
                DouchetteRetour = lecteur + messageOK;
                DouchetteRetour = DouchetteRetour.Replace("#POS", "");

                //ChangeLabelStatusIData("Status: Data processing...[Succeded]");
                Program.LogFich.Info("[DataLogicManager] Traitement terminé - Trame = " + numeroBadgeString);
                Program.LogFich.Info("Lecture OK - Numéro de badge:" + numeroBadgeString);
            }
            catch (Exception e)
            {
                ChangeLabelStatusIData("Status: Data processing...[Failed]");
                Program.LogFich.Error("[DataLogicManager] Erreur de traitement (conversion) = " + e.ToString());
                //Program.LogFich.Error("Lecture ECHEC - " + e.ToString());
                MessageBox.Show("Problème dans les paramètres de conversion");
            }
        }

        //Fonction: cf classe IDATA
        public bool envoyer(Uri url, string nomMachine)
        {
            return true;
        }

        //Fonction: On découpe selon 
        public string Decouper(string chaine)
        {
            StringBuilder chaineDecoupe = new StringBuilder();

            int cpt = Debut;
            for (int cpt2 = 0; cpt2 < Longueur; cpt2++)
            {
                chaineDecoupe.Append(chaine[cpt]);
                cpt++;
            }

            for (cpt = 0; cpt < chaineDecoupe.Length; cpt++)
            {
                Program.LogFich.Info("[DataLogicManager]: Resultat du decoupage = " + chaineDecoupe[cpt]);
            }
            return chaineDecoupe.ToString();
        }

        #region Conversion StringToByteSpecial() - ByteToString() - CorrectionTailleTrame()
        private byte StringToByteSpecial(string s)
        {
            s = s.Replace("\"", "");

            bool trouver = false;
            int cpt = 0;

            while (!trouver)
            {
                if (ctrl[cpt].Equals(s))
                    trouver = true;
                else
                    cpt++;
            }
            byte sByte = Byte.Parse(cpt.ToString());
            Debug.Print("[DataLogicManager]: String spécial à  convertir =>" + s + "=>" + sByte);
            return sByte;
        }

        public static byte[] transformeStringEnByte(string entree)
        {
            byte[] byteRetour = new byte[entree.Length];
            int z = 0;
            int a = 0;
            do
            {
                if ((a + 5 < entree.Length) && (entree.Substring(a, 5) == "<ESC>"))
                {
                    byteRetour[z] = 27;
                    a = a + 5;
                }
                else
                {
                    if ((a + 4 <= entree.Length) && (entree.Substring(a, 4) == "<CR>"))
                    {
                        byteRetour[z] = 13;
                        a = a + 4;
                    }
                    else
                    {
                        byteRetour[z] = (byte)entree[a];
                        a++;
                    }
                }
                z++;

            }
            while (a < entree.Length);

            byte[] bRetour = new byte[z];
            //bRetour = byteRetour;
            int cpt;
            for (cpt = 0; cpt < z; cpt++)
            {
                bRetour[cpt] = byteRetour[cpt];
            }
            return bRetour;
        }

        public string ByteToString(byte[] b, int taille)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte bb in b)
            {
                if (bb > Byte.Parse("29"))
                    sb.Append(Convert.ToChar(bb));
                else
                    sb.Append('"').Append(ctrl[bb]).Append('"');
            }
            return sb.ToString();
        }

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
    }
}
