using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace SandBox
{
    public class LecteurPortSerie : PortSerie
    {


        #region Constructeur
        public LecteurPortSerie(string nomPort, int bitsSeconde, int bitsDonnees, string parite, string bitsArret, string controleFlux, int timeout)
            : base(nomPort, bitsSeconde, bitsDonnees, parite, bitsArret, controleFlux, timeout)
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            serialPort.DiscardNull = true;
        }
        #endregion

        #region Properties
        /* public LecteurManager LecteurManager
        {
            get { return lecteurManager; }
            set { lecteurManager = value; }
        }*/
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
                Program.LogFich.Info("[LecteurPortSerie] Ouverture du port " + serialPort.PortName);
                return true;
            }
            catch
            {
                OnChangeLabelStatus("Status: Device disconnected...");
                Program.LogFich.Error("[LecteurPortSerie] Peripherique non branché " + serialPort.PortName);
                MessageBox.Show("Erreur, Périphérique non connecté. Veuillez le connecter puis redémarrer l'application.");
                return false;
            }
        }

        //Fonction: cf classe PortSerie
        public override void CloseConnection()
        {
            serialPort.Close();
            OnChangeLabelStatus("Status: Device closed...");
            Program.LogFich.Info("[LecteurPortSerie] Fermeture du port " + serialPort.PortName);
        }

        //Event "En cours de réception de données"
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            PeripheriqueDataReceivingEventArgs receiving = new PeripheriqueDataReceivingEventArgs();
            if (receiving != null)
            {
                base.OnDataReceiving(receiving);
            }

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

                lecture = lecteurManager.CorrectionTailleTrame(i, lecture);
                for (int cpt = 0; cpt < lecture.Length; cpt++)
                {
                    Program.LogFich.Info("[LecteurPortSerie]: Donnee brut recu = " + lecture.GetValue(cpt));
                }

                //Nettoyage des caracteres avant/apres
                /*for (int cpt = 0; cpt < lecture.Length; cpt++)
                {
                    if (lecteurManager.AvantByte != 0x00)
                    {
                        if (lecture[cpt].Equals(lecteurManager.AvantByte))
                        {
                            lecture = lecteurManager.Nettoyer(cpt, lecture);
                            Program.LogFich.Info("[LecteurPortSerie]: Suppression byte avant, indice = " + cpt);
                        }
                    }
                    if (lecteurManager.ApresByte != 0x00)
                    {
                        if (lecture[cpt].Equals(lecteurManager.ApresByte))
                        {
                            lecture = lecteurManager.Nettoyer(cpt, lecture);
                            Program.LogFich.Info("[LecteurPortSerie]: Suppression byte apres, indice = " + cpt);
                        }
                    }
                }
                for (int cpt = 0; cpt < lecture.Length; cpt++)
                    Program.LogFich.Info("[LecteurPortSerie]: Resultat du nettoyage = " + lecture.GetValue(cpt));
                */

                //Conversion en string
                string trameBrut = lecteurManager.ByteToString(lecture, lecture.Length);
                Program.LogFich.Info("[LecteurPortSerie]: Conversion en string = " + trameBrut);

                //Nettoyage
                Program.LogFich.Info("[LecteurPortSerie]: Code Avant à nettoyer = " + lecteurManager.Avant);
                Program.LogFich.Info("[LecteurPortSerie]: Code Après à nettoyer = " + lecteurManager.Apres);
                trameBrut = lecteurManager.Nettoyer(trameBrut);
                Program.LogFich.Info("[LecteurPortSerie]: Nettoyage = " + trameBrut);                

                PeripheriqueDataReceivedEventArgs DataReceived = new PeripheriqueDataReceivedEventArgs(trameBrut);
                if (DataReceived != null)
                {
                    base.OnDataReceived(DataReceived);
                }

                OnChangeLabelStatus("Status: Receiving data...[Succeeded]");
                Program.LogFich.Info("[LecteurPortSerie] Réception terminée");
            }
            catch (Exception ex)
            {
                OnChangeLabelStatus("Status: Receiving data...[Failed]");
                Program.LogFich.Error("[LecteurPortSerie] Erreur de reception: " + ex.ToString());
            }
        }

        public override void Annuler()
        {
            this.CloseConnection();
        }

        public override void SendMessage(byte[] b)
        {
            serialPort.Write(b, 0, b.Length);
        }
    }
}