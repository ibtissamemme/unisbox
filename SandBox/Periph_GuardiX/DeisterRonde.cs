using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox
{
    public class DeisterRonde
    {
        private string dateHeure;
        private string data;

        #region Constructeur
        public DeisterRonde(string _dateheure, string _data)
        {
            this.dateHeure = _dateheure;
            this.data = _data;
        }
        #endregion

        #region Properties
        public string DateHeure
        {
            get { return dateHeure; }
            set { dateHeure = value; }
        }
        public string Data
        {
            get { return data; }
            set { data = value; }
        }
        #endregion
    }
}
