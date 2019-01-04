using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Net;

namespace SandBox
{
    public class BioveinManager
    {
        #region Librairie
        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern int BioRFIDLoadModuleA([MarshalAs(UnmanagedType.LPStr)]string moduleName);

        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern void BioRFIDUnloadModule(int handle);

        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern int BioRFIDCapture(int handle, out IntPtr data, out uint dataLen, int timeout);

        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern void BioRFIDRelease(int handle, IntPtr data);

        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern int BioRFIDValidate(int handle, IntPtr data, uint dataLen, int timeout);

        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern IntPtr BioRFIDAddTemplate(int handle, IntPtr templates, IntPtr data, uint dataLen);

        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern void BioRFIDReleaseTemplates(int handle, IntPtr templates);

        [DllImport(".\\Resources\\Biovein\\libbiorfidwrapper.dll")]
        static extern IntPtr BioRFIDFind(int handle, IntPtr templates, int timeout);

        [StructLayout(LayoutKind.Sequential)]
        public class TemplateItem
        {
            public IntPtr buf;
            public uint buflen;
            public IntPtr next;
        }
        #endregion

        public event ChangeLabelStatusBioveinEventArgs ChangeLabelStatusBiovein;
        private XmlTextReader reader;
        private WebBrowser webBrowser;
        private Uri url;
        private Uri urlApplication;
        private TemplateItem currentTemplateValues = new TemplateItem();
        private List<string> allTemplatesUserName = new List<string>();
        private int handle;
        private IntPtr ctvPtr;

        #region Constructeur
        public BioveinManager(WebBrowser web, Uri urlApplication, Uri url)
        {
            this.urlApplication = urlApplication;
            this.url = url;
            this.webBrowser = web;
            StringBuilder sb = new StringBuilder();
            string tempDirectory = Directory.GetCurrentDirectory();
            sb.Append(tempDirectory);
            sb.Append("\\Resources\\Biovein\\");
            Directory.SetCurrentDirectory(sb.ToString());
            this.handle = BioRFIDLoadModuleA("brwlibhitachifingerveinh1.dll");
            currentTemplateValues.next = IntPtr.Zero;
            Directory.SetCurrentDirectory(tempDirectory);
        }
        #endregion

        #region Propriétés
        public WebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { webBrowser = value; }
        }
        #endregion

        //Fonction: On telecharge un flux XML contenant la liste des empreintes/nom/taille des données
        public void DownloadXML()
        {
            Program.LogFich.Info("[BioveinManager] DownloadXML() - [DEBUT] Telechargement XML[" + Functions.getHost() + "]");
            string getVars = "?machine=" + Functions.getHost();
            string adresse = urlApplication + "devices/HITACHI_BIOVEIN_XML.asp" + getVars;

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
                Program.LogFich.Info("[BioveinManager] DownloadXML() - [FIN] Telechargement xml termine");
            }
            catch (Exception ex)
            {
                Program.LogFich.Error("[BioveinManager] DownloadXML() - [FIN] Erreur de réception xml:" + ex.ToString());
            }
        }

        //Fonction: Fonction qui détruit le template (destructeur)
        public void UnloadData()
        {
            try
            {
                BioRFIDReleaseTemplates(handle, ctvPtr);
                //Marshal.Release(ctvPtr);
                BioRFIDUnloadModule(handle);
            }
            catch (Exception)
            {

            }

        }

        //Fonction: Fonction qui traite le flux XML, création du template
        public void TraiterXML()
        {
            Program.LogFich.Info("[BioveinManager] TraiterXML() - [DEBUT] Traitement de la liste des empreintes");

            string nomElement = "";
            try
            {
                string login = "";
                string dataStr = "";
                string datalenStr = "";

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            nomElement = reader.Name;
                            if (nomElement.Equals("empreinte"))
                            {
                                login = "";
                                dataStr = "";
                                datalenStr = "";
                            }
                            break;

                        case XmlNodeType.EndElement:
                            nomElement = reader.Name;
                            if (nomElement.Equals("empreinte"))
                            {
                                Program.LogFich.Info("[BioveinManager] TraiterXML() - Login:" + login);
                                Program.LogFich.Info("[BioveinManager] TraiterXML() - Data:" + dataStr);
                                Program.LogFich.Info("[BioveinManager] TraiterXML() - DataLEN:" + datalenStr);
                                Program.LogFich.Info("");
                                //on convertit string en byte[]
                                byte[] data = new byte[uint.Parse(datalenStr)];
                                int i = 0, j = 0;
                                StringBuilder byt = new StringBuilder(100);

                                while (j < dataStr.Length && i < uint.Parse(datalenStr))
                                {
                                    if (dataStr[j].Equals(' '))
                                    {
                                        data[i] = byte.Parse(byt.ToString());
                                        i++;
                                        byt = new StringBuilder(100);
                                    }
                                    else
                                    {
                                        byt.Append(dataStr.ToString().Substring(j, 1));

                                        if (j == dataStr.Length - 1)
                                        {
                                            data[i] = byte.Parse(byt.ToString());
                                        }
                                    }
                                    j++;
                                }

                                IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
                                Marshal.Copy(data, 0, dataPtr, data.Length);

                                IntPtr retPtr = IntPtr.Zero;

                                if (allTemplatesUserName.Count == 0) // First add, the structure is initialized
                                {
                                    retPtr = BioRFIDAddTemplate(handle, IntPtr.Zero, dataPtr, uint.Parse(datalenStr));  // Initialize the chained list by passing a "null" argument
                                }
                                else
                                {
                                    IntPtr tmpPtr = Marshal.AllocHGlobal(Marshal.SizeOf(currentTemplateValues));
                                    Marshal.StructureToPtr(currentTemplateValues, tmpPtr, true);

                                    retPtr = BioRFIDAddTemplate(handle, tmpPtr, dataPtr, uint.Parse(datalenStr));      // Passing the first argument of the chained list
                                    Marshal.Release(tmpPtr);
                                }

                                if (retPtr != IntPtr.Zero)
                                {
                                    Marshal.PtrToStructure(retPtr, currentTemplateValues);
                                    allTemplatesUserName.Add(login);
                                }
                            }
                            break;

                        case XmlNodeType.Text:

                            if (nomElement.Equals("login"))
                            {
                                login = reader.Value.ToString();
                            }
                            else if (nomElement.Equals("data"))
                            {
                                dataStr = reader.Value.ToString();
                            }
                            else if (nomElement.Equals("datalen"))
                            {
                                datalenStr = reader.Value.ToString();
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[BioveinManager] TraiterXML() - ERREUR:" + e.ToString());
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
            }
            PrintTemplates();
            Program.LogFich.Info("[BioveinManager] TraiterXML() - [FIN] Traitement de la liste des empreintes terminé");
        }

        //Fonction: Lancer le module de lecture de doigt en mode ajouter
        public void AddData()
        {
            Program.LogFich.Info("[BioveinManager] AddData() - [DEBUT]");
            ctvPtr = IntPtr.Zero;

            if (handle != 0)
            {
                try
                {
                    IntPtr dataPtr = IntPtr.Zero;
                    uint dataLen = 0;
                    int ret = BioRFIDCapture(handle, out dataPtr, out dataLen, 5000);
                    if (ret > 0)
                    {
                        IntPtr retPtr = IntPtr.Zero;
                        SendDataToAdd(dataPtr, dataLen);
                        ChangeLabelStatusBiovein("Status : Adding Finger...[Succeeded]");
                        Program.LogFich.Info("[BioveinManager] AddData() - [FIN] OK ");
                    }
                    else
                    {
                        ChangeLabelStatusBiovein("Status : Adding Finger...[Failed]");
                    }
                }
                catch (Exception ex)
                {
                    ChangeLabelStatusBiovein("Status : Reading Finger...[Failed]");
                    Program.LogFich.Error("[BioveinManager] AddData() - [FIN] Erreur: " + ex.ToString());
                }
            }
        }

        //Fonction: Envoie des données du doigts a ajouter
        public bool SendDataToAdd(IntPtr dataPtr, uint dataLen)
        {
            ChangeLabelStatusBiovein("Status : Sending data...");
            Program.LogFich.Info("[BioveinManager] SendDataToAdd() - Envoie des données");
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
                sb.Append("type_peripherique=").Append(HttpUtility.UrlEncode("hitachi", Encoding.ASCII));
                sb.Append("&machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                sb.Append("&data=").Append(HttpUtility.UrlEncode(dataSB.ToString(), Encoding.ASCII));
                sb.Append("&datalen=").Append(HttpUtility.UrlEncode(dataLen.ToString(), Encoding.ASCII));

                byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());

                StringBuilder adr = new StringBuilder();
                adr.Append(this.url.ToString());
                adr.Append(this.webBrowser.Url.Query);
                Uri adresse = new Uri(adr.ToString());
                webBrowser.Navigate(adresse, "", postBytes, "Content-Type: application/x-www-form-urlencoded");

                ChangeLabelStatusBiovein("Status : Sending data...[Succeeded]");
                Program.LogFich.Info("[BioveinManager] SendDataToAdd() - Envoie termine - Trame envoyee = " + sb.ToString());
            }
            catch (Exception e)
            {
                b = false;
                ChangeLabelStatusBiovein("Status : Sending data...[Failed]");
                Program.LogFich.Error("[BioveinManager] SendDataToAdd() - Erreur d'envoie = " + e.ToString());
            }
            return b;
        }

        //Fonction: Envoie des données du doigts authentifié
        public bool SendDataToRead(string login)
        {
            ChangeLabelStatusBiovein("Status : Sending data...");
            Program.LogFich.Info("[BioveinManager] SendDataToRead() - Envoie des données");
            bool b = true;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("type_peripherique=").Append(HttpUtility.UrlEncode("hitachi", Encoding.ASCII));
                sb.Append("&machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                sb.Append("&login=").Append(HttpUtility.UrlEncode(login, Encoding.ASCII));

                if (this.webBrowser.Document.GetElementById("ipopup") != null)
                {
                    NameValueCollection qscoll = HttpUtility.ParseQueryString(this.webBrowser.Document.Window.Frames["ipopup"].Url.Query);

                    sb.Append("&url=").Append(HttpUtility.UrlEncode(qscoll["url"]));
                    sb.Append("&title=").Append(HttpUtility.UrlEncode(qscoll["title"]));
                    sb.Append("&l=").Append(HttpUtility.UrlEncode(qscoll["l"]));
                    sb.Append("&h=").Append(HttpUtility.UrlEncode(qscoll["h"]));
                    sb.Append("&priseposte=").Append(HttpUtility.UrlEncode(qscoll["priseposte"]));
                }
                else
                    sb.Append("&url=").Append(HttpUtility.UrlEncode(this.webBrowser.Document.GetElementsByTagName("form")[0].GetAttribute("Action")));

                byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());

                Uri adresse = new Uri(url.ToString());
                webBrowser.Navigate(adresse, "", postBytes, "Content-Type: application/x-www-form-urlencoded");

                ChangeLabelStatusBiovein("Status : Sending data...[Succeeded]");
                Program.LogFich.Info("[BioveinManager] SendDataToRead() - Envoie termine - Trame envoyee = " + sb.ToString());
            }
            catch (Exception e)
            {
                b = false;
                ChangeLabelStatusBiovein("Status : Sending data...[Failed]");
                Program.LogFich.Error("[BioveinManager] SendDataToRead() - Erreur d'envoie = " + e.ToString());
            }
            return b;
        }

        //Fonction: Lancer le module de lecture de doigt en mode authentification
        public void ReadData()
        {
            //global;
            Program.LogFich.Info("[BioveinManager] ReadData() - [DEBUT]");
            string login;

            if (handle != 0)
            {
                try
                {
                    ctvPtr = Marshal.AllocHGlobal(Marshal.SizeOf(currentTemplateValues));
                    Marshal.StructureToPtr(currentTemplateValues, ctvPtr, true);

                    IntPtr matchedTemplatePtr = BioRFIDFind(handle, ctvPtr, 10000);
                    if (matchedTemplatePtr != IntPtr.Zero)
                    {
                        Program.LogFich.Info("[BioveinManager] ReadData() - Looking for a user...");
                        bool found = false;
                        int counter = 0;

                        // Looking for the index of the matched template to find the user name
                        for (IntPtr currentTemplatePtr = ctvPtr; !found && currentTemplatePtr != IntPtr.Zero; ++counter)
                        {
                            TemplateItem item = new TemplateItem();
                            Marshal.PtrToStructure(currentTemplatePtr, item);

                            if (currentTemplatePtr == matchedTemplatePtr)
                            {
                                found = true;
                            }
                            else
                            {
                                currentTemplatePtr = item.next;
                            }
                        }

                        if (found && counter > 0 && counter <= allTemplatesUserName.Count)
                        {
                            Program.LogFich.Info("[BioveinManager] ReadData() - The user {" + allTemplatesUserName[counter - 1] + "} has been successfully validated !");
                            login = allTemplatesUserName[counter - 1].ToString();
                            SendDataToRead(login);
                            ChangeLabelStatusBiovein("Status : Finger found...[" + login + "][Succeeded]");
                        }
                        else
                        {
                            Program.LogFich.Info("[BioveinManager] ReadData() - Unable to find the user name !");
                        }
                    }
                    else
                    {
                        ChangeLabelStatusBiovein("Status : Finger unknown...[Failed]");
                        Program.LogFich.Info("[BioveinManager] ReadData() - No user validated with this fingerprint");
                    }

                    Marshal.Release(ctvPtr);
                    ctvPtr = Marshal.AllocHGlobal(Marshal.SizeOf(currentTemplateValues));
                    Marshal.StructureToPtr(currentTemplateValues, ctvPtr, true);
                    Program.LogFich.Info("[BioveinManager] ReadData() - OK [FIN]");

                }
                catch (Exception ex)
                {
                    ChangeLabelStatusBiovein("Status : Reading Finger...[Failed]");
                    Program.LogFich.Error("[BioveinManager] ReadData() - Error: " + ex.ToString());
                }
            }
        }

        //Fonction: Affiche le contenue du template
        public void PrintTemplates()
        {
            Program.LogFich.Info("[BioveinManager] PrintTemplates()- ### Templates list begins ###");
            if (currentTemplateValues != null)
            {
                IntPtr ctvPtr = Marshal.AllocHGlobal(Marshal.SizeOf(currentTemplateValues));
                Marshal.StructureToPtr(currentTemplateValues, ctvPtr, true);
                int counter = 0;

                for (IntPtr currentTemplatePtr = ctvPtr; currentTemplatePtr != IntPtr.Zero; ++counter)
                {
                    TemplateItem item = new TemplateItem();
                    Marshal.PtrToStructure(currentTemplatePtr, item);

                    if (item != null && counter < allTemplatesUserName.Count)
                    {
                        Program.LogFich.Info("[BioveinManager] PrintTemplates() - Item " + counter + ": " + allTemplatesUserName[counter] + " || " + item.buf.ToString() + " || " + item.buflen.ToString());
                        byte[] data = new byte[item.buflen];

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < item.buflen; ++i)
                        {
                            data[i] = Marshal.ReadByte(item.buf, i);
                            sb.Append(data[i]);
                        }
                        Program.LogFich.Info("[BioveinManager] PrintTemplates() : " + sb.ToString());
                    }
                    else if (item == null)
                    {
                        Program.LogFich.Info("[BioveinManager] PrintTemplates()- Item is NULL ");
                    }
                    else
                    {
                        Program.LogFich.Info("[BioveinManager] PrintTemplates() : template vide");
                    }
                    currentTemplatePtr = item.next;
                }
            }
            Program.LogFich.Info("[BioveinManager] PrintTemplates()- ### Templates list ends ###");
        }
    }
}
