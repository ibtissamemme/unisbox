using System.Runtime.InteropServices;
using System.Text;

namespace SandBox
{
    public class MIL100
    {
        public enum E_BAUDRATE
        {
            EBAUD_UNKNOWN = -1,		// Unknown
            EBAUD110 = 110,		// 110 bits/sec
            EBAUD300 = 300,		// 300 bits/sec
            EBAUD600 = 600,		// 600 bits/sec
            EBAUD1200 = 1200,		// 1200 bits/sec
            EBAUD2400 = 2400,		// 2400 bits/sec
            EBAUD4800 = 4800,		// 4800 bits/sec
            EBAUD9600 = 9600,		// 9600 bits/sec (default)
            EBAUD14400 = 14400,	// 14400 bits/sec
            EBAUD19200 = 19200,	// 19200 bits/sec 
            EBAUD38400 = 38400,	// 38400 bits/sec
            EBAUD57600 = 57600,	// 57600 bits/sec
            EBAUD115200 = 115200,	// 115200 bits/sec
        };

        //API Gunnebo LecteurOperateur dll
        [DllImport(".\\Resources\\MIL100\\LecteurOperateur.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int OpenConnexion(byte by_noport, E_BAUDRATE e_vitesse);
        [DllImport(".\\Resources\\MIL100\\LecteurOperateur.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseConnexion();
        [DllImport(".\\Resources\\MIL100\\LecteurOperateur.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetTag(int dw_timeout, out byte pby_tagtype, out byte pby_taglen, byte[] pby_tagval);

        public static void OpenConnexionMIL100(string port)
        {
            port = port.Replace("COM", "");
            int iport = int.Parse(port);

            int i_rc;
            i_rc = OpenConnexion((byte)iport, MIL100.E_BAUDRATE.EBAUD38400);

            Program.LogFich.Info("MIL100 : OpenConnexion = " + i_rc);
        }

        public static void CloseConnexionMIL100()
        {
            CloseConnexion();
            Program.LogFich.Info("MIL100 : CloseConnexion");
        }

        public static string GetTagMIL100()
        {
            byte by_tagtype;
            byte by_taglen;
            byte[] aby_tagval = new byte[20];
            int i_rc;
            StringBuilder o_msg = new StringBuilder();

            string resultat = "";

            /*i_rc = OpenConnexion((byte)iport, E_BAUDRATE.EBAUD38400);
            if (i_rc != 0)
            {
                //lbMsg.Items.Add("OpenConnexion : " + i_rc);
                Program.LogFich.Info("MIL100 : OpenConnexion = " + i_rc);
            }
            else
            {*/
            i_rc = GetTag(10000, out by_tagtype, out by_taglen, aby_tagval);
            if (i_rc != 0)
            {
                if (i_rc == 1)
                {
                    //lbMsg.Items.Add("Aucun badge lu");
                    Program.LogFich.Info("MIL100 : Aucun badge lu");

                }
                else
                {
                    //lbMsg.Items.Add("GetTag : " + i_rc);
                    Program.LogFich.Info("MIL100 : GetTag = " + i_rc);
                }
            }
            else
            {
                //o_msg = new StringBuilder();
                o_msg.Append(by_tagtype.ToString("X2"));
                for (int i_count = 0; i_count < by_taglen && i_count < aby_tagval.Length; i_count++)
                {
                    o_msg.AppendFormat("{0:X2}", aby_tagval[i_count]);
                }
                //lbMsg.Items.Add(o_msg.ToString());

                resultat = o_msg.ToString();
                Program.LogFich.Info("MIL100 : Tag lu = " + o_msg.ToString());

                if (resultat.Substring(2, 1) == "0")
                {
                    resultat = resultat.Substring(3);
                    resultat = "03" + resultat;
                }

                Program.LogFich.Info("MIL100 : Tag lu sans zéro = " + resultat);

            }
            //i_rc = CloseConnexion();
            //}
            //TODO a modifier pour Gunnebo Luxembourg       
            /*string myloco_msg = "";
            if (!string.IsNullOrEmpty(o_msg.ToString()))
            {
                ulong decValue = ulong.Parse(o_msg.ToString(), System.Globalization.NumberStyles.HexNumber);
                return decValue.ToString();
                myloco_msg = o_msg.ToString().Substring(0, 2) + o_msg.ToString().Substring(3);
                Program.LogFich.Info("MIL100 : Tag lu après conversion= " + myloco_msg);
            }

            return myloco_msg;*/
            return resultat;
        }
    }


}
