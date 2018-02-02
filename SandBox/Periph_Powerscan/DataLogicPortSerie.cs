using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace SandBox
{
    public class DataLogicPortSerie : PortSerie
    {
        private DataLogicManager dataLogicManager;
        List<string> numSource;
        List<string> numDest;

        int colisNbOrigine;

        #region Constructeur
        public DataLogicPortSerie(string nomPort, int bitsSeconde, int bitsDonnees, string parite, string bitsArret, string controleFlux, int timeout)
            : base(nomPort, bitsSeconde, bitsDonnees, parite, bitsArret, controleFlux, timeout)
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            serialPort.DiscardNull = true;
        }
        #endregion

        #region Properties
        public DataLogicManager DataLogicManager
        {
            get { return dataLogicManager; }
            set { dataLogicManager = value; }
        }
        #endregion

        //Fonction: cf classe PortSerie
        public override bool OpenConnection()
        {
            try
            {
                if (serialPort.IsOpen == true)
                {
                    serialPort.Close();
                }
                serialPort.Open();
                serialPort.DiscardOutBuffer();

                OnChangeLabelStatus("Status: Device opened...");
                Program.LogFich.Info("[DataLogicPortSerie] Ouverture du port " + serialPort.PortName);
                return true;
            }
            catch
            {
                OnChangeLabelStatus("Status: Device disconnected...");
                Program.LogFich.Error("[DataLogicPortSerie] Peripherique non branché " + serialPort.PortName);
                MessageBox.Show("Erreur, Périphérique non connecté. Veuillez le connecter puis redémarrer l'application.");
                return false;
            }
        }

        //Fonction: cf classe PortSerie
        public override void CloseConnection()
        {
            serialPort.Close();
            OnChangeLabelStatus("Status: Device closed...");
            Program.LogFich.Info("[DataLogicPortSerie] Fermeture du port " + serialPort.PortName);
        }

        //Event "En cours de réception de données"
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            /* Non actif : problème de theard
                         * PeripheriqueDataReceivingEventArgs receiving = new PeripheriqueDataReceivingEventArgs();
                        if (receiving != null)
                        {
                            base.OnDataReceiving(receiving);
                        }
            */
            try
            {
                byte[] lecture = new byte[serialPort.ReadBufferSize];
                int i = 0;

                //On récupère la trame provenant du port série
                byte b;
                bool termine = false;
                do
                {
                    try
                    {
                        b = Convert.ToByte(serialPort.ReadByte());
                        lecture[i] = b;
                        i++;
                    }
                    catch
                    {
                        termine = true;
                    }
                }
                while (!termine);

                lecture = dataLogicManager.CorrectionTailleTrame(i, lecture);
                Program.LogFich.Info("[DataLogicPortSerie]: Donnee brut recu = " + lecture);

                string trameBrut = dataLogicManager.ByteToString(lecture, lecture.Length);
                Program.LogFich.Info("[DataLogicPortSerie]: Donnee string recu = " + trameBrut);

                dataLogicManager.NumeroBadgeString = trameBrut;
                dataLogicManager.ScanEncours = false;

                //PeripheriqueDataReceivedEventArgs DataReceived = new PeripheriqueDataReceivedEventArgs(trameBrut);
                //if (DataReceived != null)
                //{
                //    base.OnDataReceived(DataReceived);
                //}
                /* Non actif : problème de theard
                                OnChangeLabelStatus("Status: Receiving data...[Succeeded]");
                                Program.LogFich.Info("[DataLogicPortSerie] Réception terminée");
                 */
            }
            catch
            {
                /* Non actif : problème de theard
                                OnChangeLabelStatus("Status: Receiving data...[Failed]");
                */
                Program.LogFich.Error("[DataLogicPortSerie] Erreur de reception");
            }
        }

        // Envoi d'une trame au périphérique
        public override void SendMessage(byte[] b)
        {
            serialPort.Write(b, 0, b.Length);
        }
        // Envoi d'une trame au périphérique
        public override void SendMessageInLocal()
        {
            string entree = dataLogicManager.DouchetteRetour;
            byte[] byteBrutTest = DataLogicManager.transformeStringEnByte(entree);
            serialPort.Write(byteBrutTest, 0, byteBrutTest.Length);
        }

        public override void Annuler()
        {
            //dataLogicManager.NumeroBadgeString = "ANNULER";
            dataLogicManager.ScanEncours = false;
            dataLogicManager.scrutation = false;
        }
        // Lecture d'un code
        public string Reader()
        {
            Program.LogFich.Info("[DataLogicPortSerie]: Traitement en cours (debut)");
            dataLogicManager.NumeroBadgeString = "";
            dataLogicManager.ScanEncours = true;

            Application.DoEvents();
            while (dataLogicManager.ScanEncours == true)
            {
                Application.DoEvents();

            }
            if (dataLogicManager.NumeroBadgeString != "")
            {
                // nettoyage du code retourné
                string tempoEntree = dataLogicManager.NumeroBadgeString;

                tempoEntree = tempoEntree.Replace("CR", null);
                tempoEntree = tempoEntree.Replace("LF", null);
                tempoEntree = tempoEntree.Replace("\"", "");
                // Extration douchette et code
                string lecteur = tempoEntree.Substring(0, 4);
                dataLogicManager.NumeroBadgeString = tempoEntree.Substring(4, tempoEntree.Length - 4);
                // Mise à disposition du retour pour envoie à la douchette
                dataLogicManager.DouchetteRetour = lecteur + dataLogicManager.messageOK;
                SendMessageInLocal();
            }
            dataLogicManager.ScanEncours = false;
            dataLogicManager.scrutation = false;
            CloseConnection();
            return dataLogicManager.NumeroBadgeString;
        }

        public int verificationNumero(string entree)
        {   // gestion de la liste des numéro et indique si le numéro est trouvé ou non et si c'est le dernier
            // 0 : pas trouvé
            // 1 : trouvé
            // 2 : dernier

            int trouver = 0;
            if (entree != "")
            {
                foreach (string bb in numSource)
                {
                    if (entree == bb.ToString())
                    {
                        entree = "";
                        numDest.Add(bb.ToString());
                        numSource.RemoveAt(numSource.IndexOf(bb));
                        trouver = 1;
                        break;
                    }
                }
            }
            if (numSource.Count == 0) trouver = 2;
            return trouver;
        }

        // Lecture d'un code
        public string Search(string valin)
        {
            Program.LogFich.Info("[DataLogicPortSerie]: Traitement en cours (debut)");

            numSource = new List<string>();
            numDest = new List<string>();

            string[] entree = valin.Split(';');
            numSource.AddRange(entree);
            colisNbOrigine = numSource.Count;

            dataLogicManager.NumeroBadgeString = "";
            dataLogicManager.ScanEncours = true;
            dataLogicManager.scrutation = true;

            while (dataLogicManager.scrutation)
            {
                dataLogicManager.ScanEncours = true;
                Application.DoEvents();
                while (dataLogicManager.ScanEncours == true)
                {
                    Application.DoEvents();
                }
                if (dataLogicManager.NumeroBadgeString != "" && dataLogicManager.NumeroBadgeString != "ANNULER")
                {
                    int etatRetour;
                    // nettoyage du code retourné
                    string tempoEntree = dataLogicManager.NumeroBadgeString;

                    tempoEntree = tempoEntree.Replace("CR", null);
                    tempoEntree = tempoEntree.Replace("LF", null);
                    tempoEntree = tempoEntree.Replace("\"", "");
                    // Extration douchette et code
                    string lecteur = tempoEntree.Substring(0, 4);
                    dataLogicManager.NumeroBadgeString = tempoEntree.Substring(4, tempoEntree.Length - 4);

                    etatRetour = verificationNumero(dataLogicManager.NumeroBadgeString);
                    string messageRetour = "";
                    if (etatRetour == 0) { messageRetour = dataLogicManager.messageNOK; }
                    if (etatRetour == 1) { messageRetour = dataLogicManager.messageOK; }
                    if (etatRetour == 2) { messageRetour = dataLogicManager.messageEND; }

                    messageRetour = messageRetour.Replace("#POS", numDest.Count.ToString() + "/" + colisNbOrigine.ToString());

                    // Mise à disposition du retour pour envoie à la douchette
                    dataLogicManager.DouchetteRetour = lecteur + messageRetour;
                    SendMessageInLocal();

                    if (etatRetour == 2)
                    {   // Final
                        dataLogicManager.NumeroBadgeString = "TERMINER";
                        dataLogicManager.ScanEncours = false;
                        dataLogicManager.scrutation = false;

                    }
                }
            }
            CloseConnection();

            return dataLogicManager.NumeroBadgeString;
        }

    }
}