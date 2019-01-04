using System;
using System.Windows.Forms;

namespace SandBox
{
	public class KeyHookEventArgs : EventArgs
	{
		private Keys keyCode;
		private char chKey;
		private bool bAlt;
		private bool bAltCar;
		private bool bLeftCtrl;
		private bool bRightCtrl;
		private bool bLeftShift;
		private bool bRightShift;
		private bool bLeftWin;
		private bool bRightWin;
        private bool bEscape;
        private bool bApps;
        private bool bTab;

        #region Propriétés
        public Keys KeyCode
        {
            get { return keyCode; }
        }

        public char KeyChar
        {
            get { return chKey; }
        }

        public bool LeftAlt
        {
            get { return bAlt; }
        }

        public bool RightAlt
        {
            get { return bAltCar; }
        }

        public bool LeftCtrl
        {
            get { return bLeftCtrl; }
        }

        public bool RightCtrl
        {
            get { return bRightCtrl; }
        }

        public bool LeftShift
        {
            get { return bLeftShift; }
        }

        public bool RightShift
        {
            get { return bRightShift; }
        }

        public bool LeftWin
        {
            get { return bLeftWin; }
        }

        public bool RightWin
        {
            get { return bRightWin; }
        }

        public bool Escape
        {
            get { return bEscape; }
        }

        public bool Apps
        {
            get { return bApps; }
        }

        public bool Tab
        {
            get { return bTab; }
        }   
        #endregion

        #region Constructeur
        public KeyHookEventArgs(Keys keyCode, char keyChar, bool alt, bool altCar, bool leftCtrl, bool rightCtrl, bool leftShift, bool rightShift, bool leftWin, bool rightWin, bool escape, bool apps, bool tab)
        {
            this.keyCode = keyCode;
            chKey = keyChar;
            bAlt = alt;
            bAltCar = altCar;
            bLeftCtrl = leftCtrl;
            bRightCtrl = rightCtrl;
            bLeftShift = leftShift;
            bRightShift = rightShift;
            bLeftWin = leftWin;
            bRightWin = rightWin;
            bEscape = escape;
            bApps = apps;
            bTab = tab;
        }
        #endregion
	}
}
