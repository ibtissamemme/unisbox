using System.Windows.Forms;
using System;

namespace SandBox
{
    public class EcranClavier : Peripherique
    {
        private TextBox textBox;

        private Timer entreDeux;
        private bool fini = false;

        #region Contructeur
        public EcranClavier()
        {
        }
        #endregion

        //Fonction: On créer une textbox cachée pour récupérer les infos du périphérique
        public override bool OpenConnection()
        {
            OnChangeLabelStatus("Status: Device opened...");
            Program.LogFich.Info("[EcranClavier] Pret a la lecture");

            this.textBox = new TextBox();
            this.textBox.Name = "textBox";
            this.textBox.TabIndex = 2;
            this.textBox.Size = new System.Drawing.Size(0, 0);
            this.textBox.Focus();
            this.FenetreMere.Controls.Add(this.textBox);
            this.FenetreMere.ResumeLayout(true);

            //this.textBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);
            this.textBox.TextChanged += new EventHandler(textBox_TextChanged);
            this.textBox.Enabled = true;
            OnChangeLabelStatus("Status: Waiting for an ID...");
            return true;
        }

        //Fonction: Fonction histoire de "fermer" le périphérique
        public override void CloseConnection()
        {
            OnChangeLabelStatus("Status: Device closed...");
            Program.LogFich.Info("[EcranClavier] Fermeture connection");
            textBox.Enabled = false;
            //this.textBox.KeyPress -= new KeyPressEventHandler(textBox_KeyPress);
            textBox.Dispose();
            textBox = null;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (fini == false)
            {
                if (entreDeux == null)
                {
                    entreDeux = new Timer();
                    entreDeux.Interval = 400;
                    entreDeux.Tick += new EventHandler(entreDeux_Tick);
                    entreDeux.Start();
                }
                else
                {
                    entreDeux.Stop();
                    entreDeux.Start();
                }
            }

        }

        void entreDeux_Tick(object sender, EventArgs e)
        {
            this.fini = true;
            entreDeux.Dispose();
            entreDeux = null;
            PeripheriqueDataReceivedEventArgs psea = new PeripheriqueDataReceivedEventArgs(textBox.Text);
            if (psea != null)
            {
                Program.LogFich.Info("[EcranClavier] Lecture MRZ = " + textBox.Text);
                base.OnDataReceived(psea);
            }
        }

        /*private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            PeripheriqueDataReceivingEventArgs pseaa = new PeripheriqueDataReceivingEventArgs();
            if (pseaa != null)
                base.OnDataReceiving(pseaa);

            this.textBox.Focus();
            if (e.KeyChar.ToString().Equals("$") || e.KeyChar == 13)
            {
                finLecture++;
                if (finLecture > 1)
                {
                    PeripheriqueDataReceivedEventArgs psea = new PeripheriqueDataReceivedEventArgs(textBox.Text);
                    if (psea != null)
                    {
                        Program.LogFich.Info("[EcranClavier] Lecture MRZ = " + textBox.Text);
                        base.OnDataReceived(psea);
                    }
                }
            }
        }*/

        public override void SendMessage(byte[] b)
        {
        }
        public override bool ReceiveMessage()
        {
            return true;
        }
        public override void Echange()
        {
        }
        public override void Nettoyer()
        {
        }
        public override void Annuler()
        {
        }
        public override void SendMessageInLocal()
        {
        }
    }
}
