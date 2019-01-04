#region Using
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using log4net;
using log4net.Config;
using SandBox.Periph_Biovein.Periph_Biovein_Eden;
using SandBox.Periph_IrisCard;
using SandBox.Periph_Sign_Screen;
using SandBox.Periph_Sign_Screen_Tablet;
using SandBox.Periph_Topaz;
using SandBox.Periph_Webcam;
using STUTablet;
#endregion

namespace SandBox
{
    public delegate void ChangeLabelStatusIDataEventArgs(string text);
    public delegate void ChangeLabelStatusFormPeripheriqueEventArgs(string text);
    public delegate void ChangeLabelStatusBioveinEventArgs(string text);
    public delegate void ChangeLabelStatusEventArgs(string text);


    public partial class Main : Form
    {

        public SHDocVw.WebBrowser_V1 Web_V1;					//Interface to expose ActiveX methods
        private PeripheriqueManager peripheriqueManager;        //Gestion de la liste des périphériques du poste
        private OptionsAdminManager optionsAdminManager;        //Gestion des options de verrouillage du poste
        private BioveinManager bioVeinManager;                  //Gestion du périphériphe BioVein
        private SriSocketServeur sriSocketServeur;              //Gestion de la création serveur socket
        private string titreApplication;                        //Titre de l'application
        private Uri urlApplication;								//URL de l'application pas static...
        private Icon iconeApplication;                          //Icone de l'application
        private WebBrowser webFille;                            //Page web fille
        private Form Popup;                                     //Form fille
        private string nomApplication;                          //Nom de l'application                
        //private string AppNom;        
        private Uri urlico;										//url de l'icone
        private static System.Object lockThis = new System.Object();	//Utilisé pour la gestion des deadlock
        private Topaz_Manager TopazManager;						//Gestion de la tablette à signature topaz
        private Sign_Screen_Manager SignScreenManager;						//Gestion de la tablette à signature
        private Sign_Screen_Tablet_Manager SignScreenTabletManager;   //Gestion signature pour tablet
        private Periph_Webcam_Manager Form_webcam;				//Gestion de toute les webcams USB "standard"
        private IrisCard_Manager Form_Iriscan;					//Gestion du scanner IRISCARD forma A6        
        private BioveinEdenManager BioveinEden;
        private ScannerPiece ARH_perif;                         //ARH Combo smart, créer l'objet Peripherique en avance pour la préchauffe

        private string powerscan_lecteur;                       //Numéro du lecteur POWERSCAN
        private LecteurPortSerie powerscan_perif;               //Objet perif port serie POWERSCAN
        private Timer powerscan_timer;                          //Objet timer POWERSCAN
        private int powerscan_seconde;                          //Compteur en seconde pour le timer POWERSCAN

        private Timer beep_timer;                               //Objet timer beep  

        private FormProgressBar pb = null;

        private static readonly ILog LogFich = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool arhFlag = false;

        private bool isFirstTime = true;
        public bool pageready = false;

        public Main()
        {
            InitializeComponent();

            this.LabelCopyright.Text = "Copyright © SafeWare " + DateTime.Today.Year;

            Program.LogFich.Info("[MAIN] init config log4net");

            XmlConfigurator.Configure(new FileInfo(Directory.GetCurrentDirectory() + ".\\log4net.config"));
            log4net.Config.BasicConfigurator.Configure();

            Program.LogFich.Info("[MAIN] recup url application ico");
            urlico = new Uri(SandBox.Properties.Settings.Default.urlApplication);

            RecupererUrl();

            Web_V1 = (SHDocVw.WebBrowser_V1)this.webBrowser.ActiveXInstance;            ///Setup Web_V1 interface and register event handler 
            Web_V1.NewWindow += new SHDocVw.DWebBrowserEvents_NewWindowEventHandler(Web_V1_NewWindow);
            RecupererNomApplication();

            // Ces trois fonctions font appel à l'url du site web
            /*RecupererIcone();
            RecupererVersion();
            RecupererPeripheriques();*/

            if (titreApplication.Equals("OMNIGARDE"))
            {
                //RecupererOptionsAdmin();
            }
            MiseEnPlaceRaccourcis();

#if DEBUG
            webBrowser.IsWebBrowserContextMenuEnabled = true;
            webBrowser.WebBrowserShortcutsEnabled = true;
#else
            if (SandBox.Properties.Settings.Default.debug == false)
            {
                webBrowser.IsWebBrowserContextMenuEnabled = true;
                webBrowser.WebBrowserShortcutsEnabled = true;
            }
#endif
            //webBrowser.ScriptErrorsSuppressed = true;

            //Debug.Print(AppNumberPublic.ToString());

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Tag.Equals("0"))
            {
                if (optionsAdminManager != null)
                {
                    if (optionsAdminManager.EnableClose == false)
                    {
                        e.Cancel = true;
                        base.OnClosing(e);
                        MessageBox.Show(this, SandBox.Properties.Settings.Default.LibelleMessageQuitter);
                    }
                    else
                    {
                        string s = urlApplication.AbsoluteUri;
                        s += "index.asp?quit=true";
                        Uri url = new Uri(urlApplication, s);
                        this.Tag = "1";
                        webBrowser.Navigate(url);

                        optionsAdminManager.GestionArretOptionsAdmin();
                        if (sriSocketServeur != null)
                        {
                            sriSocketServeur.Stop();
                        }
                        if (bioVeinManager != null)
                        {
                            bioVeinManager.UnloadData();
                        }
                    }
                }
                else
                {
                    string s = urlApplication.AbsoluteUri;
                    s += "index.asp?quit=true";
                    Uri url = new Uri(urlApplication, s);
                    this.Tag = "1";
                    webBrowser.Navigate(url);

                    if (sriSocketServeur != null)
                    {
                        sriSocketServeur.Stop();
                    }
                    if (bioVeinManager != null)
                    {
                        bioVeinManager.UnloadData();
                    }

                }
            }
        }

        #region Fonctions pour gérer la sandbox/siteweb
        private void webFille_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //hideProgressBar();

            /* récupération dynamique du titre de la fenêtre */
            Popup.Text = webFille.DocumentTitle;
        }

        private void webFille_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //showProgressBar();
        }

        private void webFille_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //hideProgressBar();
        }


        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
            /*if ((sender as WebBrowser).ReadyState == WebBrowserReadyState.Complete)
            {
                return;
            }*/

            LogFich.Debug("URL=" + webBrowser.Url.ToString());

            /* Si on est sur la page login.asp et qu'on a pas de paramètre machine, on l'ajoute à l'URL et on recharge la page */
            string urlFrame = "";
            if (webBrowser.Document.Window.Frames != null && webBrowser.Document.Window.Frames.Count > 0)
            {
                urlFrame = webBrowser.Document.Window.Frames[0].Url.AbsoluteUri;
            }
            if (webBrowser.Url.AbsoluteUri.Contains("/login.asp") || webBrowser.Url.AbsoluteUri.Contains("/before_verif_login.asp") || urlFrame.Contains("/quitter.asp") || webBrowser.Url.AbsoluteUri.Contains("/choix_app.asp"))
            {

                Uri tempURI;
                if (urlFrame != "")
                    tempURI = webBrowser.Document.Window.Frames[0].Url;
                else
                    tempURI = webBrowser.Url;

                if (!tempURI.AbsoluteUri.Contains("sandbox="))
                {
                    string s = tempURI.AbsoluteUri;
                    if (string.IsNullOrEmpty(tempURI.Query))
                        s = s + "?machine=" + Functions.getHost();
                    else
                        s = s.Replace(tempURI.Query, "?machine=" + Functions.getHost());

                    LogFich.Info("MachineName = " + Functions.getHost());
                    if (!statusStrip.Items[5].Text.Contains(Functions.getHost()))
                    {
                        //System.Windows.Forms.ToolStripStatusLabel statusMachine = new System.Windows.Forms.ToolStripStatusLabel(Functions.getHost());
                        //statusMachine.Click += new EventHandler(statusMachine_Click);
                        //statusStrip.Items.Insert(5, statusMachine);
                        this.LabelStatusMachineName.Text = Functions.getHost();
                        this.LabelStatusMachineName.Click -= new EventHandler(statusMachine_Click);
                        this.LabelStatusMachineName.Click += new EventHandler(statusMachine_Click);
                    }

                    s += "&sandbox=true";
                    Uri url = new Uri(s);

                    if (isFirstTime)
                    {
                        /*while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                        {
                            Application.DoEvents();
                            //System.Threading.Thread.Sleep(100);
                        };*/
                        //WaitForPageLoad();

                        webBrowser.Hide();

                        RecupererIcone();
                        RecupererVersion();

                        if (!SandBox.Properties.Settings.Default.cookieHttpOnly)
                        {
                            RecupererPeripheriques();
                        }

                        isFirstTime = false;

                        if (urlFrame != "")
                            webBrowser.Document.Window.Frames[0].Navigate(url);
                        else
                            webBrowser.Url = url;
                        /*while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                        {
                            Application.DoEvents();
                            System.Threading.Thread.Sleep(100);
                        };*/
                        webBrowser.Show();

                    }
                    else
                    {
                        if (urlFrame != "")
                            webBrowser.Document.Window.Frames[0].Navigate(url);
                        else
                            webBrowser.Url = url;
                    }

                }
            }

            //hideProgressBar();
            if (this.Tag.Equals("1"))
            {
                optionsAdminManager.EnableClose = true;
                this.Close();
                return;
            }

            if (webBrowser.Url.Query.Contains("quit=true"))
            {
                if (optionsAdminManager != null)
                {
                    optionsAdminManager.EnableClose = true;
                    this.Close();
                    return;
                }
                else
                {
                    this.Close();
                    return;
                }
            }

            this.Text = webBrowser.DocumentTitle;
            this.titreApplication = this.Text;
            this.LabelSiteName.Text = this.Text;
        }

        private void statusMachine_Click(object sender, System.EventArgs e)
        {
            if (SandBox.Properties.Settings.Default.cookieHttpOnly)
            {
                webBrowser.Hide();
            }
            Uri url = webBrowser.Url;
            RecupererPeripheriques();
            //MessageBox.Show(this, "List of devices updated.");
            if (SandBox.Properties.Settings.Default.cookieHttpOnly)
            {
                pageready = false;
                webBrowser.Navigate(url);
                //WaitForPageLoad();
                while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                    //System.Threading.Thread.Sleep(100);
                };
                webBrowser.Show();
            }

        }

        private void Web_V1_NewWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed)
        {
            Processed = true; //Stop event from being processed

            if (webFille == null)
            {
                webFille = new WebBrowser();
                webFille.AllowWebBrowserDrop = false;
                webFille.Dock = System.Windows.Forms.DockStyle.Fill;
                webFille.Location = new System.Drawing.Point(0, 0);
                webFille.Margin = new System.Windows.Forms.Padding(0);
                webFille.MinimumSize = new System.Drawing.Size(800, 600);
                webFille.Name = "webFille";
                webFille.Size = new System.Drawing.Size(1008, 708);
                webFille.TabIndex = 2;

                Popup = new Form();
                Popup.MinimumSize = new System.Drawing.Size(1024, 768);
                Popup.Icon = iconeApplication;
                Popup.Text = titreApplication;
                Popup.Controls.Add(webFille);
                Popup.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
                Popup.ControlBox = true;
                Popup.WindowState = FormWindowState.Normal;

                //Popup.Web_V1.NewWindow -= new SHDocVw.DWebBrowserEvents_NewWindowEventHandler(Web_V1_NewWindow);
                Popup.Show();
            }

            Popup.Focus();
            Popup.FormClosed += new FormClosedEventHandler(Popup_FormClosed);


            webFille.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.webFille_Navigating);

            webFille.Url = new System.Uri("", System.UriKind.Relative);
            webFille.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webFille_Navigated);
            webFille.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webFille_DocumentCompleted);
            webFille.Navigate(URL, "", (Byte[])PostData, Headers);

        }

        void Popup_FormClosed(object sender, FormClosedEventArgs e)
        {
            hideProgressBar();
            try
            {
                webFille.Dispose();
                webFille = null;
                Popup.Dispose();
                Popup = null;
            }
            catch (Exception)
            {
            }

        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            var browser = (WebBrowser)sender;

            if (browser.ReadyState == WebBrowserReadyState.Complete)
            {
                pageready = true;
                Program.LogFich.Info("document completed=" + e.Url.ToString());
            }

            /*if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
            {
                return;
            }*/
            //if ((sender as WebBrowser).ReadyState != WebBrowserReadyState.Complete)
            //return;

            hideProgressBar();

            /*if (webBrowser.Document.Window.Frames.Count == 0 && webBrowser.Url.AbsoluteUri.Contains("/login.asp"))
            {
                RecupererIcone();
                RecupererVersion();
                RecupererPeripheriques();
            }*/

            try
            {
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE").Click -= new HtmlElementEventHandler(BADGE);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE").Click += new HtmlElementEventHandler(BADGE);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE1") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE1").Click -= new HtmlElementEventHandler(BADGE);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE1").Click += new HtmlElementEventHandler(BADGE);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE2") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE2").Click -= new HtmlElementEventHandler(BADGE);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE2").Click += new HtmlElementEventHandler(BADGE);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE3") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE3").Click -= new HtmlElementEventHandler(BADGE);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("BADGE3").Click += new HtmlElementEventHandler(BADGE);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ").Click -= new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ").Click += new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ").Click -= new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ").Click += new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_ADD") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_ADD").Click -= new HtmlElementEventHandler(HITACHI_BIOVEIN_ADD);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_ADD").Click += new HtmlElementEventHandler(HITACHI_BIOVEIN_ADD);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("PERIPH_WEBCAM") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("PERIPH_WEBCAM").Click -= new HtmlElementEventHandler(PERIPH_WEBCAM);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("PERIPH_WEBCAM").Click += new HtmlElementEventHandler(PERIPH_WEBCAM);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS1") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS1").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS1").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS2") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS2").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS2").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS3") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS3").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("TOPAZ_SIGPLUS3").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("DATALOGIC_POWERSCAN_ADD") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("DATALOGIC_POWERSCAN_ADD").Click -= new HtmlElementEventHandler(DATALOGIC_POWERSCAN_ADD);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("DATALOGIC_POWERSCAN_ADD").Click += new HtmlElementEventHandler(DATALOGIC_POWERSCAN_ADD);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("DATALOGIC_POWERSCAN_READ") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("DATALOGIC_POWERSCAN_READ").Click -= new HtmlElementEventHandler(DATALOGIC_POWERSCAN_READ);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("DATALOGIC_POWERSCAN_READ").Click += new HtmlElementEventHandler(DATALOGIC_POWERSCAN_READ);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN").Click += new HtmlElementEventHandler(SIGN_SCREEN);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN1") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN1").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN1").Click += new HtmlElementEventHandler(SIGN_SCREEN);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN2") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN2").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN2").Click += new HtmlElementEventHandler(SIGN_SCREEN);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN3") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN3").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN3").Click += new HtmlElementEventHandler(SIGN_SCREEN);
                }

                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN_TABLET") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN_TABLET").Click -= new HtmlElementEventHandler(SIGN_SCREEN_TABLET);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SIGN_SCREEN_TABLET").Click += new HtmlElementEventHandler(SIGN_SCREEN_TABLET);
                }

                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_ADD") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_ADD").Click -= new HtmlElementEventHandler(HITACHI_BIOVEIN_ADD);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_ADD").Click += new HtmlElementEventHandler(HITACHI_BIOVEIN_ADD);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ").Click -= new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("HITACHI_BIOVEIN_READ").Click += new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("EDEN_BIOVEIN_READ") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("EDEN_BIOVEIN_READ").Click -= new HtmlElementEventHandler(EDEN_BIOVEIN_READ);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("EDEN_BIOVEIN_READ").Click += new HtmlElementEventHandler(EDEN_BIOVEIN_READ);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ZALIX_VEINSECURE_READ") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ZALIX_VEINSECURE_READ").Click -= new HtmlElementEventHandler(ZALIX_VEINSECURE_READ);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ZALIX_VEINSECURE_READ").Click += new HtmlElementEventHandler(ZALIX_VEINSECURE_READ);
                }
                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ZALIX_VEINSECURE_ADD") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ZALIX_VEINSECURE_ADD").Click -= new HtmlElementEventHandler(ZALIX_VEINSECURE_ADD);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ZALIX_VEINSECURE_ADD").Click += new HtmlElementEventHandler(ZALIX_VEINSECURE_ADD);
                }

                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU").Click -= new HtmlElementEventHandler(WACOM_STU);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU").Click += new HtmlElementEventHandler(WACOM_STU);
                }

                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ARH_COMBOSMART") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ARH_COMBOSMART").Click -= new HtmlElementEventHandler(ARH_COMBOSMART);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("ARH_COMBOSMART").Click += new HtmlElementEventHandler(ARH_COMBOSMART);
                }

                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU_AFF") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU_AFF").Click -= new HtmlElementEventHandler(WACOM_STU_AFF);
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU_AFF").Click += new HtmlElementEventHandler(WACOM_STU_AFF);
                }


                //Gestion du message de retour pour le POWERSCAN
                if (powerscan_perif != null)
                {
                    if (powerscan_timer == null)
                    {
                        powerscan_timer = new Timer();
                        powerscan_timer.Tick += new EventHandler(powerscan_timer_Tick);
                        powerscan_timer.Interval = 1000;
                        powerscan_seconde = 1;
                        powerscan_timer.Start();
                    }
                    if (webBrowser.Document.GetElementById("BADGE_MESSAGE") != null && !webBrowser.Document.GetElementById("BADGE_MESSAGE").GetAttribute("value").Equals(""))
                    {
                        string message = "";
                        string format = "";
                        string messageTransmit;

                        message = webBrowser.Document.GetElementById("BADGE_MESSAGE").GetAttribute("value");
                        format = webBrowser.Document.GetElementById("BADGE_FORMAT").GetAttribute("value").ToUpper();

                        if (message.Length < 16)
                        {
                            message = new string(' ', (16 - message.Length) / 2) + message;
                        }

                        switch (format)
                        {
                            case "OK":
                                messageTransmit = "<ESC>E<ESC> " + message + " <ESC>[6q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[5q<ESC>[3q<ESC>[7q<CR>";
                                break;
                            case "ERROR":
                            case "WARNING":
                                messageTransmit = "<ESC>E<ESC> " + message + " <ESC>[7q<ESC>[8q<ESC>[4q<ESC>[5q<ESC>[8q<ESC>[4q<ESC>[5q<ESC>[8q<ESC>[4q<ESC>[5q<ESC>[5q<ESC>[5q<ESC>[5q<ESC>[9q<CR>";
                                break;
                            default:
                                messageTransmit = "<CR>";
                                break;
                        }

                        if (powerscan_lecteur != null)
                        {
                            powerscan_perif.OpenConnection();
                            powerscan_perif.SendMessage(DataLogicManager.transformeStringEnByte(powerscan_lecteur + messageTransmit));

                            powerscan_perif.Annuler();
                            powerscan_perif = null;

                            powerscan_timer.Stop();
                            powerscan_timer = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw;
                LogFich.Error(ex.ToString());
            }

            if (webBrowser.Document.GetElementById("SWISS_READER") != null)
            {
                webBrowser.Document.GetElementById("SWISS_READER").Click -= new HtmlElementEventHandler(SWISS_READER);
                webBrowser.Document.GetElementById("SWISS_READER").Click += new HtmlElementEventHandler(SWISS_READER);
            }
            if (webBrowser.Document.GetElementById("SWISS_READER2") != null)
            {
                webBrowser.Document.GetElementById("SWISS_READER2").Click -= new HtmlElementEventHandler(SWISS_READER);
                webBrowser.Document.GetElementById("SWISS_READER2").Click += new HtmlElementEventHandler(SWISS_READER);
            }
            if (webBrowser.Document.GetElementById("DESKO_EVO") != null)
            {
                webBrowser.Document.GetElementById("DESKO_EVO").Click -= new HtmlElementEventHandler(SWISS_READER);
                webBrowser.Document.GetElementById("DESKO_EVO").Click += new HtmlElementEventHandler(SWISS_READER);
            }
            if (webBrowser.Document.GetElementById("ARH_COMBOSMART") != null)
            {
                webBrowser.Document.GetElementById("ARH_COMBOSMART").Click -= new HtmlElementEventHandler(ARH_COMBOSMART);
                webBrowser.Document.GetElementById("ARH_COMBOSMART").Click += new HtmlElementEventHandler(ARH_COMBOSMART);
            }
            if (webBrowser.Document.GetElementById("MAGNETA_ESCORT2") != null)
            {
                webBrowser.Document.GetElementById("MAGNETA_ESCORT2").Click -= new HtmlElementEventHandler(MAGNETA_ESCORT2);
                webBrowser.Document.GetElementById("MAGNETA_ESCORT2").Click += new HtmlElementEventHandler(MAGNETA_ESCORT2);
            }
            if (webBrowser.Document.GetElementById("DEISTER_GUARDIX") != null)
            {
                webBrowser.Document.GetElementById("DEISTER_GUARDIX").Click -= new HtmlElementEventHandler(DEISTER_GUARDIX);
                webBrowser.Document.GetElementById("DEISTER_GUARDIX").Click += new HtmlElementEventHandler(DEISTER_GUARDIX);
            }
            if (webBrowser.Document.GetElementById("BADGE") != null)
            {
                webBrowser.Document.GetElementById("BADGE").Click -= new HtmlElementEventHandler(BADGE);
                webBrowser.Document.GetElementById("BADGE").Click += new HtmlElementEventHandler(BADGE);
            }
            if (webBrowser.Document.GetElementById("BADGE1") != null)
            {
                webBrowser.Document.GetElementById("BADGE1").Click -= new HtmlElementEventHandler(BADGE);
                webBrowser.Document.GetElementById("BADGE1").Click += new HtmlElementEventHandler(BADGE);
            }
            if (webBrowser.Document.GetElementById("BADGE2") != null)
            {
                webBrowser.Document.GetElementById("BADGE2").Click -= new HtmlElementEventHandler(BADGE);
                webBrowser.Document.GetElementById("BADGE2").Click += new HtmlElementEventHandler(BADGE);
            }
            if (webBrowser.Document.GetElementById("BADGE3") != null)
            {
                webBrowser.Document.GetElementById("BADGE3").Click -= new HtmlElementEventHandler(BADGE);
                webBrowser.Document.GetElementById("BADGE3").Click += new HtmlElementEventHandler(BADGE);
            }
            if (webBrowser.Document.GetElementById("HITACHI_BIOVEIN_ADD") != null)
            {
                webBrowser.Document.GetElementById("HITACHI_BIOVEIN_ADD").Click -= new HtmlElementEventHandler(HITACHI_BIOVEIN_ADD);
                webBrowser.Document.GetElementById("HITACHI_BIOVEIN_ADD").Click += new HtmlElementEventHandler(HITACHI_BIOVEIN_ADD);
            }
            if (webBrowser.Document.GetElementById("HITACHI_BIOVEIN_READ") != null)
            {
                webBrowser.Document.GetElementById("HITACHI_BIOVEIN_READ").Click -= new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
                webBrowser.Document.GetElementById("HITACHI_BIOVEIN_READ").Click += new HtmlElementEventHandler(HITACHI_BIOVEIN_READ);
            }
            if (webBrowser.Document.GetElementById("TOPAZ_SIGPLUS") != null)
            {
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
            }
            if (webBrowser.Document.GetElementById("TOPAZ_SIGPLUS1") != null)
            {
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS1").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS1").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
            }
            if (webBrowser.Document.GetElementById("TOPAZ_SIGPLUS2") != null)
            {
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS2").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS2").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
            }
            if (webBrowser.Document.GetElementById("TOPAZ_SIGPLUS3") != null)
            {
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS3").Click -= new HtmlElementEventHandler(TOPAZ_SIGPLUS);
                webBrowser.Document.GetElementById("TOPAZ_SIGPLUS3").Click += new HtmlElementEventHandler(TOPAZ_SIGPLUS);
            }
            if (webBrowser.Document.GetElementById("PERIPH_WEBCAM_VISI") != null)
            {
                webBrowser.Document.GetElementById("PERIPH_WEBCAM_VISI").Click -= new HtmlElementEventHandler(PERIPH_WEBCAM);
                webBrowser.Document.GetElementById("PERIPH_WEBCAM_VISI").Click += new HtmlElementEventHandler(PERIPH_WEBCAM);
            }
            if (webBrowser.Document.GetElementById("PERIPH_WEBCAM_RESI") != null)
            {
                webBrowser.Document.GetElementById("PERIPH_WEBCAM_RESI").Click -= new HtmlElementEventHandler(PERIPH_WEBCAM);
                webBrowser.Document.GetElementById("PERIPH_WEBCAM_RESI").Click += new HtmlElementEventHandler(PERIPH_WEBCAM);
            }
            if (webBrowser.Document.GetElementById("IRIS_IRISCARD_A6") != null)
            {
                webBrowser.Document.GetElementById("IRIS_IRISCARD_A6").Click -= new HtmlElementEventHandler(IRIS_IRISCARD_A6);
                webBrowser.Document.GetElementById("IRIS_IRISCARD_A6").Click += new HtmlElementEventHandler(IRIS_IRISCARD_A6);
            }
            if (webBrowser.Document.GetElementById("EDEN_BIOVEIN_READ") != null)
            {
                webBrowser.Document.GetElementById("EDEN_BIOVEIN_READ").Click -= new HtmlElementEventHandler(EDEN_BIOVEIN_READ);
                webBrowser.Document.GetElementById("EDEN_BIOVEIN_READ").Click += new HtmlElementEventHandler(EDEN_BIOVEIN_READ);
            }
            if (webBrowser.Document.GetElementById("DATALOGIC_POWERSCAN_READ") != null)
            {
                webBrowser.Document.GetElementById("DATALOGIC_POWERSCAN_READ").Click -= new HtmlElementEventHandler(DATALOGIC_POWERSCAN_READ);
                webBrowser.Document.GetElementById("DATALOGIC_POWERSCAN_READ").Click += new HtmlElementEventHandler(DATALOGIC_POWERSCAN_READ);
            }
            if (webBrowser.Document.GetElementById("DATALOGIC_POWERSCAN_ADD") != null)
            {
                webBrowser.Document.GetElementById("DATALOGIC_POWERSCAN_ADD").Click -= new HtmlElementEventHandler(DATALOGIC_POWERSCAN_ADD);
                webBrowser.Document.GetElementById("DATALOGIC_POWERSCAN_ADD").Click += new HtmlElementEventHandler(DATALOGIC_POWERSCAN_ADD);
            }
            if (webBrowser.Document.GetElementById("MAGNETA_EASYRONDE") != null)
            {
                webBrowser.Document.GetElementById("MAGNETA_EASYRONDE").Click -= new HtmlElementEventHandler(MAGNETA_EASYRONDE);
                webBrowser.Document.GetElementById("MAGNETA_EASYRONDE").Click += new HtmlElementEventHandler(MAGNETA_EASYRONDE);
            }
            if (webBrowser.Document.GetElementById("SURVACOM_TRACING") != null)
            {
                webBrowser.Document.GetElementById("SURVACOM_TRACING").Click -= new HtmlElementEventHandler(SURVACOM_TRACING);
                webBrowser.Document.GetElementById("SURVACOM_TRACING").Click += new HtmlElementEventHandler(SURVACOM_TRACING);
            }
            if (webBrowser.Document.GetElementById("SIGN_SCREEN") != null)
            {
                webBrowser.Document.GetElementById("SIGN_SCREEN").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                webBrowser.Document.GetElementById("SIGN_SCREEN").Click += new HtmlElementEventHandler(SIGN_SCREEN);
            }
            if (webBrowser.Document.GetElementById("SIGN_SCREEN1") != null)
            {
                webBrowser.Document.GetElementById("SIGN_SCREEN1").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                webBrowser.Document.GetElementById("SIGN_SCREEN1").Click += new HtmlElementEventHandler(SIGN_SCREEN);
            }
            if (webBrowser.Document.GetElementById("SIGN_SCREEN2") != null)
            {
                webBrowser.Document.GetElementById("SIGN_SCREEN2").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                webBrowser.Document.GetElementById("SIGN_SCREEN2").Click += new HtmlElementEventHandler(SIGN_SCREEN);
            }
            if (webBrowser.Document.GetElementById("SIGN_SCREEN3") != null)
            {
                webBrowser.Document.GetElementById("SIGN_SCREEN3").Click -= new HtmlElementEventHandler(SIGN_SCREEN);
                webBrowser.Document.GetElementById("SIGN_SCREEN3").Click += new HtmlElementEventHandler(SIGN_SCREEN);
            }
            if (webBrowser.Document.GetElementById("SIGN_SCREEN_TABLET") != null)
            {
                webBrowser.Document.GetElementById("SIGN_SCREEN_TABLET").Click -= new HtmlElementEventHandler(SIGN_SCREEN_TABLET);
                webBrowser.Document.GetElementById("SIGN_SCREEN_TABLET").Click += new HtmlElementEventHandler(SIGN_SCREEN_TABLET);
            }
            if (webBrowser.Document.GetElementById("ZALIX_VEINSECURE_READ") != null)
            {
                webBrowser.Document.GetElementById("ZALIX_VEINSECURE_READ").Click -= new HtmlElementEventHandler(ZALIX_VEINSECURE_READ);
                webBrowser.Document.GetElementById("ZALIX_VEINSECURE_READ").Click += new HtmlElementEventHandler(ZALIX_VEINSECURE_READ);
            }
            if (webBrowser.Document.GetElementById("ZALIX_VEINSECURE_ADD") != null)
            {
                webBrowser.Document.GetElementById("ZALIX_VEINSECURE_ADD").Click -= new HtmlElementEventHandler(ZALIX_VEINSECURE_ADD);
                webBrowser.Document.GetElementById("ZALIX_VEINSECURE_ADD").Click += new HtmlElementEventHandler(ZALIX_VEINSECURE_ADD);
            }
            if (webBrowser.Document.GetElementById("WACOM_STU") != null)
            {
                webBrowser.Document.GetElementById("WACOM_STU").Click -= new HtmlElementEventHandler(WACOM_STU);
                webBrowser.Document.GetElementById("WACOM_STU").Click += new HtmlElementEventHandler(WACOM_STU);
            }
            if (webBrowser.Document.GetElementById("WACOM_STU_AFF") != null)
            {
                webBrowser.Document.GetElementById("WACOM_STU_AFF").Click -= new HtmlElementEventHandler(WACOM_STU_AFF);
                webBrowser.Document.GetElementById("WACOM_STU_AFF").Click += new HtmlElementEventHandler(WACOM_STU_AFF);
            }
            if (webBrowser.Document.GetElementById("alert") != null)
            {
                webBrowser.Document.GetElementById("alert").Click -= new HtmlElementEventHandler(Alert_FrameRefreshed);
                webBrowser.Document.GetElementById("alert").Click += new HtmlElementEventHandler(Alert_FrameRefreshed);
            }

        }

        private void Alert_FrameRefreshed(object o, EventArgs e)
        {
            try
            {
                if (optionsAdminManager != null && optionsAdminManager.OptionAdmin_Beep > 0 && webBrowser.Document.GetElementById("alert_iframe") != null && webBrowser.Document.Window.Frames["alert_iframe"].Document.GetElementById("alarmeBeep") != null)
                {
                    if (optionsAdminManager.OptionAdmin_Beep == 1)
                    {
                        beep();
                        if (beep_timer != null)
                        {
                            beep_timer.Stop();
                            beep_timer = null;
                        }
                    }
                    if (optionsAdminManager.OptionAdmin_Beep == 2)
                    {
                        if (beep_timer == null)
                        {
                            beep_timer = new Timer();
                            beep_timer.Tick += new EventHandler(beep_timer_Tick);
                            beep_timer.Interval = 500;
                            beep_timer.Start();
                        }
                    }
                }
                else
                {
                    if (beep_timer != null)
                    {
                        beep_timer.Stop();
                        beep_timer = null;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        void beep_timer_Tick(object sender, EventArgs e)
        {
            beep();
        }

        private void beep()
        {
            try
            {
                System.Media.SystemSounds.Beep.Play();
                Functions.Beep(1000, 800, 200);
            }
            catch (Exception)
            {
            }
        }

        public void ChangeLabelStatus(string s)
        {
            try
            {
                labelStatusApplication.Text = s;
            }
            catch (Exception)
            {

            }
        }

        private void periphStatus_Click(object sender, EventArgs e)
        {
            foreach (PeripheriqueInfos perif in peripheriqueManager.ListePeripherique)
            {
                if (perif.Nom.Equals(sender.ToString()))
                {
                    MessageBox.Show(perif.ToStringPeriph(), "Device information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void periphStatus_MouseUp(object sender, MouseEventArgs e)
        {
            //MAJ des empreintes pour base access locale
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                try
                {
                    string url_webservice = "";
                    string ws_username = "";
                    string ws_password = "";

                    bool trouve = false;
                    int cpt = 0;

                    while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
                    {
                        PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                        if (perifInfo.Traitement.Equals("ZALIX_VEINSECURE"))
                        {
                            trouve = true;
                            //port = perifInfo.Port.Replace("COM", "");
                            url_webservice = perifInfo.Webservice;
                            ws_username = perifInfo.Username;
                            ws_password = Chiffrement.dechiffre(perifInfo.Password, Chiffrement.password);
                        }
                        else
                            cpt++;
                    }

                    Process proc = new Process();
                    proc.EnableRaisingEvents = false;

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;

                    proc.StartInfo.FileName = ".\\Resources\\ZalixVeinSecure\\veinsecure.exe";

                    //MessageBox.Show("valeur recupere=" + value);
                    //proc.StartInfo.Arguments = "MAJ";
                    proc.StartInfo.Arguments = "MAJ" + " \"" + url_webservice + "\" \"" + ws_username + "\" \"" + ws_password + "\"";

                    proc.Start();
                    string data = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    LogFich.Info("CALL veinsecure.exe " + proc.StartInfo.Arguments);
                }
                catch (Exception ex)
                {
                    LogFich.Error(ex.ToString());
                }
            }
        }

        #endregion

        #region Fonctions pour Recuperer[Données]
        private void RecupererIcone()
        {
            Program.LogFich.Info("[MAIN] RecupererIcone() [DEBUT]");
            /* Récupération dynamique du favicon */

            // Téléchargement
            Byte[] requestHtml = null;

            string NameIcon = "";

            if (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.Contains("TELEMAQUE"))
            {
                NameIcon = "Telemaque";
            }
            else
            {
                NameIcon = "Omnigarde";
            }

            try
            {
                string adresse = "";
                adresse = urlApplication + "favicon.ico";

                if (!SandBox.Properties.Settings.Default.cookieHttpOnly)
                {
                    WebClient wc = new WebClient();
                    //wc.UseDefaultCredentials = true;
                    wc.Credentials = CredentialCache.DefaultNetworkCredentials;

                    // Proxy
                    IWebProxy wp = WebRequest.GetSystemWebProxy();
                    wp.Credentials = CredentialCache.DefaultNetworkCredentials;
                    wc.Proxy = wp;

                    Program.LogFich.Debug("Proxy host = " + wp.GetProxy(urlApplication).AbsoluteUri);
                    //Program.LogFich.Debug("Proxy login = " + System.Security.Principal.WindowsIdentity.GetCurrent());
                    Program.LogFich.Debug("Proxy login = " + CredentialCache.DefaultNetworkCredentials.UserName);

                    var cookies = webBrowser.Document.Cookie;

                    LogFich.Debug("webbrowser cookie = " + cookies);
                    wc.Headers.Add(HttpRequestHeader.Cookie, cookies);
                    LogFich.Debug("header cookie = " + wc.Headers.Get("Cookie"));

                    requestHtml = wc.DownloadData(adresse);

                    wc.Dispose();
                }
                else
                {

                    /*webBrowser.Navigate(adresse);

                    while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    };*/

                    //requestHtml = Functions.ReadToEnd(webbrowserTemp.DocumentStream);

                }

                if (requestHtml != null && requestHtml.Length > 0)
                {
                    using (System.IO.FileStream fsfav = new System.IO.FileStream(Environment.ExpandEnvironmentVariables("%TEMP%\\") + NameIcon + "_ico.ico", System.IO.FileMode.Create, FileAccess.ReadWrite))
                    {
                        fsfav.Write(requestHtml, 0, requestHtml.Length);
                        fsfav.Close();
                        fsfav.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                LogFich.Error(e.ToString());
                //MessageBox.Show(we.ToString());
                return;
            }

            // Création
            try
            {
                WebRequest request = WebRequest.Create(new Uri(Environment.ExpandEnvironmentVariables("%TEMP%\\") + NameIcon + "_ico.ico"));
                WebResponse response = request.GetResponse();
                if (requestHtml != null && requestHtml.Length > 0)
                {
                    using (Stream s = response.GetResponseStream())
                    {
                        Image img = Image.FromStream(s);
                        Bitmap square = new Bitmap(128, 128);
                        Graphics g = Graphics.FromImage(square);
                        int x, y, w, h;
                        float r = (float)img.Width / (float)img.Height;
                        if (r > 1)
                        {
                            w = 128;
                            h = (int)((float)128 / r);
                            x = 0; y = (128 - h) / 2;
                        }
                        else
                        {
                            w = (int)((float)128 * r);
                            h = 128;
                            y = 0; x = (128 - w) / 2;
                        }
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(img, x, y, w, h);
                        g.Flush();
                        g.Dispose();
                        this.Icon = Icon.FromHandle(square.GetHicon());
                        this.iconeApplication = this.Icon;
                    }
                }
                response.Close();
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[MAIN] RecupererIcone() [ERROR] - " + e.ToString());
                return;
            }
            Program.LogFich.Info("[MAIN] RecupererIcone() [FIN]");
        }

        private void RecupererUrl()
        {
            Program.LogFich.Info("[MAIN] RecupererUrl() [DEBUT]");

            urlApplication = new Uri(SandBox.Properties.Settings.Default.urlApplication);

            //webBrowser.Url = urlApplication;
            //webBrowser.Navigate(urlApplication);
            Program.LogFich.Info("[MAIN] RecupererUrl() [FIN] - URL:" + urlApplication);
        }

        private void RecupererNomApplication()
        {
            Program.LogFich.Info("[MAIN] RecupererNomApplication() [DEBUT]");

            this.nomApplication = SandBox.Properties.Settings.Default.nomApplication;
            this.titreApplication = SandBox.Properties.Settings.Default.nomApplication;
            Program.LogFich.Info("[MAIN] RecupererNomApplication() [FIN] - Nom Application : " + nomApplication);
        }

        private void RecupererVersion()
        {
            Program.LogFich.Info("[MAIN] RecupererVersion() [DEBUT]");
            try
            {
                this.LabelProductName.Text = Application.ProductName;

                this.LabelVersion.Text = String.Format("Version {0}", Application.ProductVersion);
                if (ApplicationDeployment.IsNetworkDeployed)
                    this.LabelVersion.Text = String.Format("Version {0}", ApplicationDeployment.CurrentDeployment.CurrentVersion);

                //Permet de lire la version du site web appelé
                string line = null;
                byte[] requestHtml = null;

                if (!SandBox.Properties.Settings.Default.cookieHttpOnly)
                {
                    WebClient wc = new WebClient();
                    //wc.UseDefaultCredentials = true;
                    wc.Credentials = CredentialCache.DefaultNetworkCredentials;

                    // Proxy
                    IWebProxy wp = WebRequest.GetSystemWebProxy();
                    wp.Credentials = CredentialCache.DefaultNetworkCredentials;
                    wc.Proxy = wp;

                    Program.LogFich.Debug("Proxy host = " + wp.GetProxy(urlApplication).AbsoluteUri);
                    //Program.LogFich.Debug("Proxy login = " + System.Security.Principal.WindowsIdentity.GetCurrent());
                    Program.LogFich.Debug("Proxy login = " + CredentialCache.DefaultNetworkCredentials.UserName);

                    var cookies = webBrowser.Document.Cookie;

                    LogFich.Debug("webbrowser cookie = " + cookies);
                    wc.Headers.Add(HttpRequestHeader.Cookie, cookies);
                    LogFich.Debug("header cookie = " + wc.Headers.Get("Cookie"));

                    requestHtml = wc.DownloadData(urlApplication + "/ChangeLog.txt");
                    wc.Dispose();
                }
                else
                {
                    pageready = false;
                    webBrowser.Navigate(urlApplication + "/ChangeLog.txt");

                    /*while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
                    {
                        Application.DoEvents();
                        //System.Threading.Thread.Sleep(100);
                    };*/


                    requestHtml = Functions.ReadToEnd(webBrowser.DocumentStream);

                }

                Stream stream = new MemoryStream(requestHtml);
                StreamReader file = new StreamReader(stream);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("Version"))
                    {
                        int index = line.IndexOf("Version");
                        this.LabelSiteVersion.Text = line.Substring(index, line.Length - index);
                        break;
                    }
                }
                file.Dispose();
                stream.Dispose();
                Program.LogFich.Info("[MAIN] RecupererVersion() [FIN] - " + this.LabelSiteVersion.Text);
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[MAIN] RecupererVersion() [ERROR] - " + e.ToString());
                Program.LogFich.Info("[MAIN] RecupererVersion() [FIN]");
            }

        }

        private void RecupererPeripheriques()
        {
            Program.LogFich.Info("[Main] RecupererPeripheriques() - [DEBUT]");

            peripheriqueManager = new PeripheriqueManager(webBrowser, new Uri(urlApplication, "devices/devices.asp"), this);
            //peripheriqueManager.DownloadXML();
            peripheriqueManager.TraiterXML();

            List<ToolStripStatusLabel> deletedItems = new List<ToolStripStatusLabel>();

            foreach (ToolStripStatusLabel item in statusStrip.Items)
            {
                if (item.Name.StartsWith("device_"))
                {
                    deletedItems.Add(item);
                }
            }

            foreach (ToolStripStatusLabel item in deletedItems)
            {
                statusStrip.Items.Remove(item);
            }

            if (bioVeinManager != null)
            {
                bioVeinManager = null;
            }

            if (SignScreenManager != null)
            {
                SignScreenManager.delete_sigplus();
                SignScreenManager = null;
            }

            if (sriSocketServeur != null)
            {
                sriSocketServeur = null;
            }

            Program.LogFich.Info("[Main] RecupererPeripheriques() - Liste des peripherique:");
            foreach (PeripheriqueInfos perif in peripheriqueManager.ListePeripherique)
            {
                perif.afficher();

                if (perif.Traitement.Equals("HITACHI_BIOVEIN"))
                {
                    bioVeinManager = new BioveinManager(webBrowser, urlApplication, new Uri(urlApplication, "devices/HITACHI_BIOVEIN.asp"));
                }

                if (perif.Traitement.Equals("SIGN_SCREEN"))
                {
                    SignScreenManager = new Sign_Screen_Manager(this.webBrowser, urlApplication, this.iconeApplication);
                    SignScreenManager.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);
                    SignScreenManager.Show();

                    if (SignScreenManager.Sign_Screen == null)
                    {
                        SignScreenManager = null;
                        LogFich.Debug("SignScreenManager=null");
                    }

                }

                if (titreApplication.Equals("OMNIGARDE") && perif.Traitement.Equals("SRI_RADIO"))
                {
                    sriSocketServeur = new SriSocketServeur();
                }

                if (titreApplication.Equals("OMNIGARDE") && perif.Traitement.Equals("ZALIX_VEINSECURE"))
                {
                    //MAJ des empreintes pour base access locale
                    try
                    {
                        string url_webservice = "";
                        string ws_username = "";
                        string ws_password = "";

                        bool trouve = false;
                        int cpt = 0;

                        while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
                        {
                            PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                            if (perifInfo.Traitement.Equals("ZALIX_VEINSECURE"))
                            {
                                trouve = true;
                                //port = perifInfo.Port.Replace("COM", "");
                                url_webservice = perifInfo.Webservice;
                                ws_username = perifInfo.Username;
                                ws_password = Chiffrement.dechiffre(perifInfo.Password, Chiffrement.password);
                            }
                            else
                                cpt++;
                        }

                        Process proc = new Process();
                        proc.EnableRaisingEvents = false;

                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.RedirectStandardOutput = true;

                        proc.StartInfo.FileName = ".\\Resources\\ZalixVeinSecure\\veinsecure.exe";

                        //MessageBox.Show("valeur recupere=" + value);
                        //proc.StartInfo.Arguments = "MAJ";
                        proc.StartInfo.Arguments = "MAJ" + " \"" + url_webservice + "\" \"" + ws_username + "\" \"" + ws_password + "\"";

                        proc.Start();
                        string data = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();

                        //LogFich.Info("Retour=" + data.Split(';')[1].Trim());
                    }
                    catch (Exception ex)
                    {
                        LogFich.Error(ex.ToString());
                    }

                }

                if (perif.Traitement.Equals("ARH_COMBOSMART"))
                {
                    if (arhFlag == false)
                    {
                        this.ARH_perif = new ScannerPiece();
                        ARH_perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);
                        arhFlag = true;
                    }
                    else
                    {

                    }
                }

                if (perif.Traitement.Equals("WACOM_STU_AFF") && !System.IO.File.Exists(Directory.GetCurrentDirectory() + "/libeay32.dll"))
                {
                    try
                    {
                        LogFich.Debug("[WACOM_STU_AFF] copie des fichiers à la racine");
                        CopyAll(new DirectoryInfo(Directory.GetCurrentDirectory() + "/Resources/wacom"), new DirectoryInfo(Directory.GetCurrentDirectory()));
                    }
                    catch (Exception ex)
                    {
                        LogFich.Error(ex.ToString());
                    }

                }

                ToolStripStatusLabel periphStatus = new System.Windows.Forms.ToolStripStatusLabel();

                periphStatus.AutoSize = true;
                periphStatus.Name = "device_" + perif.Nom;
                periphStatus.Spring = true;
                periphStatus.Text = perif.Nom;
                periphStatus.Alignment = ToolStripItemAlignment.Left;
                periphStatus.Click += new System.EventHandler(periphStatus_Click);

                if (titreApplication.Equals("OMNIGARDE") && perif.Traitement.Equals("ZALIX_VEINSECURE"))
                {
                    periphStatus.MouseUp += new MouseEventHandler(periphStatus_MouseUp);
                }

                statusStrip.Items.Insert(5, periphStatus);
            }
            if (peripheriqueManager.ListePeripherique.Count == 0 && SandBox.Properties.Settings.Default.cookieHttpOnly)
            {
                ToolStripStatusLabel periphStatus = new System.Windows.Forms.ToolStripStatusLabel();
                periphStatus.Name = "device_no_device";
                periphStatus.Spring = true;
                periphStatus.Text = SandBox.Properties.Settings.Default.labelRedDevice;
                periphStatus.BackColor = Color.Red;
                periphStatus.ForeColor = Color.White;
                periphStatus.Alignment = ToolStripItemAlignment.Left;
                periphStatus.Padding = new Padding(0);
                periphStatus.Click += new EventHandler(statusMachine_Click);
                //statusStrip.Items.RemoveAt(5);                
                statusStrip.Items.Insert(5, periphStatus);
            }
            if (bioVeinManager != null)
            {
                bioVeinManager.DownloadXML();
                bioVeinManager.TraiterXML();
            }
            Program.LogFich.Info("[Main] RecupererPeripheriques() - [FIN] Récuperation périphériques terminée");
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it’s new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                LogFich.Debug("Copying " + target.FullName + "\\" + fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void RecupererOptionsAdmin()
        {
            Program.LogFich.Info("[Main] RecupererOptionsAdminEnvoie() - [DEBUT] [" + Functions.getHost() + "]");
            optionsAdminManager = new OptionsAdminManager(webBrowser, urlApplication);
            //optionsAdminManager.DownloadXML();
            optionsAdminManager.TraiterXML();
            optionsAdminManager.GestionDemarrageOptionsAdmin(this);
            Program.LogFich.Info("[Main] RecupererOptionsAdminEnvoie() - [FIN] Récuperation terminée");
        }

        //Fonction: permet lors de l'installation (ou premier démarrage) la mise en place des raccourcis dans le menu windows
        private void MiseEnPlaceRaccourcis()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                    if (ad.IsFirstRun)  //first time user has run the app since installation or update
                    {
                        Assembly code = Assembly.GetExecutingAssembly();
                        string company = string.Empty;
                        string description = string.Empty;
                        if (Attribute.IsDefined(code, typeof(AssemblyCompanyAttribute)))
                        {
                            AssemblyCompanyAttribute ascompany = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(code, typeof(AssemblyCompanyAttribute));
                            company = ascompany.Company;
                        }
                        if (Attribute.IsDefined(code, typeof(AssemblyDescriptionAttribute)))
                        {
                            AssemblyDescriptionAttribute asdescription = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(code, typeof(AssemblyDescriptionAttribute));
                            description = asdescription.Description;
                        }
                        if (company != string.Empty && description != string.Empty)
                        {
                            //Mise en place du raccourci de l'application sur le bureau (Raccourci à parti du dossier Program File)
                            StringBuilder desktopPath = new StringBuilder();
                            desktopPath.Append(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                            desktopPath.Append("\\");
                            desktopPath.Append(nomApplication);
                            desktopPath.Append(" - ");
                            desktopPath.Append(description);
                            desktopPath.Append(".appref-ms");
                            Program.LogFich.Info("[MAIN] MiseEnPlaceRaccourcis() - desktopPath = " + desktopPath);

                            StringBuilder shortcutName = new StringBuilder();
                            shortcutName.Append(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
                            shortcutName.Append("\\");
                            shortcutName.Append(company);
                            shortcutName.Append("\\");
                            shortcutName.Append(nomApplication);
                            shortcutName.Append(" - ");
                            shortcutName.Append(description);
                            shortcutName.Append(".appref-ms");

                            Program.LogFich.Info("[MAIN] MiseEnPlaceRaccourcis() - shortcutName = " + shortcutName.ToString());
                            System.IO.File.Copy(shortcutName.ToString(), desktopPath.ToString(), true);

                            //Mise en place des raccourci vers le fichiers de config & dossier log
                            //Path du dossier d'installation de la sandbox
                            StringBuilder installation = new StringBuilder();
                            installation.Append(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

                            //Path tu dossier dans Program files
                            StringBuilder destination = new StringBuilder();
                            destination.Append(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
                            destination.Append("\\");
                            destination.Append(company);
                            destination.Append("\\Settings\\");
                            destination.Append(nomApplication);
                            DirectoryInfo repertoire = Directory.CreateDirectory(destination.ToString());

                            //On crée le raccourci pour le dossier LOG
                            StringBuilder path_1 = new StringBuilder();
                            path_1.Append(installation.ToString());
                            path_1.Append("\\log");
                            IWshShell_Class shellClass = new IWshShell_Class();
                            IWshShortcut_Class shellLink = (IWshShortcut_Class)shellClass.CreateShortcut(destination.ToString() + "\\Log.lnk");
                            shellLink.TargetPath = path_1.ToString();
                            //shellLink.Arguments = "-arg";
                            shellLink.RelativePath = "C:/";
                            Program.LogFich.Info("[MAIN] MiseEnPlaceRaccourcis() - Folder log = " + shellLink.TargetPath);
                            shellLink.Save();

                            //On crée le raccourci pour le fichier app.config
                            StringBuilder path_2 = new StringBuilder();
                            path_2.Append(installation.ToString());
                            path_2.Append("\\");
                            path_2.Append(nomApplication);
                            path_2.Append(" - ");
                            path_2.Append(description);
                            path_2.Append(".exe.config");
                            IWshShell_Class shellClass2 = new IWshShell_Class();
                            IWshShortcut_Class shellLink2 = (IWshShortcut_Class)shellClass2.CreateShortcut(destination.ToString() + "\\app.config.lnk");
                            shellLink2.TargetPath = path_2.ToString();
                            //shellLink2.Arguments = "-arg";
                            shellLink2.RelativePath = "C:/";
                            Program.LogFich.Info("[MAIN] MiseEnPlaceRaccourcis() - app.config = " + shellLink2.TargetPath);
                            shellLink2.Save();

                            //On crée le raccourci pour le fichier user.config                           
                            IWshShell_Class shellClass3 = new IWshShell_Class();
                            IWshShortcut_Class shellLink3 = (IWshShortcut_Class)shellClass3.CreateShortcut(destination.ToString() + "\\user.config.lnk");
                            shellLink3.TargetPath = GetDefaultExeConfigPath(ConfigurationUserLevel.PerUserRoamingAndLocal);
                            //shellLink3.Arguments = "-arg";
                            shellLink3.RelativePath = "C:/";
                            shellLink3.Save();
                        }
                    }
                }
                //On crée le raccourci pour le fichier user.config dans le dossier local de l'app
                IWshShell_Class shellClassUser = new IWshShell_Class();
                IWshShortcut_Class shellLinkUser = (IWshShortcut_Class)shellClassUser.CreateShortcut(Path.GetDirectoryName(Application.ExecutablePath).ToString() + "\\user.config.lnk");
                shellLinkUser.TargetPath = GetDefaultExeConfigPath(ConfigurationUserLevel.PerUserRoamingAndLocal);
                shellLinkUser.RelativePath = "C:/";
                Program.LogFich.Info("[MAIN] MiseEnPlaceRaccourcis() - user.config = " + shellLinkUser.TargetPath);
                shellLinkUser.Save();

            }
            catch (Exception e)
            {
                Program.LogFich.Error("[MAIN] MiseEnPlaceRaccourcis() - Erreur : " + e.ToString());
            }
        }

        private static ApplicationId GetDeploymentInfo()
        {
            var appSecurityInfo = new System.Security.Policy.ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
            return appSecurityInfo.DeploymentId;
        }

        private static void CreateClickOnceShortcut(string location)
        {
            var updateLocation = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.UpdateLocation;
            var deploymentInfo = GetDeploymentInfo();
            using (var shortcutFile = new StreamWriter(location, false, Encoding.Unicode))
            {
                shortcutFile.Write(String.Format(@"{0}#{1}, Culture=neutral, PublicKeyToken=",
                updateLocation.ToString().Replace(" ", "%20"),
                deploymentInfo.Name.Replace(" ", "%20")));
                foreach (var b in deploymentInfo.PublicKeyToken)
                    shortcutFile.Write("{0:x2}", b);
                shortcutFile.Write(String.Format(", processorArchitecture={0}", deploymentInfo.ProcessorArchitecture));
                shortcutFile.Close();
            }
        }

        public static string GetDefaultExeConfigPath(ConfigurationUserLevel userLevel)
        {
            try
            {
                var UserConfig = ConfigurationManager.OpenExeConfiguration(userLevel);
                return UserConfig.FilePath;
            }
            catch (ConfigurationException e)
            {
                return e.Filename;
            }
        }

        #endregion

        #region Fonctions pour Gérer les boutons périphériques
        private void SWISS_READER(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement SWISS_READER");
            labelStatusApplication.Text = "Status : [SWISS_READER Processing]";

            Peripherique perif = new EcranClavier();
            perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);

            IData data = new PieceManager(webBrowser);
            data.ChangeLabelStatusIData += new ChangeLabelStatusIDataEventArgs(ChangeLabelStatus);

            FormPeripherique fPerif = new FormPeripherique(perif, data, new Uri(urlApplication, "devices/SWISS_READER.asp"), titreApplication, 30);
            fPerif.ChangeLabelStatusFormPeripherique += new ChangeLabelStatusFormPeripheriqueEventArgs(ChangeLabelStatus);

            fPerif.ShowDialog();
            fPerif.Dispose();
            perif.Annuler();
            perif = null;
        }

        private void WACOM_STU_AFF(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement WACOM_STU_AFF");
            labelStatusApplication.Text = "Status : [WACOM_STU_AFF Processing]";
            try
            {
                STUTabletControl control = new STUTabletControl();

                STUTabletConfig.Logging = false;

                bool result = false;

                string inArgument = "";

                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU_AFF_NUMBER") != null)
                {
                    inArgument = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU_AFF_NUMBER").GetAttribute("value");
                    //webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("WACOM_STU_AFF_NUMBER").SetAttribute("value", "");
                }
                else if (webBrowser.Document.GetElementById("WACOM_STU_AFF_NUMBER") != null)
                {
                    inArgument = webBrowser.Document.GetElementById("WACOM_STU_AFF_NUMBER").GetAttribute("value");
                    //webBrowser.Document.GetElementById("WACOM_STU_AFF_NUMBER").SetAttribute("value", "");
                }

                LogFich.Info("pinpad=" + inArgument);

                if (!string.IsNullOrEmpty(inArgument))
                {
                    System.IO.MemoryStream imageStream = new System.IO.MemoryStream();

                    //creating bitmap image
                    Bitmap bmp = new Bitmap(100, 100);

                    Graphics graphics = Graphics.FromImage(bmp);

                    // Create the Font object for the image text drawing.
                    Font font = new Font("ARIAL", Convert.ToInt32("20"));

                    // Instantiating object of Bitmap image again with the correct size for the text and font.
                    bmp = new Bitmap(bmp, new Size((int)graphics.MeasureString(inArgument, font).Width, (int)graphics.MeasureString(inArgument, font).Height));

                    graphics = Graphics.FromImage(bmp);
                    graphics.Clear(Color.White);
                    graphics.DrawString(inArgument, font, Brushes.Black, 0, 0);
                    graphics.Flush();

                    bmp.Save(imageStream, System.Drawing.Imaging.ImageFormat.Bmp);

                    control.SetImage(imageStream);

                    MessageBox.Show(this, "Verifiez que votre interlocuteur ai bien vu son code");
                    result = control.Clear(0xFFFF);

                    imageStream = null;

                }
                else
                {
                    MessageBox.Show(this, "Pas de code pinpad");
                }
            }
            catch (Exception ex)
            {
                LogFich.Error(ex.ToString());
            }


            labelStatusApplication.Text = "Status : None";
        }


        private void ARH_COMBOSMART(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement ARH_COMBOSMART");
            labelStatusApplication.Text = "Status : [ARH_COMBOSMART Processing]";

            if (this.ARH_perif == null)
            {
                this.ARH_perif = new ScannerPiece();
                ARH_perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);
            }

            PieceManager data = new PieceManager(webBrowser);
            data.ChangeLabelStatusIData += new ChangeLabelStatusIDataEventArgs(ChangeLabelStatus);

            FormPeripherique fPerif = new FormPeripherique(ARH_perif, data, new Uri(urlApplication, "devices/SWISS_READER.asp"), titreApplication, 300);
            fPerif.ChangeLabelStatusFormPeripherique += new ChangeLabelStatusFormPeripheriqueEventArgs(ChangeLabelStatus);

            if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SCAN_ID_VISITORID") != null)
            {
                data.Id_visiteur = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("SCAN_ID_VISITORID").GetAttribute("value");
                ARH_perif.visiteurID = data.Id_visiteur;
            }
            else
            {
                data.Id_visiteur = "";
                ARH_perif.visiteurID = "";
            }


            // Mode lecture
            fPerif.StartPosition = FormStartPosition.CenterScreen;
            //this.Enabled = false;
            fPerif.Show();
            Application.DoEvents();
            ARH_perif.Echange();

            fPerif.Close();
            //this.Enabled = true;
            this.Focus();

            fPerif.Dispose();
            //ARH_perif.Annuler();
            //ARH_perif = null;
        }

        private void HITACHI_BIOVEIN_ADD(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement HITACHI_BIOVEIN_ADD");
            labelStatusApplication.Text = "Status : [BIOVEIN_ADD Processing]";
            if (bioVeinManager == null)
                bioVeinManager = new BioveinManager(webBrowser, urlApplication, new Uri(urlApplication, "devices/HITACHI_BIOVEIN.asp"));
            bioVeinManager.ChangeLabelStatusBiovein += new ChangeLabelStatusBioveinEventArgs(ChangeLabelStatus);
            bioVeinManager.AddData();
        }

        private void HITACHI_BIOVEIN_READ(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement HITACHI_BIOVEIN_READ");
            labelStatusApplication.Text = "Status : [BIOVEIN_READ Processing]";
            if (bioVeinManager == null)
                bioVeinManager = new BioveinManager(webBrowser, urlApplication, new Uri(urlApplication, "devices/HITACHI_BIOVEIN.asp"));
            bioVeinManager.ChangeLabelStatusBiovein += new ChangeLabelStatusBioveinEventArgs(ChangeLabelStatus);
            bioVeinManager.ReadData();
        }

        private void MAGNETA_ESCORT2(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement MAGNETA_ESCORT2");
            labelStatusApplication.Text = "Status : [MAGNETA_ESCORT2 Processing]";
            bool trouve = false;
            int cpt = 0;

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("MAGNETA_ESCORT2"))
                    trouve = true;
                else
                    cpt++;
            }
            peripheriqueManager.ListePeripherique[cpt].afficher();
            MagnetaPortSerie perif = new MagnetaPortSerie(peripheriqueManager.ListePeripherique[cpt].Port,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_seconde,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_donnee,
                                                        peripheriqueManager.ListePeripherique[cpt].Parite,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_arret,
                                                        peripheriqueManager.ListePeripherique[cpt].Controle_flux,
                                                        5000,
                                                        webBrowser);
            perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);

            IData data = perif.MagnetaManager;
            data.ChangeLabelStatusIData += new ChangeLabelStatusIDataEventArgs(ChangeLabelStatus);

            FormPeripherique fPerif = new FormPeripherique(perif, data, new Uri(urlApplication, "devices/MAGNETA_ESCORT2.asp"), titreApplication, 30);
            fPerif.ChangeLabelStatusFormPeripherique += new ChangeLabelStatusFormPeripheriqueEventArgs(ChangeLabelStatus);
            fPerif.ShowDialog();
            fPerif.Close();
            perif.Annuler();
            perif = null;
        }

        private void DEISTER_GUARDIX(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement DEISTER_GUARDIX");
            labelStatusApplication.Text = "Status : [DEISTER_GUARDIX Processing]";

            bool trouve = false;
            int cpt = 0;

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("DEISTER_GUARDIX"))
                    trouve = true;
                else
                    cpt++;
            }

            DeisterPortSerie perif = new DeisterPortSerie(peripheriqueManager.ListePeripherique[cpt].Port,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_seconde,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_donnee,
                                                        peripheriqueManager.ListePeripherique[cpt].Parite,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_arret,
                                                        peripheriqueManager.ListePeripherique[cpt].Controle_flux,
                                                        65,
                                                        webBrowser);
            perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);

            IData data = perif.DeisterManager;
            data.ChangeLabelStatusIData += new ChangeLabelStatusIDataEventArgs(ChangeLabelStatus);

            FormPeripherique fPerif = new FormPeripherique(perif, data, new Uri(urlApplication, "devices/DEISTER_GUARDIX.asp"), titreApplication, 30);
            fPerif.ChangeLabelStatusFormPeripherique += new ChangeLabelStatusFormPeripheriqueEventArgs(ChangeLabelStatus);

            fPerif.ShowDialog();
            fPerif.Dispose();
            perif.Annuler();
            perif = null;
        }

        private void BADGE(object sender, HtmlElementEventArgs e)
        {

            try
            {
                Program.LogFich.Info("[Main] Bouton => Traitement BADGE");
                labelStatusApplication.Text = "Status : [BADGE Processing]";

                bool trouve = false;
                int cpt = 0;
                string nomperiph = "";

                while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
                {
                    PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                    if (perifInfo.Traitement.Equals("BADGE"))
                    {
                        trouve = true;
                        nomperiph = perifInfo.Nom;
                    }
                    else
                        cpt++;
                }

                if (nomperiph.ToUpper().Equals("MIL100"))
                {
                    Form_MIL100 fmil100 = new Form_MIL100(webBrowser, peripheriqueManager.ListePeripherique[cpt].Port.ToString());
                    fmil100.StartPosition = FormStartPosition.Manual;
                    fmil100.Location = new Point(this.Location.X + (this.Width - fmil100.Width) / 2, this.Location.Y + (this.Height - fmil100.Height) / 2);
                    fmil100.Show();
                    Application.DoEvents();
                    fmil100.lecture();
                    return;
                }

                LecteurManager lmanager = new LecteurManager(webBrowser,
                                                            peripheriqueManager.ListePeripherique[cpt].Avant,
                                                            peripheriqueManager.ListePeripherique[cpt].Apres,
                                                            peripheriqueManager.ListePeripherique[cpt].Debut,
                                                            peripheriqueManager.ListePeripherique[cpt].Longueur,
                                                            peripheriqueManager.ListePeripherique[cpt].Longueur_max,
                                                            peripheriqueManager.ListePeripherique[cpt].Decoupe,
                                                            peripheriqueManager.ListePeripherique[cpt].Conversion);
                Peripherique perif = null;

                if (peripheriqueManager.ListePeripherique[cpt].Type_port.Equals("PS2"))
                {
                    perif = new EcranClavierPeriph();
                }
                else
                {
                    perif = new LecteurPortSerie(peripheriqueManager.ListePeripherique[cpt].Port,
                                                                peripheriqueManager.ListePeripherique[cpt].Bit_seconde,
                                                                peripheriqueManager.ListePeripherique[cpt].Bit_donnee,
                                                                peripheriqueManager.ListePeripherique[cpt].Parite,
                                                                peripheriqueManager.ListePeripherique[cpt].Bit_arret,
                                                                peripheriqueManager.ListePeripherique[cpt].Controle_flux,
                                                                65);

                    //perif = (LecteurPortSerie)perif;


                    if (nomperiph.ToUpper().Equals("POWERSCAN"))
                    {
                        powerscan_perif = (LecteurPortSerie)perif;
                        //powerscan_perif.LecteurManager = lmanager;
                    }

                }
                perif.lecteurManager = lmanager;

                perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);
                IData data = perif.lecteurManager;
                //IData data = lmanager;
                data.ChangeLabelStatusIData += new ChangeLabelStatusIDataEventArgs(ChangeLabelStatus);

                int compteur = 30;

                if (webBrowser.Document.GetElementById("BADGE_AUTO") != null)
                {
                    compteur = 3000;
                }

                FormPeripherique fPerif = new FormPeripherique(perif, data, new Uri(urlApplication, ""), titreApplication, compteur);
                fPerif.ChangeLabelStatusFormPeripherique += new ChangeLabelStatusFormPeripheriqueEventArgs(ChangeLabelStatus);
                fPerif.ShowDialog();


                /*if (nomperiph.ToUpper().Equals("MIL100"))
                {
                    string numeroBadgeMIL100 = "";

                    //proc.BeginOutputReadLine();
                    numeroBadgeMIL100 = MIL100.Lecture(peripheriqueManager.ListePeripherique[cpt].Port, LogFich);


                    labelStatusApplication.Text = "Status : Badge...[" + numeroBadgeMIL100 + "]";
                    if (webBrowser.Document.GetElementById("lecteurPhysique") != null)
                    {
                        webBrowser.Document.GetElementById("lecteurPhysique").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(numeroBadgeMIL100));
                        webBrowser.Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                    }
                    else
                    {
                        webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(numeroBadgeMIL100));
                        webBrowser.Document.Window.Frames["ipopup"].Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                    }

                    labelStatusApplication.Text = "Status : None";
                    return;
                }*/

                string numeroBadge = lmanager.NumeroBadgeString;
                Program.LogFich.Info("Numéro badge : " + numeroBadge);
                if (!string.IsNullOrEmpty(numeroBadge))
                {
                    // Récupère le lecteur POWERSCAN qui dialogue
                    if (nomperiph.ToUpper().Equals("POWERSCAN"))
                    {
                        powerscan_lecteur = numeroBadge.Substring(0, 4);
                        numeroBadge = numeroBadge.Substring(4, numeroBadge.Length - 4);
                        Program.LogFich.Info("POWERSCAN numéro lecteur : " + powerscan_lecteur);
                        Program.LogFich.Info("POWERSCAN numéro badge : " + numeroBadge);
                        //powerscan_perif = perif;
                    }

                    // Lance la conversion spécial pour WIEGAND
                    if (nomperiph.ToUpper().Equals("WEIGAND") || nomperiph.ToUpper().Equals("WIEGAND"))
                    {
                        numeroBadge = hidToWiegand(SandBox.Properties.Settings.Default.CodeSiteWIEGAND, numeroBadge);
                    }

                    labelStatusApplication.Text = "Status : Badge...[" + numeroBadge + "]";
                    if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique") != null)
                    {
                        webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(numeroBadge));
                        //webBrowser.Document.Window.Frames["ipopup"].Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                    }
                    else
                    {
                        webBrowser.Document.GetElementById("lecteurPhysique").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(numeroBadge));
                        //webBrowser.Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire  
                    }

                    string numeroBadgeBrut = lmanager.NumeroBadgeBrut;
                    labelStatusApplication.Text = "Status : Badge Brut...[" + numeroBadgeBrut + "]";
                    if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("numeroBadgeBrut") != null)
                    {
                        webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("numeroBadgeBrut").SetAttribute("value", numeroBadgeBrut);
                        //webBrowser.Document.Window.Frames["ipopup"].Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                    }
                    else if (webBrowser.Document.GetElementById("numeroBadgeBrut") != null)
                    {
                        {
                            webBrowser.Document.GetElementById("numeroBadgeBrut").SetAttribute("value", numeroBadgeBrut);
                            //webBrowser.Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire  
                        }
                    }

                }
                else
                {
                    labelStatusApplication.Text = "Status : Badge...[-]";
                    if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique") != null)
                    {
                        webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("lecteurPhysique").SetAttribute("value", "");
                    }
                    else
                    {
                        webBrowser.Document.GetElementById("lecteurPhysique").SetAttribute("value", "");
                    }
                }

                fPerif.Dispose();
                perif.Annuler();
                perif = null;
                labelStatusApplication.Text = "Status : None";
            }
            catch (Exception ex)
            {
                LogFich.Error(ex.ToString());
            }

        }

        private void TOPAZ_SIGPLUS(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement TOPAZ_SIGPLUS");
            ChangeLabelStatus("Status : [TOPAZ_SIGPLUS Processing]");
            if (TopazManager == null)
                TopazManager = new Topaz_Manager(this.urlApplication, this.webBrowser);

            TopazManager.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);
            TopazManager.Lecture_signature(1);
        }

        private void WACOM_STU(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement WACOM_STU");
            ChangeLabelStatus("Status : [WACOM_STU Processing]");
            SandBox.Periph_Wacom_STU.Wacom_STU_Manager STU_Manager = new SandBox.Periph_Wacom_STU.Wacom_STU_Manager(this.urlApplication, this.webBrowser);

            STU_Manager.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);
            STU_Manager.Lecture_signature();
        }

        private void SIGN_SCREEN(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement SIGN_SCREEN");
            ChangeLabelStatus("Status : [SIGN_SCREEN Processing]");

            //RecupererPeripheriques();

            if (SignScreenManager == null)
            {
                //SignScreenManager = new SandBox.Periph_Sign_Screen.Sign_Screen_Manager(this.webBrowser, this.urlApplication);
                //SignScreenManager = new SandBox.Periph_Sign_Screen.Sign_Screen_Manager(this.urlApplication, this.webBrowser, @"O:\Partage\_doc_interxion\SUIVI_IX_SFW_P3P5_2013_04_18_fr.pdf");

                //SignScreenManager.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);
                //SignScreenManager.Show();
            }
            else
            {
                //SignScreenManager.Lecture_signature(@"O:\Partage\_doc_interxion\SUIVI_IX_SFW_P3P5_2013_04_18_fr.pdf");
                string pdf = "", message = "";

                if (webBrowser.Document.GetElementById("ipopup") != null)
                {
                    if (webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf") != null)
                    {
                        pdf = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf").GetAttribute("value");
                    }
                    if (webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_id") != null)
                    {
                        pdf = urlApplication + "gestion/show_file.asp?id=" + webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_id").GetAttribute("value");
                    }
                }
                else
                {
                    if (webBrowser.Document.GetElementById("sign_pdf") != null)
                        pdf = webBrowser.Document.GetElementById("sign_pdf").GetAttribute("value");

                    if (webBrowser.Document.GetElementById("sign_pdf_id") != null)
                        pdf = urlApplication + "gestion/show_file.asp?id=" + webBrowser.Document.GetElementById("sign_pdf_id").GetAttribute("value");
                }


                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_message") != null)
                {
                    message = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_message").GetAttribute("value");
                }
                else
                {
                    if (webBrowser.Document.GetElementById("sign_pdf_message") != null)
                        message = webBrowser.Document.GetElementById("sign_pdf_message").GetAttribute("value");
                }

                SignScreenManager.Lecture_signature(pdf, message);
            }

        }

        private void SIGN_SCREEN_TABLET(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement SIGN_SCREEN_TABLET");
            ChangeLabelStatus("Status : [SIGN_SCREEN_TABLET Processing]");

            //SignScreenManager.Lecture_signature(@"O:\Partage\_doc_interxion\SUIVI_IX_SFW_P3P5_2013_04_18_fr.pdf");
            string pdf = "", message = "";

            if (webBrowser.Document.GetElementById("ipopup") != null)
            {
                if (webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf") != null)
                {
                    pdf = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf").GetAttribute("value");
                }
                if (webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_id") != null)
                {
                    pdf = urlApplication + "/gestion/show_file.asp?id=" + webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_id").GetAttribute("value");
                }
            }
            else
            {
                if (webBrowser.Document.GetElementById("sign_pdf") != null)
                    pdf = webBrowser.Document.GetElementById("sign_pdf").GetAttribute("value");

                if (webBrowser.Document.GetElementById("sign_pdf_id") != null)
                    pdf = urlApplication + "/gestion/show_file.asp?id=" + webBrowser.Document.GetElementById("sign_pdf_id").GetAttribute("value");
            }


            if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_message") != null)
            {
                message = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("sign_pdf_message").GetAttribute("value");
            }
            else
            {
                if (webBrowser.Document.GetElementById("sign_pdf_message") != null)
                    message = webBrowser.Document.GetElementById("sign_pdf_message").GetAttribute("value");
            }

            SignScreenTabletManager = new Sign_Screen_Tablet_Manager(this.webBrowser, urlApplication, this.iconeApplication);
            SignScreenTabletManager.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);
            SignScreenTabletManager.Show();

            SignScreenTabletManager.Lecture_signature(message);


        }

        private void PERIPH_WEBCAM(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement PERIPH_WEBCAM");
            labelStatusApplication.Text = "Status : [PERIPH_WEBCAM Processing]";
            if (Form_webcam == null)
            {
                Form_webcam = new Periph_Webcam_Manager(this.urlApplication, this.webBrowser);
            }
            Form_webcam.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);
            Form_webcam.Open();
            //changeScreen(Form_webcam,SandBox.Properties.Settings.Default.NumEcran1,1);
        }

        private void IRIS_IRISCARD_A6(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement IRIS_IRISCARD_A6");
            labelStatusApplication.Text = "Status : [IRIS_IRISCARD_A6 Processing]";
            if (Form_Iriscan == null)
            {
                Form_Iriscan = new IrisCard_Manager(this.urlApplication, this.webBrowser);
            }
            Form_Iriscan.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);
            Form_Iriscan.Open();
        }

        private void EDEN_BIOVEIN_READ(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement EDEN_BIOVEIN_READ");
            if (BioveinEden == null)
            {
                /*Attention l'un des 2 url est faux*/
                BioveinEden = new BioveinEdenManager(this.webBrowser, this.urlApplication, new Uri(this.urlApplication.ToString() + "devices/EDEN_BIOVEIN.asp"));
                BioveinEden.ChangeLabelStatus += new ChangeLabelStatusEventArgs(ChangeLabelStatus);

            }
            /*téléchargement des infos de la personne à enroler*/
            //BioveinEden.DownloadXML(0);
            //BioveinEden.TraiterXML();			

            string id;
            if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("webcam_visiteurid") != null)
            {
                id = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("webcam_visiteurid").GetAttribute("value");
            }
            else
            {
                id = webBrowser.Document.GetElementById("webcam_visiteurid").GetAttribute("value").ToString();
            }

            BioveinEden.Enrolement(id);
        }

        private void DATALOGIC_POWERSCAN_READ(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement DATALOGIC_POWERSCAN_READ");
            labelStatusApplication.Text = "Status : [DATALOGIC_POWERSCAN_READ Processing]";

            bool trouve = false;
            int cpt = 0;

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("DATALOGIC_POWERSCAN"))
                    trouve = true;
                else
                    cpt++;
            }

            DataLogicManager lmanager = new DataLogicManager(webBrowser,
                                                        peripheriqueManager.ListePeripherique[cpt].Avant,
                                                        peripheriqueManager.ListePeripherique[cpt].Apres,
                                                        peripheriqueManager.ListePeripherique[cpt].Debut,
                                                        peripheriqueManager.ListePeripherique[cpt].Longueur,
                                                        peripheriqueManager.ListePeripherique[cpt].Longueur_max,
                                                        peripheriqueManager.ListePeripherique[cpt].Decoupe,
                                                        peripheriqueManager.ListePeripherique[cpt].Conversion);


            DataLogicPortSerie perif = new DataLogicPortSerie(peripheriqueManager.ListePeripherique[cpt].Port,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_seconde,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_donnee,
                                                        peripheriqueManager.ListePeripherique[cpt].Parite,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_arret,
                                                        peripheriqueManager.ListePeripherique[cpt].Controle_flux,
                                                        65);
            perif.DataLogicManager = lmanager;
            perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);

            IData data = perif.DataLogicManager;
            data.ChangeLabelStatusIData += new ChangeLabelStatusIDataEventArgs(ChangeLabelStatus);

            FormPeripherique fPerif = new FormPeripherique(perif, data, new Uri(urlApplication, ""), titreApplication, 300);
            //fPerif.Parent = this;
            fPerif.ChangeLabelStatusFormPeripherique += new ChangeLabelStatusFormPeripheriqueEventArgs(ChangeLabelStatus);

            // Mode lecture
            fPerif.StartPosition = FormStartPosition.CenterScreen;
            this.Enabled = false;
            fPerif.Show();
            perif.Reader();
            this.Enabled = true;

            if (lmanager.NumeroBadgeString != null)
            {
                labelStatusApplication.Text = "Status : Badge...[" + lmanager.NumeroBadgeString + "]";

                if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("tracking") != null)
                {
                    webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("tracking").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(lmanager.NumeroBadgeString));
                    //webBrowser.Document.Window.Frames["ipopup"].Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                }
                else
                {
                    webBrowser.Document.GetElementById("tracking").SetAttribute("value", System.Web.HttpUtility.HtmlEncode(lmanager.NumeroBadgeString));
                    webBrowser.Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
                }

            }
            else
            {
                labelStatusApplication.Text = "Status : Badge...[-]";
                //webBrowser.Document.GetElementById("lecteurPhysique").SetAttribute("value", "");
            }
            fPerif.Dispose();
            perif.Annuler();
            perif = null;
            labelStatusApplication.Text = "Status : None";
        }

        private void DATALOGIC_POWERSCAN_ADD(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement DATALOGIC_POWERSCAN_ADD");
            labelStatusApplication.Text = "Status : [DATALOGIC_POWERSCAN_ADD Processing]";

            bool trouve = false;
            int cpt = 0;

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("DATALOGIC_POWERSCAN"))
                    trouve = true;
                else
                    cpt++;
            }

            DataLogicManager lmanager = new DataLogicManager(webBrowser,
                                                        peripheriqueManager.ListePeripherique[cpt].Avant,
                                                        peripheriqueManager.ListePeripherique[cpt].Apres,
                                                        peripheriqueManager.ListePeripherique[cpt].Debut,
                                                        peripheriqueManager.ListePeripherique[cpt].Longueur,
                                                        peripheriqueManager.ListePeripherique[cpt].Longueur_max,
                                                        peripheriqueManager.ListePeripherique[cpt].Decoupe,
                                                        peripheriqueManager.ListePeripherique[cpt].Conversion);


            DataLogicPortSerie perif = new DataLogicPortSerie(peripheriqueManager.ListePeripherique[cpt].Port,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_seconde,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_donnee,
                                                        peripheriqueManager.ListePeripherique[cpt].Parite,
                                                        peripheriqueManager.ListePeripherique[cpt].Bit_arret,
                                                        peripheriqueManager.ListePeripherique[cpt].Controle_flux,
                                                        65);
            perif.DataLogicManager = lmanager;
            perif.ChangeLabelStatus += new Peripherique.ChangeLabelStatusEventHandler(ChangeLabelStatus);

            IData data = perif.DataLogicManager;
            data.ChangeLabelStatusIData += new ChangeLabelStatusIDataEventArgs(ChangeLabelStatus);

            FormPeripherique fPerif = new FormPeripherique(perif, data, new Uri(urlApplication, ""), titreApplication, 300);
            fPerif.ChangeLabelStatusFormPeripherique += new ChangeLabelStatusFormPeripheriqueEventArgs(ChangeLabelStatus);

            // Mode recherche            
            fPerif.StartPosition = FormStartPosition.CenterScreen;
            this.Enabled = false;
            fPerif.Show();
            perif.Search(webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("tracking").GetAttribute("value").ToUpper());
            this.Enabled = true;

            if (lmanager.NumeroBadgeString != null)
            {
                labelStatusApplication.Text = "Status : Badge...[" + lmanager.NumeroBadgeString + "]";
                //webBrowser.Document.GetElementById("tracking").SetAttribute("value", lmanager.NumeroBadgeString);
                //webBrowser.Document.GetElementsByTagName("form")[0].InvokeMember("submit"); //submit du formulaire
            }
            else
            {
                labelStatusApplication.Text = "Status : Badge...[-]";
                //webBrowser.Document.GetElementById("lecteurPhysique").SetAttribute("value", "");
            }
            fPerif.Dispose();
            perif.Annuler();
            perif = null;
            labelStatusApplication.Text = "Status : None";
        }

        private void MAGNETA_EASYRONDE(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement MAGNETA_EASYRONDE");
            labelStatusApplication.Text = "Status : [MAGNETA_EASYRONDE Processing]";

            bool trouve = false;
            int cpt = 0;
            string port = "COM1";
            string url_webservice = "";
            string ws_username = "";
            string ws_password = "";

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("MAGNETA_EASYRONDE"))
                {
                    trouve = true;
                    port = perifInfo.Port;
                    url_webservice = perifInfo.Webservice;
                    ws_username = perifInfo.Username;
                    ws_password = Chiffrement.dechiffre(perifInfo.Password, Chiffrement.password);
                }
                else
                    cpt++;
            }

            Process proc = new Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = ".\\Resources\\EasyRonde\\EasyRonde.exe";
            //proc.StartInfo.WorkingDirectory = "Resources\\EasyRonde";
            proc.StartInfo.Arguments = "\"OMNIGARDE\" \"" + port + "\" \"" + url_webservice + "\" \"" + ws_username + "\" \"" + ws_password + "\"";

            Program.LogFich.Info("EXE = " + proc.StartInfo.FileName);
            Program.LogFich.Info("Arguments = " + proc.StartInfo.Arguments);

            //LogFich.Debug("Arguments : \"OMNIGARDE\" \"" + port + "\" \"" + url_webservice + "\" \"" + ws_username + "\" \"" + ws_password + "\"");

            proc.Start();
            proc.WaitForExit();

            labelStatusApplication.Text = "Status : None";
        }

        private void SURVACOM_TRACING(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement SURVACOM_TRACING");
            labelStatusApplication.Text = "Status : [SURVACOM_TRACING Processing]";

            bool trouve = false;
            int cpt = 0;
            string port = "1";
            string url_webservice = "";
            string ws_username = "";
            string ws_password = "";

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("SURVACOM_TRACING"))
                {
                    trouve = true;
                    port = perifInfo.Port.Replace("COM", "");
                    url_webservice = perifInfo.Webservice;
                    ws_username = perifInfo.Username;
                    ws_password = Chiffrement.dechiffre(perifInfo.Password, Chiffrement.password);
                }
                else
                    cpt++;
            }

            Process proc = new Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = ".\\Resources\\Tracing\\SurvacomTracing.exe";
            //proc.StartInfo.Arguments = "\"OMNIGARDE\" \"" + port + "\" \"" + url_webservice + "\" \"" + Directory.GetCurrentDirectory() + "\"";            
            proc.StartInfo.Arguments = "\"OMNIGARDE\" \"" + port + "\" \"" + url_webservice + "\" \"" + ws_username + "\" \"" + ws_password + "\"";

            proc.Start();
            proc.WaitForExit();

            labelStatusApplication.Text = "Status : None";
        }

        private void ZALIX_VEINSECURE_READ(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement ZALIX_VEINSECURE_READ");
            labelStatusApplication.Text = "Status : [ZALIX_VEINSECURE_READ Processing]";

            bool trouve = false;
            int cpt = 0;
            //string port = "1";
            string url_webservice = "";
            string ws_username = "";
            string ws_password = "";

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("ZALIX_VEINSECURE"))
                {
                    trouve = true;
                    //port = perifInfo.Port.Replace("COM", "");
                    url_webservice = perifInfo.Webservice;
                    ws_username = perifInfo.Username;
                    ws_password = Chiffrement.dechiffre(perifInfo.Password, Chiffrement.password);
                }
                else
                    cpt++;
            }

            Process proc = new Process();
            proc.EnableRaisingEvents = false;

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;

            proc.StartInfo.FileName = ".\\Resources\\ZalixVeinSecure\\veinsecure.exe";

            //proc.StartInfo.Arguments = "\"WHO\"";
            proc.StartInfo.Arguments = "\"WHO\" \"" + url_webservice + "\" \"" + ws_username + "\" \"" + ws_password + "\"";


            try
            {
                proc.Start();
                string login = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                LogFich.Info("Retour=" + login.Split(';')[1].Trim());

                StringBuilder sb = new StringBuilder();
                sb.Append("type_peripherique=").Append(HttpUtility.UrlEncode("zalix_veinsecure", Encoding.ASCII));
                sb.Append("&machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                sb.Append("&login=").Append(HttpUtility.UrlEncode(login.Split(';')[1].Trim(), Encoding.ASCII));

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

                Uri adresse = new Uri(urlApplication, "devices/ZALIX_VEINSECURE.asp");
                webBrowser.Navigate(adresse, "", postBytes, "Content-Type: application/x-www-form-urlencoded");

                labelStatusApplication.Text = "Status : Sending data...[Succeeded]";
                Program.LogFich.Info("[BioveinManager] SendDataToRead() - Envoie termine - Trame envoyee = " + sb.ToString());
            }
            catch (Exception ex)
            {
                labelStatusApplication.Text = "Status : Sending data...[Failed]";
                Program.LogFich.Error("[BioveinManager] SendDataToRead() - Erreur d'envoie = " + ex.ToString());
            }

            labelStatusApplication.Text = "Status : None";
        }

        private void ZALIX_VEINSECURE_ADD(object sender, HtmlElementEventArgs e)
        {
            Program.LogFich.Info("[Main] Bouton => Traitement ZALIX_VEINSECURE_ADD");
            labelStatusApplication.Text = "Status : [ZALIX_VEINSECURE_ADD Processing]";

            bool trouve = false;
            int cpt = 0;
            //string port = "1";
            string url_webservice = "";
            string ws_username = "";
            string ws_password = "";

            while (!trouve && cpt < peripheriqueManager.ListePeripherique.Count)
            {
                PeripheriqueInfos perifInfo = peripheriqueManager.ListePeripherique[cpt];

                if (perifInfo.Traitement.Equals("ZALIX_VEINSECURE"))
                {
                    trouve = true;
                    //port = perifInfo.Port.Replace("COM", "");
                    url_webservice = perifInfo.Webservice;
                    ws_username = perifInfo.Username;
                    ws_password = Chiffrement.dechiffre(perifInfo.Password, Chiffrement.password);
                }
                else
                    cpt++;
            }

            Process proc = new Process();
            proc.EnableRaisingEvents = false;

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;

            proc.StartInfo.FileName = ".\\Resources\\ZalixVeinSecure\\veinsecure.exe";
            string value = "";

            if (webBrowser.Document.GetElementById("ipopup") != null && webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("empreinte_id") != null)
            {
                value = webBrowser.Document.Window.Frames["ipopup"].Document.GetElementById("empreinte_id").GetAttribute("value");
            }
            else
                value = webBrowser.Document.GetElementById("empreinte_id").GetAttribute("value");

            //MessageBox.Show("valeur recupere=" + value);
            //proc.StartInfo.Arguments = "REC;" + value;

            proc.StartInfo.Arguments = "REC;" + value + " \"" + url_webservice + "\" \"" + ws_username + "\" \"" + ws_password + "\"";

            try
            {
                proc.Start();
                string data = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                LogFich.Info("Retour=" + data.Split(';')[1].Trim());

                StringBuilder sb = new StringBuilder();
                sb.Append("type_peripherique=").Append(HttpUtility.UrlEncode("hitachi", Encoding.ASCII));
                sb.Append("&machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                sb.Append("&data=").Append(HttpUtility.UrlEncode(data.Split(';')[1].Trim(), Encoding.ASCII));
                sb.Append("&datalen=").Append(HttpUtility.UrlEncode(data.Split(';')[1].Trim().Length.ToString(), Encoding.ASCII));

                byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());

                StringBuilder adr = new StringBuilder();
                Uri url = new Uri(urlApplication, "devices/ZALIX_VEINSECURE.asp");
                adr.Append(url.ToString());
                adr.Append(this.webBrowser.Url.Query);
                Uri adresse = new Uri(adr.ToString());
                webBrowser.Navigate(adresse, "", postBytes, "Content-Type: application/x-www-form-urlencoded");

                labelStatusApplication.Text = "Status : Sending data...[Succeeded]";
                Program.LogFich.Info("[BioveinManager] SendDataToAdd() - Envoie termine - Trame envoyee = " + sb.ToString());
            }
            catch (Exception ex)
            {
                labelStatusApplication.Text = "Status : Sending data...[Failed]";
                Program.LogFich.Error("[BioveinManager] SendDataToAdd() - Erreur d'envoie = " + ex.ToString());
            }

            labelStatusApplication.Text = "Status : None";
        }

        #endregion

        /*#region Fonction pour empecher de déplacer/Redimensionner la fenetre
        private const int WM_NCHITTEST = 0x84;
        private const int HTCAPTION = 0x2;
        private const int HTBORDER = 0x18;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
            {
                if (m.Result.ToInt32() == HTCAPTION)
                {
                    m.Result = new IntPtr(HTBORDER);
                }
            }
        }
        #endregion*/

        public void changeScreen(Form monform, int _screennumber, int location)
        {

            Screen[] screens = Screen.AllScreens;

            int screennumber = (_screennumber % screens.Length);
            if (screennumber == 0) screennumber = screens.Length;

            Point pt = new Point(1, 0);

            for (int i = 0; i < screennumber - 1; i++)
            {
                pt.X += screens[i].Bounds.Width;
            }

            if (location == 0)
            {
                monform.Location = pt;
                //monform.Width = screens[screennumber - 1].Bounds.Width;
                //monform.Height = screens[screennumber - 1].Bounds.Height;
                monform.WindowState = FormWindowState.Maximized;
            }
            else if (location == 1)
            {

                pt.X += screens[screennumber - 1].Bounds.Width / 2;
                pt.X -= monform.Width / 2;
                pt.Y += screens[screennumber - 1].Bounds.Height / 2;
                pt.Y -= monform.Height / 2;
                monform.Location = pt;
            }
            else
            {
                monform.Location = pt;
            }
            monform.Activate();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                this.Location = Screen.AllScreens[SandBox.Properties.Settings.Default.indexScreen].Bounds.Location;
            }
            catch (Exception ex)
            {
                this.Location = Screen.AllScreens[0].Bounds.Location;
                LogFich.Error("Index screen not found = " + ex.ToString());
            }

            this.WindowState = FormWindowState.Maximized;
            //this.Activate();          

            webBrowser.Navigate(urlApplication);

            if (SandBox.Properties.Settings.Default.cookieHttpOnly)
            {
                ToolStripStatusLabel periphStatus = new System.Windows.Forms.ToolStripStatusLabel();
                periphStatus.Name = "device_no_device";
                periphStatus.Spring = true;
                periphStatus.Text = SandBox.Properties.Settings.Default.labelRedDevice;
                periphStatus.BackColor = Color.Red;
                periphStatus.ForeColor = Color.White;
                periphStatus.Alignment = ToolStripItemAlignment.Left;
                periphStatus.Padding = new Padding(0);
                periphStatus.Click += new EventHandler(statusMachine_Click);
                //statusStrip.Items.RemoveAt(5);                
                statusStrip.Items.Insert(5, periphStatus);
            }
        }

        //Cette fonction prend en paramètre le form en cour puis retourne l'écran ou le form se trouve
        private int GetScreenNumber(Form fen)
        {
            string ScreenName = Screen.FromControl(fen).DeviceName;
            ScreenName = System.Text.RegularExpressions.Regex.Replace(ScreenName, "[^0-9]", "");
            return Convert.ToInt32(ScreenName);
        }

        //fonction pour lecteur de badge HID-5352
        private string hidToWiegand(string codeSite, string badgeLecteur)
        {
            string codeRetour;
            try
            {
                codeRetour = codeSite + (((Convert.ToInt64(badgeLecteur, 16)) >> 1) & (Convert.ToInt64("0000FFFF", 16))).ToString("X4");
                Program.LogFich.Info("WIEGAND conversion OK : " + codeRetour);
            }
            catch
            {
                codeRetour = badgeLecteur;
                Program.LogFich.Error("WIEGAND conversion échouée : " + codeRetour);
            }

            return codeRetour;
        }

        //Désactivation de la croix
        private const int CS_NOCLOSE = 0x0200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }

        //Envoie un réponse au POWERSCAN au bout de 10 secondes s'il y n'en a pas eu entre temps
        private void powerscan_timer_Tick(object sender, EventArgs e)
        {
            if (powerscan_seconde >= 10)
            {
                powerscan_timer.Stop();
                powerscan_timer = null;

                powerscan_perif.OpenConnection();
                powerscan_perif.SendMessage(DataLogicManager.transformeStringEnByte(powerscan_lecteur + "<CR>"));

                powerscan_perif.Annuler();
                powerscan_perif = null;

                powerscan_seconde = 1;
            }
            else
            {
                powerscan_seconde += 1;
            }
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {

            if (!e.Url.ToString().ToLower().Contains("javascript") && !e.Url.ToString().ToLower().Contains("alert.asp") && !e.Url.ToString().ToLower().Contains("alert2.asp") && !e.Url.ToString().ToLower().Contains("export"))
            {
                showProgressBar();
            }
            //if ((sender as WebBrowser).ReadyState == WebBrowserReadyState.Complete)
            //hideProgressBar();
        }

        private void showProgressBar()
        {
            if (pb == null && SandBox.Properties.Settings.Default.showLoading)
            {

                pb = new FormProgressBar();
                pb.StartPosition = FormStartPosition.Manual;
                pb.Location = new Point(this.Location.X + (this.Width - pb.Width) / 2, this.Location.Y + (this.Height - pb.Height) / 3);
                pb.Text = titreApplication;
                pb.Show();

                Application.DoEvents();
                //this.Enabled = false;
                //webBrowser.AllowNavigation = false;                
            }
        }

        private void hideProgressBar()
        {
            if (pb != null)
            {
                pb.Dispose();
                pb = null;
                //this.Enabled = true;
                //this.Focus();
                //webBrowser.AllowNavigation = true;

            }
            else
            {
                //this.Enabled = true;
                //webBrowser.Enabled = true;                
                //webBrowser.AllowNavigation = true;
            }

            //Application.DoEvents();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            SandBox.Properties.Settings.Default.indexScreen = GetScreenNumber(this) - 1;
            SandBox.Properties.Settings.Default.Save();
            SandBox.Properties.Settings.Default.Reload();
        }

    }
}