using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SandBox;

namespace SandBox
{
    public class PieceCarteID : PieceGenerique
    {
        //Constructeur
        public PieceCarteID(List<Champ> listChamp)
        {
            this.Type_Piece     = listChamp[0].Contenu;
            this.Code_Pays      = listChamp[1].Contenu;
            this.Nom            = listChamp[2].Contenu;
            this.Departement    = listChamp[3].Contenu;
            this.Zone_Option    = listChamp[4].Contenu;
            this.ID_Document    = listChamp[5].Contenu;
            this.Clef_1         = listChamp[6].Contenu;
            this.Prenom         = listChamp[7].Contenu;
            this.Date_Naissance = listChamp[8].Contenu;
            this.Clef_2         = listChamp[9].Contenu;
            this.Sexe           = listChamp[10].Contenu;
            this.Clef_3         = listChamp[11].Contenu;
            this.Nationalite = this.Code_Pays;
            Nettoyage();    
        }

        public override void Nettoyage()
        {
            this.Date_Naissance = InverseDate(this.Date_Naissance);
            this.Nom = CorrectionNomPrenom(this.Nom);
            this.Prenom = CorrectionNomPrenom(this.Prenom);
            this.Code_Pays = CorrectionNomPrenom(this.Code_Pays);
            this.ID_Document = CorrectionNomPrenom(this.ID_Document);
        }

        public override void AfficherPiece()
        {
            Program.LogFich.Info("Contenu Piece Carte ID:");
            Program.LogFich.Info(" Type: " + Type_Piece );
            Program.LogFich.Info(" Pays: " + Code_Pays);
            Program.LogFich.Info(" Nom: " + Nom);
            Program.LogFich.Info(" Dep: " + Departement);
            Program.LogFich.Info(" Zone: " + Zone_Option);
            Program.LogFich.Info(" Id_Doc: " + ID_Document);
            Program.LogFich.Info(" clef1: " + Clef_1);
            Program.LogFich.Info(" prenom: " + Prenom);
            Program.LogFich.Info(" dateNaissance: " + Date_Naissance);
            Program.LogFich.Info(" clef2: " + Clef_2);
            Program.LogFich.Info(" Sexe: " + Sexe);
            Program.LogFich.Info(" clef3: " + Clef_3);
        }
    }
}
