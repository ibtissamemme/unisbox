using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox
{
    public abstract class PieceGenerique
    {
        private string type_Piece;
        private string code_Pays;
        private string nationalite;
        private string departement;
        private string date_Validite;
        private string nom;
        private string prenom;
        private string date_Naissance;
        private string zone_Option;
        private string zone_Option2;
        private string iD_Document;
        private string sexe;
        private string clef_1;
        private string clef_2;
        private string clef_3;
        private string clef_4;
        private string clef_5;
        private string inconnu;
        private string photo;
        private string recto;
        private string verso;

        #region Properties

        public string Type_Piece
        {
            get { return type_Piece; }
            set { type_Piece = value; }
        }
        public string Code_Pays
        {
            get { return code_Pays; }
            set { code_Pays = value; }
        }
        public string Nom
        {
            get { return nom; }
            set { nom = value; }
        }
        public string Zone_Option
        {
            get { return zone_Option; }
            set { zone_Option = value; }
        }
        public string Zone_Option2
        {
            get { return zone_Option2; }
            set { zone_Option2 = value; }
        }
        public string ID_Document
        {
            get { return iD_Document; }
            set { iD_Document = value; }
        }
        public string Clef_1
        {
            get { return clef_1; }
            set { clef_1 = value; }
        }
        public string Prenom
        {
            get { return prenom; }
            set { prenom = value; }
        }
        public string Date_Naissance
        {
            get { return date_Naissance; }
            set { date_Naissance = value; }
        }
        public string Clef_2
        {
            get { return clef_2; }
            set { clef_2 = value; }
        }
        public string Sexe
        {
            get { return sexe; }
            set { sexe = value; }
        }
        public string Clef_3
        {
            get { return clef_3; }
            set { clef_3 = value; }
        }
        public string Nationalite
        {
            get { return nationalite; }
            set { nationalite = value; }
        }
        public string Date_Validite
        {
            get { return date_Validite; }
            set { date_Validite = value; }
        }
        public string Clef_4
        {
            get { return clef_4; }
            set { clef_4 = value; }
        }
        public string Clef_5
        {
            get { return clef_5; }
            set { clef_5 = value; }
        }
        public string Departement
        {
            get { return departement; }
            set { departement = value; }
        }
        public string Inconnu
        {
            get { return inconnu; }
            set { inconnu = value; }
        }
        public string Photo
        {
            get { return photo; }
            set { photo = value; }
        }
        public string Recto
        {
            get { return recto; }
            set { recto = value; }
        }
        public string Verso
        {
            get { return verso; }
            set { verso = value; }
        }
        #endregion

        public abstract void AfficherPiece();
        public abstract void Nettoyage();

        public string InverseDate(string tmp)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tmp[4]);
            sb.Append(tmp[5]);
            sb.Append("/");
            sb.Append(tmp[2]);
            sb.Append(tmp[3]);
            sb.Append("/");
            sb.Append(tmp[0]);
            sb.Append(tmp[1]);

            return sb.ToString();
        }

        public void SeparerNomPrenom()
        {
            StringBuilder sbNom = new StringBuilder();
            StringBuilder sbPrenom = new StringBuilder();
            bool termine = false;                   //Variable qui permet d'arreter l'analyse dès qu'il ya plus de 2 chevrons
            string tmp = this.Nom;
            int i = 0;                              //Variable pour parcourir tmp

            while (!termine)
            {
                if (tmp[i].Equals('<') && tmp[i + 1].Equals('<'))              //On rencontre un 1er chevron
                    termine = true;
                else
                    sbNom.Append(tmp[i]);
                i++;
            }
            for (int j = i + 1; j < tmp.Length; j++)
                sbPrenom.Append(tmp[j]);

            this.Nom = sbNom.ToString();
            this.Prenom = sbPrenom.ToString();
        }

        public string CorrectionNomPrenom(string tmp)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;                              //Variable pour parcourir tmp
            bool termine = false;                   //Variable qui permet d'arreter l'analyse dès qu'il ya plus de 2 chevrons

            while (i < tmp.Length && !termine)
            {
                if (tmp[i].Equals('<'))              //On rencontre un 1er chevron
                {
                    if (i < tmp.Length - 1)
                    {
                        i++;
                        if (tmp[i].Equals('<'))       //Si le prochain est aussi un chevron, on a termine
                            termine = true;
                        else                            //On est dans le cas d'un Nom composé
                            sb.Append(" ");
                    }
                    else
                        termine = true;
                }
                else
                {
                    sb.Append(tmp[i]);
                    i++;
                }
            }
            return sb.ToString();
        }
    }
}
