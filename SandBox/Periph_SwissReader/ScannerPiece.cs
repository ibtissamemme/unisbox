using System;
using System.IO;
using System.Windows.Forms;
using gx;
using pr;

namespace SandBox
{
    public class ScannerPiece : Peripherique
    {
        string MRZ = "";
        public PassportReader pr;

        public string visiteurID { get; set; } //No MRZ mais visiteurid pour scan de la pièce

        #region Contructeur
        public ScannerPiece()
        {

            pr = new PassportReader();

            visiteurID = "";

            if (!pr.IsValid())
            {
                throw new InvalidProgramException("Lecteur non prêt.");
            }
            else
            {
                try
                {
                    pr.UseDevice(0, (int)PR_USAGEMODE.PR_UMODE_FULL_CONTROL);
                }
                catch (Exception ex)
                {
                    Program.LogFich.Error(ex.ToString());
                    MessageBox.Show("Lecteur ARH non prêt.");
                    //throw new InvalidProgramException("Lecteur non prêt.");

                }
            }
        }
        #endregion

        //Fonction: On créer une textbox cachée pour récupérer les infos du périphérique
        public override bool OpenConnection()
        {
            OnChangeLabelStatus("Status: Device opened...");
            Program.LogFich.Info("[ScannerPiece] Pret a la lecture");

            OnChangeLabelStatus("Status: Waiting for an ID...");

            return true;
        }

        //Fonction: Fonction histoire de "fermer" le périphérique
        public override void CloseConnection()
        {
            OnChangeLabelStatus("Status: Device closed...");
            Program.LogFich.Info("[ScannerPiece] Fermeture connection");
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
            string temp = scanner_piece();

            PeripheriqueDataReceivedEventArgs psea = new PeripheriqueDataReceivedEventArgs(temp);
            if (psea != null)
            {
                Program.LogFich.Info("[ScannerPiece] Lecture MRZ = " + temp);
                base.OnDataReceived(psea);
            }
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

        public string scanner_piece()
        {

            prDoc doc;
            try
            {

                //if (!pr.IsCalibrated((int)PR_WINDOW_ID.PR_OW_DEFAULT))
                //    pr.Calibrate((int)PR_WINDOW_ID.PR_OW_DEFAULT);

                pr.Capture();

                /*if (pr.TestDocument((int)PR_WINDOW_ID.PR_OW_DEFAULT) == (int)PR_TESTDOC.PR_TD_IN)
                {
                    MessageBox.Show("OK");
                    pr.Capture();
                }
                else
                {
                    MessageBox.Show("PAS OK");
                    pr.CloseDevice();
                    pr.Close();
                    return "";
                }*/

            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de se connecter au scanner de pièces d'identité");
                Program.LogFich.Error("[ScannerPiece] Impossible de se connecter au scanner de pièces d'identité");
                Program.LogFich.Error(ex.ToString());

                //pr.CloseDevice();
                //pr.Close();
                return "";
            }

            try
            {
                if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "recto.jpeg"))
                    File.Delete(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "recto.jpeg");

                doc = pr.Recognize(0);

                pr.SaveImage(0, (int)PR_LIGHT.PR_LIGHT_WHITE, (int)PR_IMAGE_TYPE.PR_IT_DOCUMENT, Environment.ExpandEnvironmentVariables("%TEMP%\\") + "recto.jpeg", (int)GX_IMGFILEFORMATS.GX_JPEG);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.ToString());
                Program.LogFich.Error("[ScannerPiece] Erreur : " + ex.ToString());
                //pr.CloseDevice();
                //pr.Close();
                return "";
            }

            try
            {
                //pr.SelfTest((int)PR_SELFTEST_ELEMENTS.PRV_ST_LIGHT_SWITCH);


                gxImage img = doc.FieldImage((int)PR_DOCFIELD.PR_DF_VIZ_FACE);
                if (img != null)
                {
                    if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg"))
                        File.Delete(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg");
                    img.Save(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg", (int)GX_IMGFILEFORMATS.GX_JPEG);
                    img.Dispose();
                }

                MRZ = doc.Field((int)PR_DOCFIELD.PR_DF_MRZ1) + doc.Field((int)PR_DOCFIELD.PR_DF_MRZ2) + doc.Field((int)PR_DOCFIELD.PR_DF_MRZ3);
                if (MRZ.Equals(""))
                {
                    Program.LogFich.Info("[ScannerPiece] pas de piste MRZ sur le recto");
                }

                /*if (MessageBox.Show("Pour Scanner le verso, tournez la pièce et cliquer sur Oui.", "Scan Verso", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    pr.CloseDevice();
                    //pr.Close();
                    scan_verso();
                }*/


                if (this.MRZ.Equals("") && visiteurID.Equals(""))
                {
                    MessageBox.Show("Le Document n'a pas de piste MRZ");
                    Program.LogFich.Info("[ScannerPiece] Le Document n'a pas de piste MRZ");
                }

                return MRZ;

            }
            catch (Exception ex)
            {
                Program.LogFich.Error("[ScannerPiece] n'a pas pu lire le recto du document : " + ex.ToString());
                return "";
            }
            finally
            {
                if (pr != null)
                {
                    pr.ResetDocument();
                    //pr.CloseDevice();
                    //pr.Close();
                    //pr.Dispose();
                }
            }

        }

        private void scan_verso()
        {
            //PassportReader pr = new PassportReader();
            try
            {
                //pr.UseDevice(0, (int)PR_USAGEMODE.PR_UMODE_FULL_CONTROL);
                pr.Capture();


                if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "verso.jpeg"))
                    File.Delete(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "verso.jpeg");
                pr.SaveImage(0, (int)PR_LIGHT.PR_LIGHT_WHITE, (int)PR_IMAGE_TYPE.PR_IT_DOCUMENT, Environment.ExpandEnvironmentVariables("%TEMP%/") + "verso.jpeg", (int)GX_IMGFILEFORMATS.GX_JPEG);

                prDoc doc = pr.Recognize(0);
                gxImage img = doc.FieldImage((int)PR_DOCFIELD.PR_DF_VIZ_FACE);
                if (img != null)
                {
                    if (File.Exists(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg"))
                        File.Delete(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg");
                    img.Save(Environment.ExpandEnvironmentVariables("%TEMP%\\") + "PhotoIdentite.jpeg", (int)GX_IMGFILEFORMATS.GX_JPEG);
                }

                string temp;
                temp = doc.Field((int)PR_DOCFIELD.PR_DF_MRZ1) + doc.Field((int)PR_DOCFIELD.PR_DF_MRZ2) + doc.Field((int)PR_DOCFIELD.PR_DF_MRZ3);
                if (!temp.Equals(""))
                {
                    MRZ = temp;
                }
                else
                {
                    Program.LogFich.Info("[ScannerPiece] pas de piste MRZ sur le verso");
                }

            }
            catch (Exception ex)
            {
                Program.LogFich.Error("[ScannerPiece] n'a pas pu lire le verso du document : " + ex.ToString());
            }
            finally
            {
                //pr.CloseDevice();
                //pr.Close();
                //pr.Dispose();
                //pr = null;
            }
        }
    }
}

