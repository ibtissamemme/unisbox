using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace SandBox
{
    //Classe qui permet de hook les touches du clavier en mode verrouillage
    public class KBHookProvider
    {
        #region Imports de fonctions DLL
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(HookType type, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern int ToAscii(uint uVirtKey, uint uScanCode, byte[] lpKeyState,
           [Out] StringBuilder lpChar, uint uFlags);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        #endregion // Imports de fonctions DLL.

        #region Constantes
        /// <summary>
        /// Voir la doc Win32 pour plus d'infos.
        /// </summary>
        private const int HC_ACTION = 0;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;
        private const int LLKHF_ALTDOWN = KF_ALTDOWN >> 8;
        private const int KF_ALTDOWN = 0x2000;
        #endregion

        #region Structures
        /// <summary>
        /// Structure de données provenant du hook clavier.
        /// Voir la doc Win32 pour plus d'infos.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        #endregion

        #region Enumérations
        /// <summary>
        /// Types de hook. Voir la doc Win32 pour plus d'infos.
        /// </summary>
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }
        #endregion

        #region Évènements
        /// <summary>
        /// Évènement déclenché par le hook clavier.
        /// </summary>
        public event EventHandler<KeyHookEventArgs> KeyDown;
        public event EventHandler<KeyHookEventArgs> KeyUp;
        #endregion

        #region Délégués
        /// <summary>
        /// Délégué de hook clavier. Voir la doc Win32 pour plus d'infos.
        /// </summary>
        /// <param name="nCode">Voir la doc Win32 pour plus d'infos.</param>
        /// <param name="wParam">Voir la doc Win32 pour plus d'infos.</param>
        /// <param name="lParam">Voir la doc Win32 pour plus d'infos.</param>
        /// <returns>Voir la doc Win32 pour plus d'infos.</returns>
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        #endregion

        #region Propriétés
        public bool Enabled
        {
            get
            { return enabled; }
            set
            {
                if (value)
                { Enable(); }
                else
                { Disable(); }
            }
        }
        /// <summary>
        /// Si Block==true l'evenement ne sera pas envoyé à la file d'attente des messages.
        /// </summary>
        public bool Block
        {
            get { return (block == 1); }
            set
            {
                if (value) block = 1;
                else block = 0;
            }
        }

        #endregion

        #region Variables
        private bool enabled;
        private int hHook;
        private HookProc KBHookProc;
        private bool leftCtrl;
        private bool rightCtrl;
        private bool leftShift;
        private bool rightShift;
        private bool leftWin;
        private bool rightWin;
        private bool alt;
        private bool altCar;
        private bool escape;
        private bool apps;
        private bool tab;
        private int block;

        private WebBrowser webBrowser;
        #endregion

        #region Méthodes privées

        private int MainHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= HC_ACTION) && ((wParam == WM_KEYDOWN) || (wParam == WM_SYSKEYDOWN)))
            {
                // Cast : structure pour obtenir les données du hook clavier.
                KBDLLHOOKSTRUCT hookStruct;
                hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                // On récupère le code de touche.
                Keys keyCode = (Keys)hookStruct.vkCode;

                // On récupère l'état du clavier. Nécessaire pour ensuite obtenir le caractère ASCII.
                byte[] keyStates = new byte[255];
                GetKeyboardState(keyStates);
                StringBuilder sbChar = new StringBuilder();
                ToAscii((uint)hookStruct.vkCode, (uint)hookStruct.scanCode, keyStates, sbChar, (uint)hookStruct.flags);
                char keyChar;
                if (sbChar.Length > 0)
                {
                    keyChar = sbChar[0];
                }
                else
                {
                    keyChar = ' ';
                }

                // On récupère l'état de chaque touche spéciale (Control, Alt, Shift et Windows)
                if (hookStruct.vkCode == 0x5B) leftWin = true;
                if (hookStruct.vkCode == 0x5C) rightWin = true;
                if (hookStruct.vkCode == 0xA0) leftShift = true;
                if (hookStruct.vkCode == 0xA1) rightShift = true;
                if (hookStruct.vkCode == 0xA2) leftCtrl = true;
                if (hookStruct.vkCode == 0xA3) rightCtrl = true;
                if (hookStruct.vkCode == 0xA4) alt = true;
                if (hookStruct.vkCode == 0xA5) altCar = true;
                if (hookStruct.vkCode == 0x1B) escape = true;
                if (hookStruct.vkCode == 0x5D) apps = true;
                if (hookStruct.vkCode == 0x09) tab = true;

                // On crée l'évènement.
                KeyHookEventArgs args = new KeyHookEventArgs(keyCode, keyChar, alt, altCar, leftCtrl, rightCtrl, leftShift, rightShift, leftWin, rightWin, escape, apps, tab);
                EventHandler<KeyHookEventArgs> keyDown = KeyDown;

                // Vérifier si il y a au moins un délégué qui écoute l'évènement.
                // Sinon, ça déclenche une erreur si on lance l'évènement dans le vide.
                if (keyDown != null)
                {
                    keyDown(this, args);    // Lancer l'évènement.
                }

                bool a = alt || altCar;
                bool c = rightCtrl || leftCtrl;
                if ((a) && (c) && (hookStruct.vkCode == 0x2E))              //Cas spécial ALT+CTRL+SUPP
                {
                    Program.LogFich.Error("[KBHookProvider] MainHookProc() - SAISIE CTRL + ALT + SUPPR");
                    Uri url = new Uri(SandBox.Properties.Settings.Default.urlApplication.ToString() + "/admin/ctrl_alt_suppr.asp");

                    StringBuilder sb = new StringBuilder();
                    sb.Append("machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                    sb.Append("&typeError=").Append(HttpUtility.UrlEncode("-15", Encoding.ASCII));
                    byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());
                    webBrowser.Navigate(url, "", postBytes, "Content-Type: application/x-www-form-urlencoded");
                    MessageBox.Show("Saisie du ctrl+alt+suppr détectée");
                    webBrowser.GoBack();
                    return block;
                }
                else if (alt && tab)                                             //ALT + TAB
                {
                    return CallNextHookEx(hHook, nCode, wParam, lParam);
                }
                else if ((leftShift && (hookStruct.vkCode == 0x79)) || apps)     //Shift + F10 ou Apps
                {
                    return block;
                }
                else if (leftWin || rightWin || escape || alt)
                {
                    return block;
                }
                else
                {
                    return CallNextHookEx(hHook, nCode, wParam, lParam);
                }
            }
            if ((nCode >= HC_ACTION) && ((wParam == WM_KEYUP) || (wParam == WM_SYSKEYUP)))
            {
                // Cast : structure pour obtenir les données du hook clavier.
                KBDLLHOOKSTRUCT hookStruct;
                hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                // On récupère le code de touche.
                byte keyCode = (byte)hookStruct.vkCode;

                // obtenir le caractère ASCII.
                byte[] keyStates = new byte[255];
                GetKeyboardState(keyStates);
                StringBuilder sbChar = new StringBuilder();
                ToAscii((uint)hookStruct.vkCode, (uint)hookStruct.scanCode, keyStates, sbChar, (uint)hookStruct.flags);
                char keyChar;
                if (sbChar.Length > 0)
                {
                    keyChar = sbChar[0];
                }
                else
                {
                    keyChar = ' ';
                }

                if (keyCode == 0x5B) leftWin = false;
                if (keyCode == 0x5C) rightWin = false;
                if (keyCode == 0xA0) leftShift = false;
                if (keyCode == 0xA1) rightShift = false;
                if (keyCode == 0xA2) leftCtrl = false;
                if (keyCode == 0xA3) rightCtrl = false;
                if (keyCode == 0xA4) alt = false;
                if (keyCode == 0xA5) altCar = false;
                if (keyCode == 0x1B) escape = false;
                if (keyCode == 0x5D) apps = false;
                if (keyCode == 0x09) tab = false;

                KeyHookEventArgs args = new KeyHookEventArgs((Keys)keyCode, keyChar, alt, altCar, leftCtrl, rightCtrl, leftShift, rightShift, leftWin, rightWin, escape, apps, tab);
                EventHandler<KeyHookEventArgs> keyup = KeyUp;
                if (keyup != null)
                {
                    keyup(this, args);
                }

                bool a = alt || altCar;
                bool c = rightCtrl || leftCtrl;
                if ((a) && (c) && (hookStruct.vkCode == 0x2E))              //Cas spécial ALT+CTRL+SUPP
                {
                    return block;
                }
                if (alt && tab)                                             //ALT + TAB
                {
                    return CallNextHookEx(hHook, nCode, wParam, lParam);
                }
                if ((leftShift && (hookStruct.vkCode == 0x79)) || apps)     //Shift + F10 ou Apps
                {
                    return block;
                }
                if (leftWin || rightWin || escape || alt)
                {
                    return block;
                }
                else
                {
                    return CallNextHookEx(hHook, nCode, wParam, lParam);
                }
            }
            else
            {
                return CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }


        [DllImport("coredll.dll", SetLastError = true)]
        static extern Int32 GetLastError();

        /// Enregistre le hook clavier.
        private void Enable()
        {
            KBHookProc = new HookProc(MainHookProc);

            // Obtient le handle de l'application.
            IntPtr hInstance = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]);

            // Installe le hook clavier.
            hHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, KBHookProc, (IntPtr)hInstance, 0);

            // Si le handle du hook retourné vaut 0, cela veut dire que le hook n'a pas marché.
            if (hHook != 0)
            {
                enabled = true;     // Hook réussi.
                Program.LogFich.Info("[KBHookProvider] Enable() - Succeed");
            }
            else
            {
                enabled = false;    // Erreur, ça n'a pas marché.
                Program.LogFich.Error("[KBHookProvider] Enable() - Failed");
            }
        }

        // "Désenregistre" le hook clavier.
        private void Disable()
        {
            if (enabled)
            {
                enabled = !UnhookWindowsHookEx(hHook);  // Désactive le hook. (Ou plutôt "désenregistre").
                hHook = 0;
            }
        }
        #endregion

        #region Méthodes publiques
        /// <summary>
        /// Constructeur.
        /// </summary>
        public KBHookProvider(WebBrowser web)
        {
            this.webBrowser = web;
        }

        /// <summary>
        /// Destructeur. Libère le hook si ce n'est pas fait.
        /// </summary>
        ~KBHookProvider()
        {
            // Si l'utilisateur n'enlève pas le hook, faut le faire à sa place.
            this.Disable();
        }
        #endregion
    }
}
