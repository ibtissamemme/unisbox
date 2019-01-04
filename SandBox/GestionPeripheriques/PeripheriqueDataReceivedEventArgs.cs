using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox
{
	public class PeripheriqueDataReceivedEventArgs : EventArgs
	{
		private string data = null;
        private Trame trame = null;

        #region Constructeur 
        public PeripheriqueDataReceivedEventArgs(string data)
        {
            if (data == null) throw new NullReferenceException();
            this.data = data;
        }

        public PeripheriqueDataReceivedEventArgs(Trame trame)
        {
            try
            {
                if (trame == null) throw new NullReferenceException();
                this.trame = trame;
            }
            catch { }
        }
        #endregion
        
        #region Properties
        public string Data
        {
            get { return this.data; }
        }
        public Trame Trame
        {
            get { return this.trame; }
        }
	    #endregion
	}
}
