using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox
{
    public class Champ
    {
        private int position_deb;	//Indice de départ du champ
        private int position_fin;	//Indice de fin du champ
        private string nomChamp;	//Name du champ lu
        private string contenu;	    //Contenu du champ qu’on récupèra

        #region Properties
        public int Position_deb
        {
            get { return position_deb; }
            set { position_deb = value; }
        }
        public int Position_fin
        {
            get { return position_fin; }
            set { position_fin = value; }
        }
        public string NomChamp
        {
            get { return nomChamp; }
            set { nomChamp = value; }
        }
        public string Contenu
        {
            get { return contenu; }
            set { contenu = value; }
        }
        #endregion
    }
}
