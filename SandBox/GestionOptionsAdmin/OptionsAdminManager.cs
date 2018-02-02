using System;
using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SandBox
{
    class OptionsAdminManager
    {
        private XmlReader reader;
        private WebBrowser webBrowser;
        private Uri urlApplication;

        private bool enableClose = false;
        private KBHookProvider prov;

        private bool optionAdmin_AutoStart;
        private bool optionAdmin_LockDesktop;
        private bool optionAdmin_ShutDown;
        private int optionAdmin_Beep = 0;

        private static System.Object lockThis = new System.Object();

        #region Propriétés
        public bool EnableClose
        {
            get { return enableClose; }
            set { enableClose = value; }
        }
        public bool OptionAdmin_LockDesktop
        {
            get { return optionAdmin_LockDesktop; }
            set { optionAdmin_LockDesktop = value; }
        }
        public int OptionAdmin_Beep
        {
            get { return optionAdmin_Beep; }
            set { optionAdmin_Beep = value; }
        }
        #endregion

        #region Constructeur
        public OptionsAdminManager(WebBrowser web, Uri urlApplication)
        {
            this.urlApplication = urlApplication;
            this.webBrowser = web;
            optionAdmin_AutoStart = false;
            optionAdmin_LockDesktop = false;
            optionAdmin_ShutDown = false;
        }
        #endregion

        //Fonction: Telecharge flux XML contenant les options administrateur du poste
        public void DownloadXML()
        {
            Program.LogFich.Info("[OptionsAdminManager] DownloadXML() - [DEBUT] Telechargement xml [" + Functions.getHost() + "]");
            string getVars = "?machine=" + Functions.getHost();
            string adresse = urlApplication.ToString() + "admin/optionadmin.asp" + getVars;

            //-----------test xml local-------------------
            string nomfichier;
            string[] tmp;
            tmp = urlApplication.ToString().Split('/');
            nomfichier = tmp[tmp.Length - 2];
            adresse = Environment.ExpandEnvironmentVariables("%TEMP%\\") + nomfichier + "_optionadmin.xml";
            //-----------test xml local-------------------

            try
            {
                //reader = new XmlTextReader(adresse);                
                XmlReaderSettings xmlSettings = new XmlReaderSettings();
                xmlSettings.CloseInput = true;
                reader = XmlReader.Create(adresse, xmlSettings);
                Program.LogFich.Info("[OptionsAdminManager] DownloadXML() - [FIN] Telechargement xml termine");
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[OptionsAdminManager] DownloadXML() - [FIN] Erreur de Telechargement" + e.ToString());
                //DownloadXML();
                MessageBox.Show(e.Message);
            }
        }

        //Fonction: Traitement du flux XML (initialisation des variables options)
        public void TraiterXML()
        {
            Program.LogFich.Info("[OptionsAdminManager] TraiterXML() - [DEBUT] Traitement des options admin");

            string nomElement = "";

            //Protection de la ressource
            lock (lockThis)
            {
                try
                {
                    this.DownloadXML();
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                nomElement = reader.Name;
                                break;

                            case XmlNodeType.Text:
                                if (nomElement.Equals("optionadmin_autostart"))
                                {
                                    if (reader.Value.Equals("1"))
                                        optionAdmin_AutoStart = true;
                                    else
                                        optionAdmin_AutoStart = false;
                                }
                                else if (nomElement.Equals("optionadmin_lockdesktop"))
                                {
                                    if (reader.Value.Equals("1"))
                                        optionAdmin_LockDesktop = true;
                                    else
                                        optionAdmin_LockDesktop = false;
                                }
                                else if (nomElement.Equals("optionadmin_shutdown"))
                                {
                                    if (reader.Value.Equals("1"))
                                        optionAdmin_ShutDown = true;
                                    else
                                        optionAdmin_ShutDown = false;
                                }
                                else if (nomElement.Equals("optionadmin_beep"))
                                {
                                    int.TryParse(reader.Value.ToString(), out optionAdmin_Beep);
                                }
                                break;
                        }
                    }
                    Program.LogFich.Info("[OptionsAdminManager] TraiterXML() - optionAdmin_AutoStart:" + optionAdmin_AutoStart);
                    Program.LogFich.Info("[OptionsAdminManager] TraiterXML() - optionAdmin_LockDesktop:" + optionAdmin_LockDesktop);
                    Program.LogFich.Info("[OptionsAdminManager] TraiterXML() - optionAdmin_ShutDown:" + optionAdmin_ShutDown);
                    Program.LogFich.Info("[OptionsAdminManager] TraiterXML() - optionAdmin_Beep:" + optionAdmin_Beep);
                    Program.LogFich.Info("[OptionsAdminManager] TraiterXML() - [FIN] Traitement des options admin terminé");
                    reader.Close();
                    reader = null;
                }
                catch (Exception e)
                {
                    Program.LogFich.Error("[OptionsAdminManager] TraiterXML() - [FIN] Erreur:" + e.ToString());
                    MessageBox.Show(e.Message);
                }
            }
        }

        //Fonction: Lors du démarrage du poste
        public void GestionDemarrageOptionsAdmin(Form targetForm)
        {
            if (optionAdmin_LockDesktop)
            {
                StartControlKeyboard();
                HideStartMenu();
                Maximize(targetForm);
            }

            if (optionAdmin_AutoStart)
                StartAutoLaunch();
            else
                StopAutoLaunch();
        }

        //Fonction: Lors de l'arret du poste
        public void GestionArretOptionsAdmin()
        {
            if (optionAdmin_LockDesktop)
            {
                StopControlKeyBoard();
                ShowStartMenu();
            }
            if (optionAdmin_ShutDown)
                ShutDownComputer();
        }

        #region StartAutoLaunch() - StopAutoLaunch()
        //Fonction: Met en place le démarrage automatique de la sandbox au démarrage de windows (ajouter raccourci dans "Démarrage")
        private void StartAutoLaunch()
        {
            Assembly code = Assembly.GetExecutingAssembly();
            string company = string.Empty;
            string description = string.Empty;
            if (Attribute.IsDefined(code, typeof(AssemblyCompanyAttribute)))
            {
                AssemblyCompanyAttribute ascompany =
                    (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(code,
                    typeof(AssemblyCompanyAttribute));
                company = ascompany.Company;
            }
            if (Attribute.IsDefined(code, typeof(AssemblyDescriptionAttribute)))
            {
                AssemblyDescriptionAttribute asdescription =
                    (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(code,
                    typeof(AssemblyDescriptionAttribute));
                description = asdescription.Description;
            }
            if (company != string.Empty && description != string.Empty)
            {
                try
                {
                    StringBuilder demarrage = new StringBuilder();
                    demarrage.Append(System.IO.Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Startup)));
                    demarrage.Append("\\");
                    demarrage.Append(SandBox.Properties.Settings.Default.nomApplication);
                    demarrage.Append(" - ");
                    demarrage.Append(description);
                    demarrage.Append(".appref-ms");

                    StringBuilder shortcutName = new StringBuilder();
                    shortcutName.Append(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
                    shortcutName.Append("\\");
                    shortcutName.Append(company);
                    shortcutName.Append("\\");
                    shortcutName.Append(SandBox.Properties.Settings.Default.nomApplication);
                    shortcutName.Append(" - ");
                    shortcutName.Append(description);
                    shortcutName.Append(".appref-ms");

                    System.IO.File.Copy(shortcutName.ToString(), demarrage.ToString(), true);
                }
                catch (Exception e)
                {
                    Program.LogFich.Error("[OptionsAdmin] StartAutoLaunch() - " + e.ToString());
                }
            }
        }

        //Fonction: Met en place l'arret du lancement automatique au démarrage de windows (supprimer raccourci dans "Démarrage")
        private void StopAutoLaunch()
        {
            Assembly code = Assembly.GetExecutingAssembly();
            string company = string.Empty;
            string description = string.Empty;
            if (Attribute.IsDefined(code, typeof(AssemblyCompanyAttribute)))
            {
                AssemblyCompanyAttribute ascompany =
                    (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(code,
                    typeof(AssemblyCompanyAttribute));
                company = ascompany.Company;
            }
            if (Attribute.IsDefined(code, typeof(AssemblyDescriptionAttribute)))
            {
                AssemblyDescriptionAttribute asdescription =
                    (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(code,
                    typeof(AssemblyDescriptionAttribute));
                description = asdescription.Description;
            }
            if (company != string.Empty && description != string.Empty)
            {
                try
                {
                    StringBuilder demarrage = new StringBuilder();
                    demarrage.Append(System.IO.Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Startup)));
                    demarrage.Append("\\");
                    demarrage.Append(SandBox.Properties.Settings.Default.nomApplication);
                    demarrage.Append(" - ");
                    demarrage.Append(description);
                    demarrage.Append(".appref-ms");

                    System.IO.File.Delete(demarrage.ToString());
                }
                catch (Exception e)
                {
                    Program.LogFich.Error("[OptionsAdmin] StopAutoLaunch() - " + e.ToString());
                }
            }
        }
        #endregion

        #region Module pour gérer le FullScreen et mettre la sandbox en PREMIER PLAN
        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int which);
        [DllImport("user32.dll")]
        public static extern void SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int width, int height, uint flags);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private static IntPtr HWND_TOP = IntPtr.Zero;
        private const int SWP_SHOWWINDOW = 64; // 0x0040

        public static int ScreenX
        {
            get { return GetSystemMetrics(SM_CXSCREEN); }
        }

        public static int ScreenY
        {
            get { return GetSystemMetrics(SM_CYSCREEN); }
        }

        public static void SetWinFullScreen(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_TOP, 0, 0, ScreenX, ScreenY, SWP_SHOWWINDOW);
        }

        public void Maximize(Form targetForm)
        {
            targetForm.WindowState = FormWindowState.Maximized;
            targetForm.FormBorderStyle = FormBorderStyle.None;
            //TODO: A DECOMMENTER
            //targetForm.TopMost = true;
            SetWinFullScreen(targetForm.Handle);
        }
        #endregion

        #region Module pour gérer la barre Windows - HideStartMenu() - ShowStartMenu()
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumThreadWindows(int threadId, EnumThreadProc pfnEnum, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern System.IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out int lpdwProcessId);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private const string VistaStartMenuCaption = "Start";
        private static IntPtr vistaStartMenuWnd = IntPtr.Zero;
        private delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);

        //Fonction: Affiche la barre de menu windows
        public static void ShowStartMenu()
        {
            SetVisibility(true);
        }

        //Fonction: Cache la barre de menu windows
        public static void HideStartMenu()
        {
            SetVisibility(false);
        }

        public static bool Visible
        {
            set { SetVisibility(value); }
        }

        private static void SetVisibility(bool show)
        {
            // get taskbar window
            IntPtr taskBarWnd = FindWindow("Shell_TrayWnd", null);

            // try it the WinXP way first...
            IntPtr startWnd = FindWindowEx(taskBarWnd, IntPtr.Zero, "Button", "Start");
            if (startWnd == IntPtr.Zero)
            {
                // ok, let's try the Vista easy way...
                startWnd = FindWindow("Button", null);

                if (startWnd == IntPtr.Zero)
                {
                    // no chance, we need to to it the hard way...
                    startWnd = GetVistaStartMenuWnd(taskBarWnd);
                }
            }

            ShowWindow(taskBarWnd, show ? SW_SHOW : SW_HIDE);
            ShowWindow(startWnd, show ? SW_SHOW : SW_HIDE);
        }

        /// <summary>
        /// Returns the window handle of the Vista start menu orb.
        /// </summary>
        /// <param name="taskBarWnd">windo handle of taskbar</param>
        /// <returns>window handle of start menu</returns>
        private static IntPtr GetVistaStartMenuWnd(IntPtr taskBarWnd)
        {
            // get process that owns the taskbar window
            int procId;
            GetWindowThreadProcessId(taskBarWnd, out procId);

            Process p = Process.GetProcessById(procId);
            if (p != null)
            {
                // enumerate all threads of that process...
                foreach (ProcessThread t in p.Threads)
                {
                    EnumThreadWindows(t.Id, MyEnumThreadWindowsProc, IntPtr.Zero);
                }
            }
            return vistaStartMenuWnd;
        }

        /// <summary>
        /// Callback method that is called from 'EnumThreadWindows' in 'GetVistaStartMenuWnd'.
        /// </summary>
        /// <param name="hWnd">window handle</param>
        /// <param name="lParam">parameter</param>
        /// <returns>true to continue enumeration, false to stop it</returns>
        private static bool MyEnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder buffer = new StringBuilder(256);
            if (GetWindowText(hWnd, buffer, buffer.Capacity) > 0)
            {
                Console.WriteLine(buffer);
                if (buffer.ToString() == VistaStartMenuCaption)
                {
                    vistaStartMenuWnd = hWnd;
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Module pour gérer le HOOK clavier - StartControlKeyboard() - StopControlKeyBoard()
        //Fonction: Lancement du hooker de clavier
        private void StartControlKeyboard()
        {
            prov = new KBHookProvider(webBrowser);
            prov.KeyDown += new EventHandler<KeyHookEventArgs>(prov_KeyDown);
            prov.KeyUp += new EventHandler<KeyHookEventArgs>(prov_KeyUp);
            prov.Enabled = true;
            prov.Block = true;
        }
        //Fonction: Stop le hooker de clavier
        private void StopControlKeyBoard()
        {
            prov.Enabled = false;
        }

        private void prov_KeyDown(object sender, KeyHookEventArgs e)
        {
        }

        private void prov_KeyUp(object sender, KeyHookEventArgs e)
        {
        }
        #endregion

        //Fonction: Extinction du poste
        private void ShutDownComputer()
        {
            ManagementBaseObject outParameters = null;
            ManagementClass sysOS = new ManagementClass("Win32_OperatingSystem");
            sysOS.Get();
            // enables required security privilege.
            sysOS.Scope.Options.EnablePrivileges = true;
            // get our in parameters
            ManagementBaseObject inParameters = sysOS.GetMethodParameters("Win32Shutdown");
            // pass the flag of 0 = System Shutdown
            inParameters["Flags"] = "1";
            inParameters["Reserved"] = "0";
            foreach (ManagementObject manObj in sysOS.GetInstances())
            {
                outParameters = manObj.InvokeMethod("Win32Shutdown", inParameters, null);
            }
        }
    }
}
