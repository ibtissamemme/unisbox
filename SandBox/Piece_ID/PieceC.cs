using System;
using System.Collections.Generic;
using System.Text;
using SandBox;

namespace SandBox
{
    public class PieceC : PieceGenerique
    {
        public PieceC(List<Champ> listChamp)
        {
            this.Type_Piece         = listChamp[0].Contenu;
            this.Code_Pays          = listChamp[1].Contenu;
            this.ID_Document        = listChamp[2].Contenu;
            this.Clef_1             = listChamp[3].Contenu;
            this.Zone_Option        = listChamp[4].Contenu;
            this.Date_Naissance     = listChamp[5].Contenu;
            this.Clef_2             = listChamp[6].Contenu;
            this.Sexe               = listChamp[7].Contenu;
            this.Date_Validite      = listChamp[8].Contenu;
            this.Clef_3             = listChamp[9].Contenu;
            this.Nationalite        = listChamp[10].Contenu;
            this.Zone_Option2       = listChamp[11].Contenu;
            this.Clef_4             = listChamp[12].Contenu;
            this.Nom                = listChamp[13].Contenu;

            Nettoyage();
        }

        public override void Nettoyage()
        {
            this.Date_Validite = InverseDate(this.Date_Validite);
            this.Date_Naissance = InverseDate(this.Date_Naissance);
            SeparerNomPrenom();
            this.Nom = CorrectionNomPrenom(this.Nom);
            this.Prenom = CorrectionNomPrenom(this.Prenom);
            this.Code_Pays = CorrectionNomPrenom(this.Code_Pays);
            this.ID_Document = CorrectionNomPrenom(this.ID_Document);
        }

        public override void AfficherPiece()
        {
            Program.LogFich.Info("Contenu Piece C:");
            Program.LogFich.Info(" Type: " + Type_Piece);
            Program.LogFich.Info(" Pays: " + Code_Pays);
            Program.LogFich.Info(" Id_Doc: " + ID_Document);
            Program.LogFich.Info(" clef1: " + Clef_1);
            Program.LogFich.Info(" Zone1: " + Zone_Option);
            Program.LogFich.Info(" dateNaissance: " + Date_Naissance);
            Program.LogFich.Info(" clef2: " + Clef_2);
            Program.LogFich.Info(" Sexe: " + Sexe);
            Program.LogFich.Info(" dateValidite: " + Date_Validite);
            Program.LogFich.Info(" clef3: " + Clef_3);
            Program.LogFich.Info(" Nationalite: " + Nationalite);
            Program.LogFich.Info(" Zone2: " + Zone_Option2);
            Program.LogFich.Info(" clef4: " + Clef_4);
            Program.LogFich.Info(" Nom: " + Nom);
            Program.LogFich.Info(" Prenom: " + Prenom);
        }
    }
}
