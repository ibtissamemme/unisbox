using System;
using System.Windows.Forms;
using System.Threading;

namespace SandBox
{
    public partial class FormPeripherique : Form
    {
        public event ChangeLabelStatusFormPeripheriqueEventArgs ChangeLabelStatusFormPeripherique;
        private Peripherique perif;
        private IData data;
        private Uri url;
        private int cpt = 30;
        private bool receiving = false;

        private Thread demoThread = null;
        delegate void SetTextCallback(string text);

        #region Propriétés
        public IData Data
        {
            get { return this.data; }
            set { data = value; }
        }
        public Peripherique Perif
        {
            get { return perif; }
            set { perif = value; }
        }
        #endregion

        #region Constructeur
        public FormPeripherique(Peripherique perif, IData data, Uri url, string nomAppli, int timeOut)
        {
            InitializeComponent();
            this.data = data;
            this.perif = perif;
            this.url = url;
            this.perif.FenetreMere = this;
            this.Text = nomAppli;
            this.cpt = timeOut;

            perif.DataReceived += new Peripherique.PeripheriqueDataReceivedEventHandler(perif_DataReceived);
            perif.DataReceiving += new Peripherique.PeripheriqueDataReceivingEventHandler(perif_DataReceiving);
        }
        #endregion

        private void FormLecture_Load(object sender, EventArgs e)
        {
            bool test = perif.OpenConnection();
            if (test)
            {
                //cpt = 30;
                if (perif is SandBox.EcranClavierPeriph)
                {
                    label.Text = SandBox.Properties.Settings.Default.LibelleMessageLecteurPortSerie;
                }
                else if (perif is SandBox.EcranClavier)
                {
                    label.Text = SandBox.Properties.Settings.Default.LibelleMessagePieceID;
                }
                else if (perif is SandBox.ScannerPiece)
                {
                    //label.Text = SandBox.Properties.Settings.Default.MessagePieceID;
                    label.Text = "Patientez svp...";
                }
                else if (perif is SandBox.LecteurPortSerie)
                {
                    label.Text = SandBox.Properties.Settings.Default.LibelleMessageLecteurPortSerie;
                }
                else if (perif is SandBox.DeisterPortSerie)
                {
                    label.Text = SandBox.Properties.Settings.Default.LibelleMessageDeister;
                }
                else if (perif is SandBox.MagnetaPortSerie)
                {
                    label.Text = SandBox.Properties.Settings.Default.LibelleMessageMagneta;
                }
                else if (perif is SandBox.DataLogicPortSerie)
                {
                    //cpt = 300;
                    label.Text = SandBox.Properties.Settings.Default.LibelleMessageDataLogicPortSerie;
                }
                timer.Start();               
            }
            else
            {
                Close();
            }
        }

        private void FormLecture_FormClosed(object sender, FormClosedEventArgs e)
        {
            receiving = false;
            perif.Annuler();
            Program.LogFich.Info("[FormPeripherique] Fermeture Fenetre");
            ChangeLabelStatusFormPeripherique("Status : None");
        }

        private void boutonAnnuler_Click(object sender, EventArgs e)
        {
            receiving = false;
            perif.Annuler();
            //this.Close();
            ChangeLabelStatusFormPeripherique("Status : Transfer canceled...");
            Program.LogFich.Info("[FormPeripherique] Bouton cancel");
        }

        private void perif_DataReceiving(object sender, PeripheriqueDataReceivingEventArgs e)
        {
            receiving = true;
            perif.DataReceiving -= new Peripherique.PeripheriqueDataReceivingEventHandler(perif_DataReceiving);
            ChangeLabelStatusFormPeripherique("Status : Receiving data...");
            Program.LogFich.Info("[FormPeripherique] En cours de réception");

            this.demoThread = new Thread(new ThreadStart(this.ThreadProcSafe));

            this.demoThread.Start();
        }

        private void perif_DataReceived(object sender, PeripheriqueDataReceivedEventArgs e)
        {
            receiving = false;
            data.traiter(e.Data);
            perif.SendMessageInLocal();
            data.envoyer(url, Functions.getHost());
            perif.Nettoyer();
            perif.DataReceived -= new Peripherique.PeripheriqueDataReceivedEventHandler(perif_DataReceived);
            ChangeLabelStatusFormPeripherique("Status : Data received...");
            Program.LogFich.Info("[FormPeripherique] Réception terminée");
            cpt = 0;
        }

        private void timer_Tick(object sender, EventArgs e)
        {

            if ((cpt < 1) && (!receiving))
            {
                timer.Stop();
                //perif.Annuler();
                Close();
            }
            cpt--;
        }

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.label.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label.Text = text;
                this.progressBar1.Visible = true;
                this.progressBar1.Style = ProgressBarStyle.Marquee;
                this.progressBar1.MarqueeAnimationSpeed = 10;
            }
        }

        private void ThreadProcSafe()
        {
            this.SetText("En cours de réception ...");
        }

    }
}
