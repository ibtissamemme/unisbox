using System.Windows.Forms;
using System;

namespace SandBox
{
    public class EcranClavierPeriph : Peripherique
    {

        private TextBox textBox;
        private Timer entreDeux;
        private bool fini = false;

        #region Contructeur
        public EcranClavierPeriph()
        {
        }
        #endregion

        //Fonction: On créer une textbox cachée pour récupérer les infos du périphérique
        public override bool OpenConnection()
        {
            Program.LogFich.Info("[EcranClavierPeriph] Pret a la lecture");

            this.textBox = new TextBox();
            this.textBox.Name = "textBox";
            this.textBox.TabIndex = 2;
            this.textBox.Size = new System.Drawing.Size(0, 0);
            this.textBox.Multiline = true;
            this.textBox.Focus();
            this.FenetreMere.Controls.Add(this.textBox);
            this.FenetreMere.ResumeLayout(true);

            //this.textBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);
            this.textBox.TextChanged += new EventHandler(textBox_TextChanged);

            this.textBox.Enabled = true;

            return true;
        }

        //Fonction: Fonction histoire de "fermer" le périphérique
        public override void CloseConnection()
        {
            Program.LogFich.Info("[EcranClavierPeriph] Fermeture connexion");
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
                    entreDeux.Interval = 200;
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
            this.Filtrer();
            PeripheriqueDataReceivedEventArgs psea = new PeripheriqueDataReceivedEventArgs(textBox.Text);
            if (psea != null)
            {
                Program.LogFich.Info("[EcranClavierPeriph] Lecture = " + textBox.Text);
                base.OnDataReceived(psea);
            }
            entreDeux.Dispose();
            entreDeux = null;
        }

        /*private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            textBox.Text += e.KeyChar.ToString();
            PeripheriqueDataReceivedEventArgs psea = new PeripheriqueDataReceivedEventArgs(textBox.Text);
            if (psea != null)
            {
                Program.LogFich.Info("[EcranClavierPeriph] Lecture = " + textBox.Text);
                base.OnDataReceived(psea);
            }
        }*/

        public void Filtrer()
        {
            textBox.Text = textBox.Text.Replace(Environment.NewLine, "");
            char[] specials = { '&', 'é', '"', '\'', '(', '-', 'è', '_', 'ç', 'à' };
            char[] chiffres = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

            for (int i = 0; i < specials.Length; i++)
            {
                textBox.Text = textBox.Text.Replace(specials[i], chiffres[i]);
            }

        }

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
