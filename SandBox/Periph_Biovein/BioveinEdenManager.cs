using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using Communication;
using System.Net;

namespace SandBox.Periph_Biovein.Periph_Biovein_Eden
{
    class BioveinEdenManager
    {
        /*Constantes*/
        const int MAXENROLE = 1000;

        public event ChangeLabelStatusEventArgs ChangeLabelStatus;
        private WebBrowser webBrowser;
        private Uri url;
        private Uri urlApplication;

        Biovein bio;

        /*Variables pour l'enrolement d'un utilisateur*/
        public String nom;
        public String prenom;
        public int droits;

        /*Variables du Biovein*/
        String IP;
        String mdp;
        int port;

        private string idVisiteur;

        XmlTextReader reader;
        String fichier;

        #region Constructeur
        public BioveinEdenManager(WebBrowser web, Uri urlApplication, Uri url)
        {
            this.urlApplication = urlApplication;
            this.url = url;
            this.webBrowser = web;
            /*StringBuilder sb = new StringBuilder();
            string tempDirectory = Directory.GetCurrentDirectory();
            sb.Append(tempDirectory);
            sb.Append("\\Resources\\BioveinEden\\");*/
            //Directory.SetCurrentDirectory(tempDirectory);         
        }

        #endregion

        public void Enrolement(String _idVisiteur)
        {
            ChangeLabelStatus("Status: Début enrolement.");
            SplashForm sf = new SplashForm();
            sf.affiche("Enrôlement en cour...");
            idVisiteur = _idVisiteur;
            try
            {
                DownloadXML(1);
                TraiterXML();

                bio = new Biovein(IP, port, mdp);

                DownloadXML(0);
                TraiterXML();

                /*on enrole la personne et on récupère le template*/
                String template = Enroler(nom, prenom, droits, 1);

                if (template == "")
                {
                    ChangeLabelStatus("Status:[Fin] Echec Enrôle.");
                    sf.Close();
                    return;
                }

                /*enregistrement du template dans la base de donnée*/
                SendDataToAdd(template, (uint)template.Length);
            }
            catch (Exception ex)
            {
                Program.LogFich.Error("[BioveinEdenManager] Enrolement() - Erreur : " + ex.ToString());
            }
            /*Fin Enrolement*/
            sf.Close();
            ChangeLabelStatus("Status: Fin enrolement.");
        }

        private String Enroler(string nom, string prenom, int indexDroit, int index)
        {
            UserInformation userInformation = new UserInformation();
            String template = "";

            /*Ici on libère la case mémoire si elle est occupée*/
            try
            {
                bio.DeleteUserByUserIndex(index);
            }
            catch (Exception)
            {

            }

            userInformation.FirstName = nom;
            userInformation.Name = prenom;
            userInformation.MenuAccess = indexDroit;
            userInformation.UserIndex = index;
            /*cte : mode de fonctionnement*/
            userInformation.WiegandClockDataIdentifier = (long)0;

            try
            {
                bio.SetUserInformation(userInformation);

            }
            catch (Exception)
            {
                ChangeLabelStatus("Status: Erreur lors de l'enregistrement de " + nom + " " + prenom + ".");
                return "";
            }

            try
            {
                /*On enrolera toujours 2 doits*/
                template = bio.EnrolUser(userInformation.UserIndex, (byte)1);
            }
            catch (Exception ex)
            {
                ChangeLabelStatus("Status: Erreur lors de l'enrolement de " + nom + " " + prenom + ". Message : " + ex.ToString());
                bio.DeleteUserByUserIndex(index);
                Program.LogFich.Error(ex.ToString());
            }
            return template;
        }

        //Fonction: Envoie des données du doigts a ajouter
        public bool SendDataToAdd(String dataPtr, uint dataLen)
        {
            ChangeLabelStatus("Status: Sending data...");
            Program.LogFich.Info("[BioveinEdenManager] SendDataToAdd() - Envoie des données");
            bool b = true;

            byte[] data = new byte[dataLen];
            StringBuilder dataSB = new StringBuilder();
            for (int i = 0; i < dataLen; ++i)
            {
                data[i] = Marshal.ReadByte(dataPtr, i);
                dataSB.Append(data[i]);
                dataSB.Append(" ");
            }

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("type_peripherique=").Append(HttpUtility.UrlEncode("EDEN_BIOVEIN_READ", Encoding.ASCII));
                sb.Append("&machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                sb.Append("&data=").Append(HttpUtility.UrlEncode(dataSB.ToString(), Encoding.ASCII));
                //sb.Append("&data=").Append(HttpUtility.UrlEncode(dataSB.ToString().Replace(" ", "").Trim(), Encoding.ASCII));
                sb.Append("&datalen=").Append(HttpUtility.UrlEncode(dataLen.ToString(), Encoding.ASCII));

                byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());

                StringBuilder adr = new StringBuilder();
                adr.Append(this.url.ToString());
                adr.Append(this.webBrowser.Url.Query);
                Uri adresse = new Uri(adr.ToString());
                webBrowser.Navigate(adresse, "", postBytes, "Content-Type: application/x-www-form-urlencoded");

                ChangeLabelStatus("Status: Sending data...[Succeeded]");
                Program.LogFich.Info("[BioveinEdenManager] SendDataToAdd() - Envoie termine - Trame envoyee = " + sb.ToString());
            }
            catch (Exception e)
            {
                b = false;
                ChangeLabelStatus("Status: Sending data...[Failed]");
                Program.LogFich.Error("[BioveinEdenManager] SendDataToAdd() - Erreur d'envoie = " + e.ToString());
            }
            return b;
        }

        public void TraiterXML()
        {
            if (fichier.Equals("EDEN_BIOVEIN_UTILISATEUR")) TraiterXMLEDEN_BIOVEIN_UTILISATEUR();
            else TraiterXMLEDEN_BIOVEIN_INFO();
        }

        /*Fonction: On telecharge un flux XML contenant les données suivante :
         * ID + TEMPLATE : EDEN_BIOVEIN_UTILISATEUR numéro 0
         * ou
         * IP + PORT + PWD Biovein : EDEN_BIOVEIN_INFO numéro != 0*/
        public void DownloadXML(int download)
        {
            /*Attention au choix du fichier téléchargé*/
            if (download != 0) fichier = "EDEN_BIOVEIN_INFO";
            else fichier = "EDEN_BIOVEIN_UTILISATEUR";

            Program.LogFich.Info("[BioveinEdenManager] DownloadXML() - [DEBUT] Telechargement XML[" + Functions.getHost() + "]");
            string getVars = "?machine=" + Functions.getHost();
            getVars += "&idvisiteur=" + idVisiteur;
            string adresse = urlApplication + "devices/" + fichier + ".asp" + getVars;

            try
            {
                WebClient wc = new WebClient();
                //wc.UseDefaultCredentials = true;
                wc.Credentials = CredentialCache.DefaultNetworkCredentials;

                // Proxy
                IWebProxy wp = WebRequest.GetSystemWebProxy();
                wp.Credentials = CredentialCache.DefaultNetworkCredentials;
                wc.Proxy = wp;

                var cookies = webBrowser.Document.Cookie;
                wc.Headers.Add(HttpRequestHeader.Cookie, cookies);

                reader = new XmlTextReader(new MemoryStream(wc.DownloadData(adresse)));
                //wc.DownloadData(urlApplication + "gestion/session_abandon.asp");
                wc.Dispose();
                Program.LogFich.Info("[BioveinEdenManager] DownloadXML() - [FIN] Telechargement xml termine");
            }
            catch (Exception ex)
            {
                Program.LogFich.Error("[BioveinEdenManager] DownloadXML() - [FIN] Erreur de réception xml:" + ex.ToString());
            }
        }


        /*IP + PORT + PWD Biovein : EDEN_BIOVEIN_INFO*/
        public void TraiterXMLEDEN_BIOVEIN_INFO()
        {
            Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_INFO() - [DEBUT] Traitement empreintes");

            string nomElement = "";
            string Ip = "";
            string Pwd = "";
            int Port = 10001;

            try
            {


                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            nomElement = reader.Name;
                            if (nomElement.Equals("info"))
                            {
                                Ip = "";
                                Pwd = "";
                                Port = 10001;
                            }
                            break;

                        case XmlNodeType.EndElement:
                            nomElement = reader.Name;
                            if (nomElement.Equals("info"))
                            {
                                Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_INFO() - Ip:" + Ip);
                                Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_INFO() - Pwd:" + Pwd);
                                Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_INFO() - Port:" + Port.ToString());

                                /*
                                //on convertit string en byte[]
                                byte[] data_Ip = new byte[uint.Parse(Ip.Length.ToString())];
                                byte[] data_Pwd = new byte[uint.Parse(Pwd.Length.ToString())];


                                //data_Ip
                                int i = 0, j = 0;
                                StringBuilder byt = new StringBuilder(100);

                                while (j < Ip.Length && i < uint.Parse(Ip.Length.ToString()))
                                {
                                    if (Ip[j].Equals(' '))
                                    {
                                        data_Ip[i] = byte.Parse(byt.ToString());
                                        i++;
                                        byt = new StringBuilder(100);
                                    }
                                    else
                                    {
                                        byt.Append(Ip.ToString().Substring(j, 1));

                                        if (j == Ip.Length - 1)
                                        {
                                            data_Ip[i] = byte.Parse(byt.ToString());
                                        }
                                    }
                                    j++;
                                }

                                IntPtr dataPtr = Marshal.AllocHGlobal(data_Ip.Length);
                                Marshal.Copy(data_Ip, 0, dataPtr, data_Ip.Length);

                                IntPtr retPtr = IntPtr.Zero;

                                //data_Pwd
                                i = 0; j = 0;

                                while (j < Pwd.Length && i < uint.Parse(Pwd.Length.ToString()))
                                {
                                    if (Pwd[j].Equals(' '))
                                    {
                                        data_Ip[i] = byte.Parse(byt.ToString());
                                        i++;
                                        byt = new StringBuilder(100);
                                    }
                                    else
                                    {
                                        byt.Append(Pwd.ToString().Substring(j, 1));

                                        if (j == Pwd.Length - 1)
                                        {
                                            data_Ip[i] = byte.Parse(byt.ToString());
                                        }
                                    }
                                    j++;
                                }

                                dataPtr = Marshal.AllocHGlobal(data_Pwd.Length);
                                Marshal.Copy(data_Pwd, 0, dataPtr, data_Pwd.Length);

                                retPtr = IntPtr.Zero;
                                */
                            }
                            break;

                        case XmlNodeType.Text:

                            if (nomElement.Equals("ip"))
                            {
                                Ip = reader.Value.ToString();
                            }
                            else if (nomElement.Equals("mdp"))
                            {
                                Pwd = reader.Value.ToString();
                            }
                            else if (nomElement.Equals("port"))
                            {
                                Port = int.Parse(reader.Value.ToString());
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_INFO() - ERREUR:" + e.ToString());
            }
            finally
            {
                reader.Close();
                reader = null;
            }

            this.IP = Ip;
            this.mdp = Pwd;
            this.port = Port;

            Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_INFO() - [FIN] Traitement des paramètres du Biovein terminé.");
        }

        /* nom
         * prenom
         * droit
         */
        public void TraiterXMLEDEN_BIOVEIN_UTILISATEUR()
        {
            Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_UTILISATEUR() - [DEBUT] Traitement empreintes");

            string nomElement = "";
            string nom = "";
            string prenom = "";
            int droit = 0;

            try
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            nomElement = reader.Name;
                            if (nomElement.Equals("info"))
                            {
                                nom = "";
                                prenom = "";
                                droit = 0;
                            }
                            break;

                        case XmlNodeType.EndElement:
                            nomElement = reader.Name;
                            if (nomElement.Equals("info"))
                            {
                                Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_UTILISATEUR() - nom:" + nom);
                                Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_UTILISATEUR() - prenom:" + prenom);
                                Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_UTILISATEUR() - droit:" + droit.ToString());
                                //Program.LogFich.Info("");
                                /*
                                //on convertit string en byte[]
                                byte[] data_nom = new byte[uint.Parse(nom.Length.ToString())];
                                byte[] data_prenom = new byte[uint.Parse(prenom.Length.ToString())];


                                //data_Ip
                                int i = 0, j = 0;
                                StringBuilder byt = new StringBuilder(100);

                                while (j < nom.Length && i < uint.Parse(nom.Length.ToString()))
                                {
                                    if (nom[j].Equals(' '))
                                    {
                                        data_nom[i] = byte.Parse(byt.ToString());
                                        i++;
                                        byt = new StringBuilder(100);
                                    }
                                    else
                                    {
                                        byt.Append(nom.ToString().Substring(j, 1));

                                        if (j == nom.Length - 1)
                                        {
                                            data_nom[i] = byte.Parse(byt.ToString());
                                        }
                                    }
                                    j++;
                                }

                                IntPtr dataPtr = Marshal.AllocHGlobal(data_nom.Length);
                                Marshal.Copy(data_nom, 0, dataPtr, data_nom.Length);

                                IntPtr retPtr = IntPtr.Zero;

                                //data_Pwd
                                i = 0; j = 0;

                                while (j < prenom.Length && i < uint.Parse(prenom.Length.ToString()))
                                {
                                    if (prenom[j].Equals(' '))
                                    {
                                        data_nom[i] = byte.Parse(byt.ToString());
                                        i++;
                                        byt = new StringBuilder(100);
                                    }
                                    else
                                    {
                                        byt.Append(prenom.ToString().Substring(j, 1));

                                        if (j == prenom.Length - 1)
                                        {
                                            data_nom[i] = byte.Parse(byt.ToString());
                                        }
                                    }
                                    j++;
                                }

                                dataPtr = Marshal.AllocHGlobal(data_prenom.Length);
                                Marshal.Copy(data_prenom, 0, dataPtr, data_prenom.Length);

                                retPtr = IntPtr.Zero;*/
                            }
                            break;

                        case XmlNodeType.Text:

                            if (nomElement.Equals("nom"))
                            {
                                nom = reader.Value.ToString();
                            }
                            else if (nomElement.Equals("prenom"))
                            {
                                prenom = reader.Value.ToString();
                            }
                            else if (nomElement.Equals("droit"))
                            {
                                droit = int.Parse(reader.Value.ToString());
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_UTILISATEUR() - ERREUR:" + e.ToString());
            }

            this.nom = nom;
            this.prenom = prenom;
            this.droits = droit;

            Program.LogFich.Info("[BioveinEdenManager] TraiterXMLEDEN_BIOVEIN_UTILISATEUR() - [FIN] Traitement des paramètres du Biovein terminé.");
        }
    }
}
