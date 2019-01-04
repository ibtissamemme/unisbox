using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SandBox;

namespace SandBox
{
    public class Trame
    {
        //Infos de la trame
        private string name;
        private string type;
        private long deviceNumber;
        private string dateHeure;
        private string status_bit0 = "RAM error occurred";
        private string status_bit1 = "Memory Empty";
        private string status_bit2 = "Battery has to be changed";
        private string status_bit3 = "Memory overflow";
        private int nbrCheckpoints;

        //Infos d'une ronde
        private string dateEnTraitement;
        private string heureEnTraitement;
        private string data;
        private List<DeisterRonde> rondes = new List<DeisterRonde>();

        #region Constructeur
        public Trame()
        {
        }
        public Trame(byte[] trame)
        {
            this.Name = Convert.ToChar(trame.GetValue(0)).ToString();

            StringBuilder sb0 = new StringBuilder();
            sb0.Append(Convert.ToChar(trame.GetValue(1)).ToString());
            sb0.Append(Convert.ToChar(trame.GetValue(2)).ToString());
            this.Type = sb0.ToString();

            StringBuilder sb1 = new StringBuilder();
            sb1.Append(Convert.ToChar(trame.GetValue(3)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(4)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(5)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(6)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(7)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(8)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(9)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(10)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(11)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(12)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(13)).ToString());
            sb1.Append(Convert.ToChar(trame.GetValue(14)).ToString());
            this.DeviceNumber = long.Parse(sb1.ToString());

            StringBuilder sb2 = new StringBuilder();
            sb2.Append(Convert.ToChar(trame.GetValue(15)).ToString());
            sb2.Append(Convert.ToChar(trame.GetValue(16)).ToString());
            sb2.Append("-");
            sb2.Append(Convert.ToChar(trame.GetValue(17)).ToString());
            sb2.Append(Convert.ToChar(trame.GetValue(18)).ToString());
            sb2.Append("-");
            sb2.Append(Convert.ToChar(trame.GetValue(19)).ToString());
            sb2.Append(Convert.ToChar(trame.GetValue(20)).ToString());
            sb2.Append("T");
            sb2.Append(Convert.ToChar(trame.GetValue(21)).ToString());
            sb2.Append(Convert.ToChar(trame.GetValue(22)).ToString());
            sb2.Append(":");
            sb2.Append(Convert.ToChar(trame.GetValue(23)).ToString());
            sb2.Append(Convert.ToChar(trame.GetValue(24)).ToString());
            this.dateHeure = sb2.ToString();

            if (trame[25] == 0x30) status_bit0 = "OK";
            if (trame[26] == 0x30) status_bit1 = "OK";
            if (trame[27] == 0x30) status_bit2 = "OK";
            if (trame[28] == 0x30) status_bit3 = "OK";

            StringBuilder sb6 = new StringBuilder();
            sb6.Append(Convert.ToChar(trame[29]));
            sb6.Append(Convert.ToChar(trame[30]));
            sb6.Append(Convert.ToChar(trame[31]));
            sb6.Append(Convert.ToChar(trame[32]));
            this.nbrCheckpoints = Convert.ToInt32("0x" + sb6.ToString(), 16);
        }
        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        public long DeviceNumber
        {
            get { return deviceNumber; }
            set { deviceNumber = value; }
        }
        public string DateHeure
        {
            get { return dateHeure; }
            set { dateHeure = value; }
        }
        public string Data
        {
            get { return data; }
            set { data = value; }
        }
        public string Status_bit0
        {
            get { return status_bit0; }
            set { status_bit0 = value; }
        }
        public string Status_bit1
        {
            get { return status_bit1; }
            set { status_bit1 = value; }
        }
        public string Status_bit2
        {
            get { return status_bit2; }
            set { status_bit2 = value; }
        }
        public string Status_bit3
        {
            get { return status_bit3; }
            set { status_bit3 = value; }
        }
        public int NbrCheckpoints
        {
            get { return nbrCheckpoints; }
            set { nbrCheckpoints = value; }
        }
        public List<DeisterRonde> Rondes
        {
            get { return rondes; }
            set { rondes = value; }
        }
        #endregion

        public void AddRonde(byte[] trameRonde)
        {
            if (trameRonde.GetValue(0).Equals(Convert.ToByte(0x64)))
            {
                StringBuilder sb1 = new StringBuilder();
                sb1.Append(Convert.ToChar(trameRonde.GetValue(1)).ToString());
                sb1.Append(Convert.ToChar(trameRonde.GetValue(2)).ToString());
                sb1.Append("-");
                sb1.Append(Convert.ToChar(trameRonde.GetValue(3)).ToString());
                sb1.Append(Convert.ToChar(trameRonde.GetValue(4)).ToString());
                sb1.Append("-");
                sb1.Append(Convert.ToChar(trameRonde.GetValue(5)).ToString());
                sb1.Append(Convert.ToChar(trameRonde.GetValue(6)).ToString());
                // correction de l'année de la date sur 4 caractères
                dateEnTraitement = "20" + sb1.ToString();

                StringBuilder sb2 = new StringBuilder();
                sb2.Append("T");
                sb2.Append(Convert.ToChar(trameRonde.GetValue(7)).ToString());
                sb2.Append(Convert.ToChar(trameRonde.GetValue(8)).ToString());
                sb2.Append(":");
                sb2.Append(Convert.ToChar(trameRonde.GetValue(9)).ToString());
                sb2.Append(Convert.ToChar(trameRonde.GetValue(10)).ToString());
                heureEnTraitement = sb2.ToString();

                StringBuilder sb3 = new StringBuilder();
                sb3.Append(Convert.ToChar(trameRonde.GetValue(11)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(12)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(13)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(14)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(15)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(16)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(17)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(18)).ToString());
                data = sb3.ToString();
            }
            else
            {
                StringBuilder sb2 = new StringBuilder();
                sb2.Append("T");
                sb2.Append(Convert.ToChar(trameRonde.GetValue(0)).ToString());
                sb2.Append(Convert.ToChar(trameRonde.GetValue(1)).ToString());
                sb2.Append(":");
                sb2.Append(Convert.ToChar(trameRonde.GetValue(2)).ToString());
                sb2.Append(Convert.ToChar(trameRonde.GetValue(3)).ToString());
                heureEnTraitement = sb2.ToString();

                StringBuilder sb3 = new StringBuilder();
                sb3.Append(Convert.ToChar(trameRonde.GetValue(4)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(5)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(6)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(7)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(8)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(9)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(10)).ToString());
                sb3.Append(Convert.ToChar(trameRonde.GetValue(11)).ToString());
                data = sb3.ToString();
            }
            string dateheure = dateEnTraitement + heureEnTraitement;
            DeisterRonde r = new DeisterRonde(dateheure, data);
            rondes.Add(r);
        }

        public void AfficherTrameInfo()
        {
            Program.LogFich.Info("[DeisterTrame] Name:" + Name);
            Program.LogFich.Info("[DeisterTrame] Type:" + Type);
            Program.LogFich.Info("[DeisterTrame] Device Number:" + DeviceNumber);
            Program.LogFich.Info("[DeisterTrame] Date:" + DateHeure);
            Program.LogFich.Info("[DeisterTrame] Status:" + Status_bit0);
            Program.LogFich.Info("[DeisterTrame] Status:" + Status_bit1);
            Program.LogFich.Info("[DeisterTrame] Status:" + Status_bit2);
            Program.LogFich.Info("[DeisterTrame] Status:" + Status_bit3);
            Program.LogFich.Info("[DeisterTrame] Nbr Checkpoints:" + NbrCheckpoints);
        }

        public void AfficherListeRonde()
        {
            foreach (DeisterRonde r in rondes)
            {
                Program.LogFich.Info("[DeisterTrame] Date:" + r.DateHeure + "   " + r.Data);
            }
        }
    }
}
