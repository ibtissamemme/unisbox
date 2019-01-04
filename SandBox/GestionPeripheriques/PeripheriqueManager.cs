using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;

namespace SandBox
{
    public class PeripheriqueManager
    {
        private List<PeripheriqueInfos> listePeripherique = new List<PeripheriqueInfos>();
        private XmlReader reader;
        private WebBrowser webBrowser;
        private Uri url;
        private static System.Object lockThis = new System.Object();

        private Main main_form;

        #region Constructeur
        public PeripheriqueManager(WebBrowser web, Uri url, Main main_form)
        {
            this.url = url;
            this.webBrowser = web;
            this.main_form = main_form;
        }
        #endregion

        #region Properties
        public List<PeripheriqueInfos> ListePeripherique
        {
            get { return listePeripherique; }
            set { listePeripherique = value; }
        }
        #endregion

        //Fonction: On telecharge un flux XML qui indique les périphériques + infos du poste
        public void DownloadXML()
        {
            Program.LogFich.Info("[PeripheriqueManager] DownloadXML() - [DEBUT] Telechargement xml [" + Functions.getHost() + "]");
            string getVars = "?machine=" + Functions.getHost();
            string adresse = url.ToString() + getVars;

            string nomfichier;
            string[] tmp;
            tmp = url.ToString().Split('/');
            nomfichier = tmp[tmp.Length - 3];

            //-----------xml telechargement------------------
            string stringHtml = null;
            try
            {

                if (!SandBox.Properties.Settings.Default.cookieHttpOnly)
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

                    stringHtml = wc.DownloadString(adresse);
                    wc.Dispose();
                }
                else
                {
                    Uri urlTemp = webBrowser.Url;
                    main_form.pageready = false;
                    webBrowser.Navigate(adresse);
                    //await main_form.PageLoad(10);
                    //main_form.WaitForPageLoad();
                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                        //System.Threading.Thread.Sleep(100);
                    };
                    stringHtml = webBrowser.Document.Body.InnerText;
                    //webBrowser.Navigate(urlTemp);

                }
            }
            catch (WebException we)
            {
                Program.LogFich.Error(we.ToString());
                //MessageBox.Show(we.Message);
                return;
            }

            try
            {
                XmlDocument xml = new System.Xml.XmlDocument();
                string clearXml = stringHtml;
                if (SandBox.Properties.Settings.Default.cookieHttpOnly)
                {
                    clearXml = stringHtml.Replace(" <", "<").Replace("\r\n-", "").Replace("\r\n", "").Substring(stringHtml.IndexOf("?>") + 1).Trim();
                }
                xml.LoadXml(clearXml);
                xml.Save(Environment.ExpandEnvironmentVariables("%TEMP%\\") + nomfichier + "_devices.xml");
            }
            catch (Exception e)
            {
                Program.LogFich.Error(e.ToString());
                MessageBox.Show(e.Message);
            }

            //-----------test xml local-------------------           
            adresse = Environment.ExpandEnvironmentVariables("%TEMP%\\") + nomfichier + "_devices.xml";

            try
            {
                //reader = new XmlTextReader(adresse);
                XmlReaderSettings xmlSettings = new XmlReaderSettings();
                xmlSettings.CloseInput = true;
                reader = XmlReader.Create(adresse, xmlSettings);

                Program.LogFich.Info("[PeripheriqueManager] DownloadXML() - [FIN] Telechargement xml termine");
            }
            catch (Exception e)
            {
                Program.LogFich.Info("[PeripheriqueManager] DownloadXML() - [FIN] Erreur de Telechargement" + e.ToString());
                //DownloadXML();
                MessageBox.Show(e.Message);
            }
        }

        //Fonction: On traite le flux XML (on récupère les infos qu'on ajoute a la listePeripherique
        public void TraiterXML()
        {
            Program.LogFich.Info("[PeripheriqueManager] TraiterXML() - [DEBUT] Traitement de la liste des periphériques");

            PeripheriqueInfos perifInfo = new PeripheriqueInfos();
            string nomElement = "";

            //protection de la ressource
            lock (lockThis)
            {
                try
                {
                    this.DownloadXML();
                    while (reader.Read() && (reader != null))
                    {
                        string s = reader.Value;
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                nomElement = reader.Name;
                                if (nomElement.Equals("peripherique"))
                                {
                                    perifInfo = new PeripheriqueInfos();
                                }
                                break;

                            case XmlNodeType.EndElement:
                                nomElement = reader.Name;
                                if (nomElement.Equals("peripherique"))
                                {
                                    listePeripherique.Add(perifInfo);
                                }
                                break;

                            case XmlNodeType.Text:

                                if (nomElement.Equals("nom"))
                                {
                                    perifInfo.Nom = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("type_port"))
                                {
                                    perifInfo.Type_port = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("bit_seconde"))
                                {
                                    if (!reader.Value.ToString().Equals(""))
                                        perifInfo.Bit_seconde = int.Parse(reader.Value.ToString());
                                }
                                else if (nomElement.Equals("bit_donnee"))
                                {
                                    if (!reader.Value.ToString().Equals(""))
                                        perifInfo.Bit_donnee = int.Parse(reader.Value.ToString());
                                }
                                else if (nomElement.Equals("parite"))
                                {
                                    perifInfo.Parite = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("bit_arret"))
                                {
                                    perifInfo.Bit_arret = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("controle_flux"))
                                {
                                    perifInfo.Controle_flux = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("port"))
                                {
                                    perifInfo.Port = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("webservice"))
                                {
                                    perifInfo.Webservice = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("ws_username"))
                                {
                                    perifInfo.Username = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("ws_password"))
                                {
                                    perifInfo.Password = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("traitement"))
                                {
                                    perifInfo.Traitement = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("avant"))
                                {
                                    perifInfo.Avant = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("apres"))
                                {
                                    perifInfo.Apres = reader.Value.ToString();
                                }
                                else if (nomElement.Equals("debut"))
                                {
                                    if (!reader.Value.ToString().Equals(""))
                                        perifInfo.Debut = int.Parse(reader.Value.ToString());
                                }
                                else if (nomElement.Equals("longueur"))
                                {
                                    if (!reader.Value.ToString().Equals(""))
                                        perifInfo.Longueur = int.Parse(reader.Value.ToString());
                                }
                                else if (nomElement.Equals("longueur_max"))
                                {
                                    if (!reader.Value.ToString().Equals(""))
                                        perifInfo.Longueur_max = int.Parse(reader.Value.ToString());
                                }
                                else if (nomElement.Equals("decoupe"))
                                {
                                    if (!reader.Value.ToString().Equals(""))
                                        perifInfo.Decoupe = int.Parse(reader.Value.ToString());
                                }
                                else if (nomElement.Equals("conversion"))
                                {
                                    if (!reader.Value.ToString().Equals(""))
                                        perifInfo.Conversion = int.Parse(reader.Value.ToString());
                                }
                                break;
                        }
                    }
                    reader.Close();
                    reader = null;
                }
                catch (Exception e)
                {
                    string s = e.ToString();
                    if (reader != null)
                    {
                        reader.Close();
                        reader = null;
                    }
                    //TraiterXML();
                    Program.LogFich.Error(s);
                    //MessageBox.Show(e.Message);
                }
                Program.LogFich.Info("[PeripheriqueManager] TraiterXML() - [FIN] Traitement de la liste des periphériques terminé");
            }

        }
    }
}


