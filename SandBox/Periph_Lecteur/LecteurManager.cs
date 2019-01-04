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
    public class LecteurManager : IData
    {
        public event ChangeLabelStatusIDataEventArgs ChangeLabelStatusIData;
        private WebBrowser webBrowser;
        private static string[] ctrl = { "NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL", "BS", "TAB", "LF", "VT", "FF", "CR", "SO", "SI", "DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB", "CAN", "EM", "SUB", "ESC", "FS", "GS" };
        private string numeroBadgeString;
        private string numeroBadgeBrut;
        private string avant;
        private string apres;
        private byte avantByte;
        private byte apresByte;
        private int debut;
        private int longueur;
        private int longueur_max;
        private int decoupe;
        private int conversion;


        #region Constructeur
        public LecteurManager(WebBrowser web, string avant, string apres, int debut, int longueur, int longueur_max, int decoupe, int conversion)
        {
            this.webBrowser = web;
            this.avant = avant;
            this.apres = apres;
            this.debut = debut;
            this.longueur = longueur;
            this.longueur_max = longueur_max;
            this.decoupe = decoupe;
            this.conversion = conversion;

            /*if (avant != null)
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
            }*/
        }
        #endregion

        #region Properties
        public string NumeroBadgeString
        {
            get { return this.numeroBadgeString; }
            set { numeroBadgeString = value; }
        }
        public string NumeroBadgeBrut
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

        #endregion

        //Fonction: cf classe IDATA
        public void traiter(string s)
        {
            ChangeLabelStatusIData("Status: Data processing...");
            Program.LogFich.Info("[LecteurManager]: Traitement en cours (debut)");

            try
            {
                // nettoyage général du code retourné
                s = s.Trim();
                s = s.Replace(Environment.NewLine, "");

                this.NumeroBadgeBrut = s;

                // nettoyage du code retourné pour douchette
                //s = s.Replace("\"CR\"\"LF\"", "");                

                //Decoupe avant la conversion
                if (decoupe == 1)
                {
                    Program.LogFich.Info("[LecteurManager] Traitement = Découpe avant conversion");

                    //[DECOUPAGE]
                    s = Decouper(s);

                    //[CONVERSION]
                    if (conversion == 1)            //Conversion Dec en hex
                    {
                        ulong decValue = ulong.Parse(s);
                        numeroBadgeString = decValue.ToString("X");
                    }
                    else if (conversion == 2)       //Conversion Hex en Dec
                    {
                        ulong decValue = ulong.Parse(s, System.Globalization.NumberStyles.HexNumber);
                        numeroBadgeString = decValue.ToString();
                    }
                    else
                    {
                        numeroBadgeString = s;
                    }
                }
                //Decoupe apres la conversion
                else if (decoupe == 2)
                {
                    Program.LogFich.Info("[LecteurManager] Traitement = Découpe après conversion");
                    //[CONVERSION]
                    if (conversion == 1)            //Conversion Dec en hex
                    {
                        ulong decValue = ulong.Parse(s);
                        numeroBadgeString = decValue.ToString("X");
                    }
                    else if (conversion == 2)       //Conversion Hex en Dec
                    {
                        ulong decValue = ulong.Parse(s, System.Globalization.NumberStyles.HexNumber);
                        numeroBadgeString = decValue.ToString(); ;
                    }
                    else
                    {
                        numeroBadgeString = s;
                    }
                    //[DECOUPAGE]
                    s = Decouper(s);
                }
                //Pas de découpage
                else
                {
                    //[CONVERSION]
                    if (conversion == 1)            //Conversion Dec en hex
                    {
                        ulong decValue = ulong.Parse(s);
                        numeroBadgeString = decValue.ToString("X");
                    }
                    else if (conversion == 2)       //Conversion Hex en Dec
                    {
                        ulong decValue = ulong.Parse(s, System.Globalization.NumberStyles.HexNumber);
                        numeroBadgeString = decValue.ToString(); ;
                    }
                    else
                    {
                        numeroBadgeString = s;
                    }
                }
                //MessageBox.Show(numeroBadgeString);
                ChangeLabelStatusIData("Status: Data processing...[Succeded]");
                Program.LogFich.Info("[LecteurManager] Traitement terminé - Trame = " + numeroBadgeString);
                //Program.LogFich.InfoUtilisateur("Lecture OK - Numéro de badge:" + numeroBadgeString);
            }
            catch (Exception e)
            {
                ChangeLabelStatusIData("Status: Data processing...[Failed]");
                Program.LogFich.Error("[LecteurManager] Erreur de traitement (conversion) = " + e.ToString());
                //Program.LogFich.InfoUtilisateur("Lecture ECHEC - " + e.ToString());
                MessageBox.Show("Problème dans les paramètres de conversion");
            }
        }

        //Fonction: cf classe IDATA
        public bool envoyer(Uri url, string nomMachine)
        {
            return true;
        }

        //Fonction: On nettoie les caracteres qu"on ne veut pas (saisies dans l'administration du périphérique)
        /*public byte[] Nettoyer(int indiceDelete, byte[] chaine)
        {
            try
            {
                byte[] chaineClean = new byte[chaine.Length - 1];

                int cpt2 = 0;

                for (int cpt = 0; cpt < chaine.Length; cpt++)
                {
                    if (cpt != indiceDelete)
                    {
                        chaineClean[cpt2] = chaine[cpt];
                        cpt2++;
                    }
                }
                return chaineClean;
            }
            catch (Exception ex)
            {
                Program.LogFich.Error("Erreur Nettoyer = " + ex.ToString());
                return chaine;
            }

        }*/
        public string Nettoyer(string chaine)
        {
            string temp_chaine = chaine;
            string[] tab_avant = null;
            string[] tab_apres = null;
            try
            {
                if (this.avant != null)
                {
                    tab_avant = this.avant.Split(' ');
                }
                if (this.apres != null)
                {
                    tab_apres = this.apres.Split(' ');
                }

                if (tab_avant != null)
                {
                    for (int i = 0; i < tab_avant.Length; i++)
                    {
                        temp_chaine = temp_chaine.Replace(tab_avant[i].ToString(), "");
                    }
                }
                if (tab_apres != null)
                {
                    for (int i = 0; i < tab_apres.Length; i++)
                    {
                        temp_chaine = temp_chaine.Replace(tab_apres[i].ToString(), "");
                    }
                }
            }
            catch (Exception ex)
            {
                Program.LogFich.Error("Erreur fonction Nettoyer = " + ex.ToString());
            }
            return temp_chaine.Trim();
        }

        //Fonction: On découpe selon 
        public string Decouper(string chaine)
        {
            StringBuilder chaineDecoupe = new StringBuilder();

            int cpt = Debut;
            if (this.Longueur.Equals("") || chaine.Length < this.Longueur)
            {
                this.Longueur = chaine.Length;
            }

            for (int cpt2 = 0; cpt2 < this.Longueur - 1; cpt2++)
            {
                if (cpt <= this.Longueur - 1)
                {
                    chaineDecoupe.Append(chaine[cpt]);
                    cpt++;
                }
            }

            for (cpt = 0; cpt < chaineDecoupe.Length; cpt++)
            {
                Program.LogFich.Info("[LecteurManager]: Resultat du découpage = " + chaineDecoupe[cpt]);
            }

            if (this.Longueur_max != 0 && chaineDecoupe.Length < this.Longueur_max)
            {
                for (int i = chaineDecoupe.Length; i < this.Longueur_max; i++)
                {
                    chaineDecoupe.Insert(0, "0");
                }
                Program.LogFich.Info("[LecteurManager]: Resultat du découpage avec longueur max = " + chaineDecoupe.ToString());
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
            Debug.Print("[LecteurManager]: String spécial à convertir =>" + s + "=>" + sByte);
            return sByte;
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
