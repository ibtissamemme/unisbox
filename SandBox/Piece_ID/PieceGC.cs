using System;
using System.Collections.Generic;
using System.Text;
using SandBox;

namespace SandBox
{
    public class PieceGC : PieceGenerique
    {
        public PieceGC(List<Champ> listChamp)
        {
            this.Type_Piece         = listChamp[0].Contenu;
            this.Code_Pays          = listChamp[1].Contenu;
            this.Nom                = listChamp[2].Contenu;
            this.ID_Document        = listChamp[3].Contenu;
            this.Clef_1             = listChamp[4].Contenu;
            this.Nationalite        = listChamp[5].Contenu;
            this.Date_Naissance     = listChamp[6].Contenu;
            this.Inconnu            = listChamp[7].Contenu;
            this.Sexe               = listChamp[8].Contenu;
            this.Date_Validite      = listChamp[9].Contenu;
            this.Clef_2             = listChamp[10].Contenu;
            this.Zone_Option        = listChamp[11].Contenu;
            this.Clef_3             = listChamp[12].Contenu;

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
            Program.LogFich.Info("Contenu Piece GC: ");
            Program.LogFich.Info(" Type: " + Type_Piece);
            Program.LogFich.Info(" Pays: " + Code_Pays);
            Program.LogFich.Info(" Nom: " + Nom);
            Program.LogFich.Info(" Prenom: " + Prenom);
            Program.LogFich.Info(" Id_Doc: " + ID_Document);
            Program.LogFich.Info(" clef1: " + Clef_1);
            Program.LogFich.Info(" Nationalite: " + Nationalite);
            Program.LogFich.Info(" dateNaissance: " + Date_Naissance);
            Program.LogFich.Info(" inconnu: " + Inconnu);
            Program.LogFich.Info(" Sexe: " + Sexe);
            Program.LogFich.Info(" dateValidite: " + Date_Validite);
            Program.LogFich.Info(" clef2: " + Clef_2);
            Program.LogFich.Info(" Zone: " + Zone_Option);
            Program.LogFich.Info(" clef3: " + Clef_3);
        }
    }
}
