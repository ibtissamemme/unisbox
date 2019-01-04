using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.Net;

namespace SandBox
{
    public class PieceManager : IData
    {
        public event ChangeLabelStatusIDataEventArgs ChangeLabelStatusIData;
        private WebBrowser webBrowser;
        private XmlTextReader reader;                       //Lecture fichier XML
        private List<Champ> listChamp = new List<Champ>();  //Tableau des champs
        private PieceGenerique piece;                       //Classe PieceGenerique
        private string trameBrut;                           //Trame sans traitement
        private string trameNet;                            //Trame avec traitement
        private string id_piece;                            //Type de pièce
        private string id_visiteur;                            //Type de pièce

        #region Constructeur
        public PieceManager(WebBrowser web)
        {
            this.webBrowser = web;
            id_visiteur = "";
        }
        #endregion

        #region Properties
        WebBrowser IData.webBrowser
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public List<Champ> ListChamp
        {
            get { return listChamp; }
            set { listChamp = value; }
        }
        public XmlTextReader Reader
        {
            get { return reader; }
            set { reader = value; }
        }
        public string TrameBrut
        {
            get { return trameBrut; }
            set { trameBrut = value; }
        }
        public string TrameNet
        {
            get { return trameNet; }
            set { trameNet = value; }
        }
        public string Id_piece
        {
            get { return id_piece; }
            set { id_piece = value; }
        }
        public string Id_visiteur
        {
            get { return id_visiteur; }
            set { id_visiteur = value; }
        }
        public PieceGenerique Piece
        {
            get { return piece; }
            set { piece = value; }
        }
        public WebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { webBrowser = value; }
        }
        #endregion

        public void traiter(string s)
        {
            ChangeLabelStatusIData("Status: Data processing...");
            Program.LogFich.Info("[PieceManager] TrameBrut recu = " + TrameBrut);

            try
            {
                if (this.id_visiteur == "")
                {
                    FileStream fs = new FileStream(Path.GetDirectoryName((System.Reflection.Assembly.GetEntryAssembly().Location)) + "\\Resources\\ModelPiece.xml", FileMode.Open, FileAccess.Read);
                    this.reader = new XmlTextReader(fs);
                    Program.LogFich.Info("[PieceManager] Lecture du fichier ModelPiece.xml termine");

                    //Nettoyage de la trame et reconnaissance du modele
                    this.trameBrut = s;
                    FormatageNet();
                    CheckPiece();
                    if (!this.id_piece.Equals("0"))
                    {
                        LectureModel();
                        AnalyseModelXml();
                        CreatePiece();
                        piece.AfficherPiece();

                        ChangeLabelStatusIData("Status: Data processing...[Succeeded]");
                        Program.LogFich.Info("[PieceManager] Traitement termine");
                    }
                    else
                    {
                        MessageBox.Show("Type de pièce inconnue, Veuillez recommencer");

                        ChangeLabelStatusIData("Error: Data processing...[Failed]");
                        Program.LogFich.Info("[PieceManager]: Erreur de traitement / Piece non reconnu");
                        //throw new Exception("Type de pièce inconnue, Veuillez recommencer");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Erreur de lecture du fichier ModelPiece.xml = " + e.ToString());
                ChangeLabelStatusIData("Error: Data processing(XML)...[Failed]");
                Program.LogFich.Error("[PieceManager] Erreur de lecture du fichier ModelPiece.xml");
            }
        }

        public bool envoyer(Uri url, string nomMachine)
        {
            ChangeLabelStatusIData("Status: Sending data...");
            Program.LogFich.Info("[PieceManager]: Envoie des données");
            bool b = true;

            try
            {
                StringBuilder sb = new StringBuilder();

                string boundary = "-----------------------------" + DateTime.Now.Ticks.ToString("x");
                sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("type_peripherique\"" + Environment.NewLine + Environment.NewLine).Append("piece").Append(Environment.NewLine);
                sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("machine\"" + Environment.NewLine + Environment.NewLine).Append(nomMachine).Append(Environment.NewLine);

                // Avec piste MRZ
                if (this.Piece != null)
                {
                    if (this.Piece.Type_Piece.Contains("P"))
                    {
                        this.Piece.Type_Piece = "P";
                    }
                    if (this.Piece.Type_Piece.Equals("IR") && this.Piece.Code_Pays.Equals("FRA"))
                    {
                        this.Piece.Type_Piece = "TS";
                    }
                    if (this.Piece.Type_Piece.Equals("ID") && !this.Piece.Code_Pays.Equals("FRA"))
                    {
                        this.Piece.Type_Piece = "IR";
                    }

                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("type_piece\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Type_Piece).Append(Environment.NewLine);

                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("id_document\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.ID_Document).Append(Environment.NewLine);
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("code_pays\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Code_Pays).Append(Environment.NewLine);
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("nom\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Nom).Append(Environment.NewLine);
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("prenom\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Prenom).Append(Environment.NewLine);
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("date_naissance\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Date_Naissance).Append(Environment.NewLine);
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("sexe\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Sexe).Append(Environment.NewLine);
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("date_validite\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Date_Validite).Append(Environment.NewLine);
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("nationalite\"" + Environment.NewLine + Environment.NewLine).Append(this.Piece.Nationalite).Append(Environment.NewLine);

                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("id_visiteur\"" + Environment.NewLine + Environment.NewLine).Append(this.Id_visiteur).Append(Environment.NewLine);

                    if (this.Piece.Photo != "" && this.Piece.Photo != null)
                    {
                        sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("photo\"; filename=\"photo.jpg\"" + Environment.NewLine + "Content-Type: image/jpeg");

                        sb.Append(Environment.NewLine + Environment.NewLine);
                        sb.Append(Encoding.Default.GetString(Functions.StreamFile(this.Piece.Photo)));
                        sb.Append(Environment.NewLine);

                        File.Delete(this.Piece.Photo);
                    }

                    if (this.Piece.Recto != "" && this.Piece.Recto != null)
                    {
                        sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("recto\"; filename=\"recto.jpg\"" + Environment.NewLine + "Content-Type: image/jpeg");

                        sb.Append(Environment.NewLine + Environment.NewLine);
                        sb.Append(Encoding.Default.GetString(Functions.StreamFile(this.Piece.Recto)));
                        sb.Append(Environment.NewLine);

                        File.Delete(this.Piece.Recto);
                    }

                    if (this.Piece.Verso != "" && this.Piece.Verso != null)
                    {
                        sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("verso\"; filename=\"verso.jpg\"" + Environment.NewLine + "Content-Type: image/jpeg");

                        sb.Append(Environment.NewLine + Environment.NewLine);
                        sb.Append(Encoding.Default.GetString(Functions.StreamFile(this.Piece.Verso)));
                        sb.Append(Environment.NewLine);

                        File.Delete(this.Piece.Verso);
                    }

                    sb.Append("--" + boundary).Append("--");

                    byte[] postBytes = Encoding.Default.GetBytes(sb.ToString());
                    WebBrowser.Navigate(url, "", postBytes, "Content-Type: multipart/form-data; boundary=" + boundary + Environment.NewLine + "Content-Length: " + postBytes.Length + Environment.NewLine + Environment.NewLine);

                    ChangeLabelStatusIData("Status: Sending data...[Succeeded]");
                    //Program.LogFich.Debug("[PieceManager]: Envoie termine - Trame envoyee = " + sb.ToString());
                    Program.LogFich.Info("Envoi OK - Type:" + this.Piece.Type_Piece + " ID:" + this.Piece.ID_Document + " Pays:" + this.Piece.Code_Pays + " Nom:" + this.Piece.Nom + " Prénom:" + this.Piece.Prenom + " DateNaissance:" + this.Piece.Date_Naissance + " Sexe:" + this.Piece.Sexe + " Validité:" + this.Piece.Date_Validite + " Nationalité:" + this.Piece.Nationalite + " Photo:" + this.Piece.Photo + " Recto:" + this.Piece.Recto);
                }
                else if (!this.Id_visiteur.Equals(""))
                {
                    sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("id_visiteur\"" + Environment.NewLine + Environment.NewLine).Append(this.Id_visiteur).Append(Environment.NewLine);


                    if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "recto.jpeg"))
                    {
                        sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("recto\"; filename=\"recto.jpg\"" + Environment.NewLine + "Content-Type: image/jpeg");

                        sb.Append(Environment.NewLine + Environment.NewLine);
                        sb.Append(Encoding.Default.GetString(Functions.StreamFile(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "recto.jpeg")));
                        sb.Append(Environment.NewLine);
                    }

                    /*if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "verso.jpeg"))
                    {

                        sb.Append("--" + boundary).Append(Environment.NewLine + "Content-Disposition: form-data; name=\"").Append("recto\"; filename=\"verso.jpg\"" + Environment.NewLine + "Content-Type: image/jpeg");

                        sb.Append(Environment.NewLine + Environment.NewLine);
                        sb.Append(Encoding.Default.GetString(Functions.StreamFile(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "verso.jpeg")));
                        sb.Append(Environment.NewLine);
                    }*/

                    sb.Append("--" + boundary).Append("--");

                    byte[] postBytes = Encoding.Default.GetBytes(sb.ToString());
                    WebBrowser.Navigate(url, "", postBytes, "Content-Type: multipart/form-data; boundary=" + boundary + Environment.NewLine + "Content-Length: " + postBytes.Length + Environment.NewLine + Environment.NewLine);

                }

            }
            catch (Exception e)
            {
                b = false;
                ChangeLabelStatusIData("Status: Sending data...[Failed]");
                Program.LogFich.Error("[PieceManager] Erreur d'envoie = " + e.ToString());
                //Program.LogFich.Error("Envoi ECHEC - " + e.ToString());
            }
            return b;
        }

        #region Gestion du fichier XML: LectureModel() - TraverseSiblings() - TraverseChildren() - AnalyseModelXml()
        public void LectureModel()
        {
            try
            {
                XPathDocument xdoc = new XPathDocument(reader);
                XPathNavigator nav = xdoc.CreateNavigator();
                XPathNodeIterator nodeItor = nav.Select("Pieces/Piece[@id='" + id_piece + "']/*");
                nodeItor.MoveNext();
                TraverseSiblings(nodeItor);
                Program.LogFich.Info("[PieceManager] LectureModel ID: = " + id_piece);
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[PieceManager] Erreur de LectureModel = " + e.ToString());
            }
        }

        public void TraverseSiblings(XPathNodeIterator nodeItor)
        {
            XPathNodeIterator igor = nodeItor.Clone();
            igor.Current.MoveToNext();
            bool more = false;
            do
            {
                Champ champ = new Champ();
                champ.NomChamp = igor.Current.Name;

                TraverseChildren(igor, champ);
                more = igor.Current.MoveToNext();

                listChamp.Add(champ);
            } while (more);
        }

        public void TraverseChildren(XPathNodeIterator nodeItor, Champ champ)
        {
            XPathNodeIterator igor = nodeItor.Clone();
            igor.Current.MoveToFirstChild();
            bool more = false;
            do
            {
                if (igor.Current.Name.Equals("DEBUT"))
                {
                    champ.Position_deb = int.Parse(igor.Current.Value);
                }
                else
                {
                    champ.Position_fin = int.Parse(igor.Current.Value);
                }
                more = igor.Current.MoveToNext();
            } while (more);
        }

        public void AnalyseModelXml()
        {
            foreach (Champ ch in listChamp)
            {
                int deb = ch.Position_deb;
                int fin = ch.Position_fin;

                StringBuilder sb = new StringBuilder();

                for (int i = deb; i <= fin; i++)
                {
                    sb.Append(trameNet[i]);
                }
                ch.Contenu = sb.ToString();
            }
            Program.LogFich.Info("[PieceManager] Analyse Model Xml ID du type de piece = " + id_piece);
        }
        #endregion

        #region Gestion des trames pour créer une pièce: FormatageNet() - CheckPiece() - CreatePiece()
        public void FormatageNet()
        {
            trameNet = trameBrut.Trim();

            // swissreader
            trameNet = trameNet.Replace("$", "");
            trameNet = trameNet.Replace("#", "");

            // 3M
            //trameNet = trameNet.Replace("START", "");
            trameNet = trameNet.Replace("STARTOCR Line 1: ", "");
            trameNet = trameNet.Replace("STARTOCR Line 2: ", "");
            trameNet = trameNet.Replace("OCR Line 2: ", "");
            trameNet = trameNet.Replace("OCR Line 3: ", "");
            //trameNet = trameNet.Replace("END", "");

            Program.LogFich.Info("[PieceManager] FormatageNet = " + TrameNet);
        }

        public void CheckPiece()
        {
            if (!trameNet.Equals(""))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(this.trameNet[0]);
                sb.Append(this.trameNet[1]);

                StringBuilder sb_pays = new StringBuilder();
                sb_pays.Append(this.trameNet[2]);
                sb_pays.Append(this.trameNet[3]);
                sb_pays.Append(this.trameNet[4]);

                this.id_piece = "0";

                //if (sb.ToString().Equals("IR") || (sb.ToString().Equals("ID") && !sb_pays.ToString().Equals("FRA")))
                //    this.id_piece = "3";
                if (sb.ToString().StartsWith("A") || sb.ToString().StartsWith("C") || sb.ToString().StartsWith("I"))
                    this.id_piece = "3";
                if (sb.ToString().Equals("ID") && sb_pays.ToString().Equals("FRA"))
                    this.id_piece = "1";
                if (sb.ToString().Equals("C1") || sb.ToString().Equals("C2"))
                    this.id_piece = "6";
                if (sb.ToString().Equals("GC"))
                    this.id_piece = "5";
                if (sb.ToString().Equals("TS"))
                    this.id_piece = "4";
                if (sb.ToString().StartsWith("P"))
                    this.id_piece = "2";
                if (sb.ToString().StartsWith("V"))
                {
                    if (TrameNet.Length > 40)
                        this.id_piece = "8";
                    else
                        this.id_piece = "7";
                }

                Program.LogFich.Info("[PieceManager] CheckPiece ID trouvé = " + id_piece);
            }
            else
            {
                this.id_piece = "0";
                Program.LogFich.Warn("[PieceManager] CheckPiece ID Non trouvé = " + id_piece);
            }
        }

        public void CreatePiece()
        {
            PieceGenerique p;

            if (this.id_piece.Equals("1"))
            {
                p = new PieceCarteID(ListChamp);
                this.piece = p;
            }
            else if (this.id_piece.Equals("3"))
            {
                p = new PieceCIE(ListChamp);
                this.piece = p;
            }
            else if (this.id_piece.Equals("6"))
            {
                p = new PieceC(ListChamp);
                this.piece = p;
            }
            else if (this.id_piece.Equals("5"))
            {
                p = new PieceGC(ListChamp);
                this.piece = p;
            }
            else if (this.id_piece.Equals("4"))
            {
                p = new PieceTS(ListChamp);
                this.piece = p;
            }
            else if (this.id_piece.Equals("2"))
            {
                p = new PiecePassport(ListChamp);
                this.piece = p;
            }


            else if (this.id_piece.Equals("7"))
            {
                p = new PieceVisaA(ListChamp);
                this.piece = p;
            }
            else if (this.id_piece.Equals("8"))
            {
                p = new PieceVisaB(ListChamp);
                this.piece = p;
            }

            if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg"))
            {
                this.Piece.Photo = Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg";
            }
            if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "recto.jpeg"))
            {
                this.Piece.Recto = Environment.ExpandEnvironmentVariables("%TEMP%\\") + "recto.jpeg";
            }
            if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "verso.jpeg"))
            {
                this.Piece.Verso = Environment.ExpandEnvironmentVariables("%TEMP%\\") + "verso.jpeg";
            }

            Program.LogFich.Info("[PieceManager] CreatePiece = " + this.Piece.GetType().ToString());
        }
        #endregion
    }
}
