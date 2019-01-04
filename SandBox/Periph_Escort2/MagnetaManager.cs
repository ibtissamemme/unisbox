using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Web;
using System.Diagnostics;
using System.Windows.Forms;

namespace SandBox
{
    public class MagnetaManager : IData
    {
        public event ChangeLabelStatusIDataEventArgs ChangeLabelStatusIData;
        private WebBrowser webBrowser;

        private string rondier;
        private string batterie;
        private ArrayList rondes;
        private string sortieString;
        private int nbrRondeLues;

        #region Constructeur
        public MagnetaManager(WebBrowser web)
        {
            nbrRondeLues = 0;
            this.webBrowser = web;
        }
        #endregion

        #region Properties
        public string Rondier
        {
            get { return this.rondier; }
        }
        public string Batterie
        {
            get { return this.batterie; }
        }
        public int NbrRondeLues
        {
            get { return this.nbrRondeLues; }
        }
        public ArrayList Rondes
        {
            get { return this.rondes; }
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

        //Fonction: cf class IDATA
        public void traiter(string s)
        {
            ChangeLabelStatusIData("Status : Data processing...");
            Program.LogFich.Info("[MagnetaManager] : Traitement en cours = " + s);

            batterie = "";
            this.rondes = new ArrayList();
            s = s + "\n RESUME";
            sortieString = s;
            char[] separateurs = { '\f', '\n', '\r' };
            string[] ss = s.Split(separateurs, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;


            string tmp = "";
            Match m;
            //lecture premiere ligne pour récupérer le n° de rondier
            for (int j = 0; j < 5; j++)
            {
                tmp = ss[j].Trim();
                Regex rondierRegex = new Regex(@"^[\d]{2}.[\d]{2}.[\d]{2} [\d]{2}:[\d]{2} #([\d]+)");
                m = rondierRegex.Match(tmp);
                if (m.Success)
                {
                    rondier = m.Groups[1].Value;
                    break;
                }
            }

            //les 3 lignes suivantes sont inutiles
            i++; i++; i++;

            StringBuilder sb;
            Regex finRondeRegex = new Regex("-{13}");

            try
            {
                //pour chaque ronde jusqu'à RESUME
                tmp = ss[i++];
                while (!tmp.Contains("RESUME"))
                {
                    sb = new StringBuilder();
                    m = finRondeRegex.Match(tmp);
                    while (!m.Success)
                    {
                        sb.Append(tmp).Append('\n');
                        tmp = ss[i++];
                        m = finRondeRegex.Match(tmp);
                    }
                    sb.Append(tmp).Append('\n');
                    MagnetaRonde ronde = new MagnetaRonde(sb.ToString());
                    rondes.Add(ronde);
                    tmp = ss[i++];
                }
            }
            catch (Exception ex)
            {
                ChangeLabelStatusIData("Status: Processing error...[Failed]");
                Program.LogFich.Error("[MagnetaManager] Erreur de traitement : " + ex.ToString());
            }


            try
            {
                //récupération du niveau de la batterie
                sb = new StringBuilder();
                Regex batterieRegex = new Regex(@"^[\s|:]*(.*[Bb][Aa][Tt]{2}[Ee][Rr][Ii][Ee].*[^\s|:])[\s|:]*$");
                for (i++; i < ss.Length; i++)
                {
                    m = batterieRegex.Match(ss[i]);
                    if (m.Success)
                    {
                        sb.Append(m.Groups[1].Value).Append('\n');
                    }
                }

                batterie = sb.ToString();

                foreach (MagnetaRonde r in rondes)
                {
                    r.SortieString += batterie;
                }
            }
            catch (Exception ex)
            {
                Program.LogFich.Error("[MagnetaManager] Erreur de traitement batterie : " + ex.ToString());
            }

            ChangeLabelStatusIData("Status : Data processing...[Succeded]");
            Program.LogFich.Info("[MagnetaManager] Traitement terminé");

        }

        //Fonction: cf class IDATA
        public bool envoyer(Uri url, string nomMachine)
        {
            ChangeLabelStatusIData("Status : Sending data...");
            Program.LogFich.Info("[MagnetaManager]: Envoie de données (debut)");
            bool b = true;

            //Il y a des rondes
            if (rondes.Count != 0)
            {
                StringBuilder param = new StringBuilder();
                param.Append("type_peripherique=").Append(HttpUtility.UrlEncode("magneta", Encoding.ASCII));
                param.Append("&ronde_rondier=").Append(HttpUtility.UrlEncode(rondier, Encoding.ASCII));
                param.Append("&machine=").Append(HttpUtility.UrlEncode(nomMachine, Encoding.ASCII));

                int i = 1;
                Program.LogFich.Info("[MagnetaManager]: nombre de ronde trouvé " + rondes.Count.ToString());
                foreach (MagnetaRonde r in rondes)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(param);

                    //correction date
                    DateTime date_temp;
                    date_temp = r.Date;
                    if (date_temp == new DateTime(1, 1, 1))
                    {
                        date_temp = DateTime.Now;
                    }

                    // Set values for the request back
                    sb.Append("&ronde_date=").Append(HttpUtility.UrlEncode(date_temp.ToString(), Encoding.ASCII));
                    sb.Append("&ronde_num=").Append(HttpUtility.UrlEncode(r.Numero, Encoding.ASCII));
                    sb.Append("&ronde_lib=").Append(HttpUtility.UrlEncode(r.Libelle, Encoding.ASCII));
                    sb.Append("&ronde_agent=").Append(HttpUtility.UrlEncode(r.Agent, Encoding.ASCII));
                    sb.Append("&ronde_detail=").Append(HttpUtility.UrlEncode(r.SortieString, Encoding.ASCII));

                    Program.LogFich.Info("[MagnetaManager]: Trame à envoyée =" + sb.ToString());

                    try
                    {
                        if (rondes.IndexOf(r) < rondes.Count - 1)
                        {
                            //todo changer l'appel de l'url par celui du webbrowser
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                            request.UseDefaultCredentials = true;

                            IWebProxy wp = WebRequest.GetSystemWebProxy();
                            request.Proxy = wp;

                            request.KeepAlive = false;
                            request.ProtocolVersion = HttpVersion.Version10;
                            request.Method = "POST";
                            request.ContentType = "application/x-www-form-urlencoded";

                            byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());
                            request.ContentLength = postBytes.Length;

                            Program.LogFich.Debug("[MagnetaManager]: Appel : " + url.ToString());
                            Stream requestStream = request.GetRequestStream();
                            // now send it                            
                            requestStream.Write(postBytes, 0, postBytes.Length);
                            requestStream.Close();
                            requestStream.Dispose();

                            // grab te response and print it out to the console along with the status code
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            b = b && (response.StatusCode == HttpStatusCode.OK);

                            Program.LogFich.Debug("[MagnetaManager]: Retour : " + response.StatusDescription);

                            //StreamReader sr = new StreamReader(response.GetResponseStream());
                            //string s = sr.ReadToEnd();
                            nbrRondeLues++;
                        }
                        else
                        {
                            sb.Append("&retour=").Append("True");
                            byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());
                            Program.LogFich.Debug("[MagnetaManager]: Appel : " + url.ToString());
                            WebBrowser.Navigate(url, "", postBytes, "Content-Type: application/x-www-form-urlencoded");
                            Program.LogFich.Debug("[MagnetaManager]: Retour");
                            nbrRondeLues++;
                        }
                        ChangeLabelStatusIData("MagnetaManager: Sending data...[Succeeded]");

                        //Program.LogFich.InfoUtilisateur("Envoie OK - " + sb.ToString());                        
                    }
                    catch (WebException we)
                    {
                        //_log.Error(we.ToString());
                        using (WebResponse response = we.Response)
                        {
                            HttpWebResponse httpResponse = (HttpWebResponse)response;
                            ChangeLabelStatusIData("Status : Sending data...[Failed]");
                            Program.LogFich.Error("[MagnetaManager]: Erreur d'envoie = " + httpResponse.StatusCode);
                            using (Stream data = response.GetResponseStream())
                            {
                                string text = new StreamReader(data).ReadToEnd();
                                Program.LogFich.Error(text);
                            }
                        }
                        b = false;
                    }
                    catch (Exception e)
                    {
                        ChangeLabelStatusIData("Status : Sending data...[Failed]");
                        Program.LogFich.Error("[MagnetaManager]: Erreur d'envoie = " + e.ToString());
                        //Program.LogFich.InfoUtilisateur("Envoie ECHEC - " + e.ToString());
                        b = false;
                    }
                    i++;

                }

            }
            else //Il n'y a pas de ronde
            {
                nbrRondeLues = 0;
                ChangeLabelStatusIData("Status : Device is empty...");
                Program.LogFich.Info("[MagnetaManager]: Aucune ronde dans l'appareil");
            }
            return b;
        }
    }
}
