using System;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace SandBox.Periph_IrisCard
{
    public partial class Form_Iriscan : Form
    {        
        private Cardiris.ApplicationClass appScan;
        private Cardiris.ICard Carte;
        private String[] Scanner;
        private Uri urlapp;
        private WebBrowser WebBrowser;
        private IrisCard_Manager IrisCardManager;

        public Form_Iriscan(Uri _urlapp, WebBrowser _WebBrowser, IrisCard_Manager _IrisCardManager)
        {
            WebBrowser = _WebBrowser;
            urlapp = _urlapp;
            IrisCardManager = _IrisCardManager;
            InitializeComponent();
            this.Text = WebBrowser.Document.Title;
        }

        private void Form_Iriscan_Load(object sender, EventArgs e)
        {
            label1.Text = "Déposez la carte (face vers le bas). Cliquez sur le bouton Scanner.";
            appScan = new Cardiris.ApplicationClass();
            listBox1.Visible = false;
            pictureBox1.Image = SandBox.Properties.Resources.IRIS_IRISCARD_A6;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Scan();
            envoyer();
            IrisCardManager.Close();
            this.Close();
        }

        private void Scan()
        {
            label1.Text = "Patientez svp...";

            String cheminFicTmp = "./img.jpg";
            Cardiris.CiReturnValue res;
            res = Cardiris.CiReturnValue.ciRetFailure;



            res = appScan.Scan(cheminFicTmp, 100, Cardiris.CiScannerModel.ciScannerModelA6noUI, Cardiris.CiColorMode.ciColorModeGrayLevel, Cardiris.CiResolution.ciresolution600);

            while (res == Cardiris.CiReturnValue.ciRetSuccess)
            {
                appScan.PreProcess(cheminFicTmp, true, true, Cardiris.CiRotation.ciRotationNone);
                Carte = appScan.Recognize(cheminFicTmp, Cardiris.CiCountry.ciCtryFrance, true);

                res = Cardiris.CiReturnValue.ciRetEndOfProcess;
            }
            //test = appScan.ExtractGetCard(cheminFicTmp, Cardiris.CiImageFormat.ciImgFmtJpg);
            //MessageBox.Show("test : " + res.ToString());	
            //pictureBox1.Image = Image.FromFile(cheminFicTmp);

            if (res == Cardiris.CiReturnValue.ciRetEndOfProcess)
            {
                String[] tableau = new String[13];

                tableau[0] = Carte.LastName;
                tableau[1] = Carte.FirstName;
                tableau[2] = Carte.Company;
                tableau[3] = Carte.Title;

                tableau[4] = Carte.Address;
                tableau[5] = Carte.City;
                tableau[6] = Carte.ZipCode;
                tableau[7] = Carte.State;

                tableau[8] = Carte.Phone;
                tableau[9] = Carte.CellPhone;
                tableau[10] = Carte.EMail;
                tableau[11] = Carte.Fax;

                tableau[12] = Carte.Extra;

                Scanner = tableau;

                for (int i = 0; i < 13; i++)
                {
                    listBox1.Items.Add(Scanner[i]);
                }
            }

            if (res == Cardiris.CiReturnValue.ciRetFailure)
            {
                label1.Text = "Déposez la carte (face vers le bas). Cliquez sur le bouton Scanner.";
            }
            else
            {
                label1.Text = "La carte a été scannée avec succès.";
            }
        }

        public bool envoyer()
        {
            //ChangeLabelStatusIData("Status: Sending data...");
            Program.LogFich.Info("[Form_Iriscan]: Envoie des données");
            bool b = true;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("machine=").Append(HttpUtility.UrlEncode(Functions.getHost(), Encoding.ASCII));
                sb.Append("&LastName=").Append(HttpUtility.UrlEncode(Scanner[0], Encoding.ASCII));
                sb.Append("&FirstName=").Append(HttpUtility.UrlEncode(Scanner[1], Encoding.ASCII));
                sb.Append("&Company=").Append(HttpUtility.UrlEncode(Scanner[2], Encoding.ASCII));
                sb.Append("&Title=").Append(HttpUtility.UrlEncode(Scanner[3], Encoding.ASCII));
                sb.Append("&Adresse=").Append(HttpUtility.UrlEncode(Scanner[4], Encoding.ASCII));
                sb.Append("&City=").Append(HttpUtility.UrlEncode(Scanner[5], Encoding.ASCII));
                sb.Append("&ZipCode=").Append(HttpUtility.UrlEncode(Scanner[6], Encoding.ASCII));
                sb.Append("&State=").Append(HttpUtility.UrlEncode(Scanner[7], Encoding.ASCII));
                sb.Append("&Phone=").Append(HttpUtility.UrlEncode(Scanner[8], Encoding.ASCII));
                sb.Append("&CellPhone=").Append(HttpUtility.UrlEncode(Scanner[9], Encoding.ASCII));
                sb.Append("&EMail=").Append(HttpUtility.UrlEncode(Scanner[10], Encoding.ASCII));
                sb.Append("&Fax=").Append(HttpUtility.UrlEncode(Scanner[11], Encoding.ASCII));
                sb.Append("&Extra=").Append(HttpUtility.UrlEncode(Scanner[12], Encoding.ASCII));

                byte[] postBytes = Encoding.ASCII.GetBytes(sb.ToString());
                Uri url = new Uri(urlapp.ToString() + "devices/IRIS_IRISCARD_A6.asp");
                WebBrowser.Navigate(url, "", postBytes, "Content-Type: application/x-www-form-urlencoded");

                //ChangeLabelStatusIData("Status: Sending data...[Succeeded]");
                Program.LogFich.Info("[Form_Iriscan]: Envoie termine - Trame envoyee = " + sb.ToString());
                //Program.LogFich.InfoUtilisateur("Envoi OK - Type:" + this.Piece.Type_Piece + " ID:" + this.Piece.ID_Document + " Pays:" + this.Piece.Code_Pays + " Nom:" + this.Piece.Nom + " Prénom:" + this.Piece.Prenom + " DateNaissance:" + this.Piece.Date_Naissance + " Sexe" + this.Piece.Sexe + " Validité:" + this.Piece.Date_Validite + " Nationalité:" + this.Piece.Nationalite);
            }
            catch (Exception e)
            {
                b = false;
                //ChangeLabelStatusIData("Status: Sending data...[Failed]");
                Program.LogFich.Error("[Form_Iriscan] Erreur d'envoie = " + e.ToString());
                //Program.LogFich.InfoUtilisateur("[Form_Iriscan] Envoi ECHEC - " + e.ToString());
            }
            return b;
        }

        private void Form_Iriscan_FormClosing(object sender, FormClosingEventArgs e)
        {
            IrisCardManager.Close();
        }

    }
}
