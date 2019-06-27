using System.Runtime.InteropServices;
using System.Text;
using System;
using SandBox.Periph_STID;
using System.Timers;
using System.Diagnostics;

namespace SandBox
{
    public class STID
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
        /*   [DllImport(".\\Resources\\STID\\SSCPlibMifareGlobal.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
           private static extern int SSCP_Connect();
           [DllImport(".\\Resources\\STID\\SSCPlibMifareGlobal.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
           private static extern int SSCP_Disconnect();
           [DllImport(".\\Resources\\STID\\SSCPlibMifareGlobal.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
           private static extern int SSCPr_Scan_14443_A(ref byte nbCard, ref byte CardType, ref byte CardSize, ref byte Info, ref byte UIDLen, [Out] byte[] UID, [Out]byte[] ATS);
           */




        private static void setRs32usb()
        {
            UInt16 status = SSCPlibMIFARE.SSCP_READER_BAD_PARAM_DATA_ERR;
            string mesg = "";


            status = SSCPlibMIFARE.SSCP_SetCOMType(SSCPlibMIFARE.ct_rs232);
            if ((byte)status == SSCPlibMIFARE.SSCP_OK)
            {
                mesg = "communication type changed to RS232/USB";
                Program.LogFich.Info("communication type result : " + mesg);
            }
            else
            {
                SSCPlibMIFARE.SSCP_GetErrorMsg(0, status, ref mesg);
                Program.LogFich.Error("communication type result : " + mesg);
            }

        }

        private static void setPortNum(string port)
        {
            UInt16 status;
            string mesg = "";

            status = SSCPlibMIFARE.SSCP_Serial_SetPort(port);
            if ((byte)status == SSCPlibMIFARE.SSCP_OK)
            {
                mesg = "Serial COMPort changed";
                Program.LogFich.Info("Serial COMPort changed result : " + mesg);
            }

            else
            {
                SSCPlibMIFARE.SSCP_GetErrorMsg(0, status, ref mesg);
                Program.LogFich.Error("Serial COMPort changed result : " + mesg);
            }
        }


        private static void setBaudRate()
        {
            UInt16 status;
            string baudrate = STID.E_BAUDRATE.EBAUD38400.ToString();
            byte br;
            string mesg = "";


            switch (baudrate)
            {
                case "9600": br = 0; break;
                case "19200": br = 1; break;
                case "38400": br = 2; break;
                case "57200": br = 3; break;
                case "115200": br = 4; break;
                default: br = 0; break;
            }

            status = SSCPlibMIFARE.SSCP_Serial_SetBaudRate(2);
            if ((byte)status == SSCPlibMIFARE.SSCP_OK)
            {
                mesg = "Baudrate changed";
                Program.LogFich.Info("Baudrate Set result : " + mesg);
            }

            else
            {
                SSCPlibMIFARE.SSCP_GetErrorMsg(0, status, ref mesg);
                Program.LogFich.Error("Baudrate Set result : " + mesg);
            }


        }

        private static bool setConnect()
        {

            UInt16 status;
            string mesg = "";
            bool gConnected = false;
            status = SSCPlibMIFARE.SSCP_Connect();
            if ((byte)status == SSCPlibMIFARE.SSCP_OK)
            {
                gConnected = true;
                Program.LogFich.Info("openning serial communication port : " + mesg);

            }
            else
            {
                gConnected = false;
                Program.LogFich.Error("Error openning serial communication port : " + status.ToString());
            }
            return gConnected;
        }
        public static void OpenConnexionSTID(string port)
        {


            // port = port.Replace("COM", "");
            //   int iport = int.Parse(port);


            UInt16 status;
            string mesg = "";

            status = SSCPlibMIFARE.SSCP_Initialize();

            if ((byte)status == SSCPlibMIFARE.SSCP_OK)
            {
                mesg = "Library initialised";
                setBaudRate();
                setPortNum(port.ToString());
                setRs32usb();
                if (setConnect())
                {

                    Program.LogFich.Info("Before call to tag STID");

                }

            }
            else
            {
                SSCPlibMIFARE.SSCP_GetErrorMsg(0, status, ref mesg);
            }



        }

        public static void CloseConnexionSTID()
        {
            SSCPlibMIFARE.SSCP_Disconnect();
            Program.LogFich.Info("STID : CloseConnexion");
        }




        public static string GetTagSTID(string idtechnostid)
        {
            byte info = 0;
            byte UIDLen = 0;
            byte CardType = 0;
            byte CardSize = 0;
            byte[] UID = new byte[10];
            byte[] ATS = new byte[20];
            byte nbCard = 0;

            string resultat = "";
            UInt16 status;

            string msg = "";
            Stopwatch timerForReader = new Stopwatch();
            timerForReader.Start();
            while (timerForReader.Elapsed.TotalSeconds < 5 && UIDLen == 0)
            {
                

                status = SSCPlibMIFARE.SSCPr_Scan_14443_A(ref nbCard, ref CardType, ref CardSize, ref info, ref UIDLen, UID, ATS);

                if ((byte)status == SSCPlibMIFARE.SSCP_OK)
                {
                    msg = "Card    # =" + nbCard.ToString() + '\n';
                    msg += "Card type =" + CardType.ToString() + '\n';
                    msg += "Card size =" + (CardSize >> 1).ToString() + '\n';
                    msg += "Info   =" + info.ToString() + '\n';
                    msg += "UIDLen =" + UIDLen.ToString() + '\n';
                    msg += "UID    =" + System.BitConverter.ToString(UID, 0, UIDLen).Replace("-", "") + '\n';
                    msg += "ATS    =" + System.BitConverter.ToString(ATS, 0, ATS[0]).Replace("-", "");
                    Program.LogFich.Info("STID : badge lu : " + msg);
                    resultat = System.BitConverter.ToString(UID, 0, UIDLen).Replace("-", "");
                    Program.LogFich.Info("STID : Brut : [" + resultat + "]");
                    if (resultat.StartsWith("0"))
                    {
                        resultat = resultat.Substring(1);
                        Program.LogFich.Info("STID : sans zéro : [" + resultat + "]");
                    }
                    if (!nbCard.ToString().Equals("0"))
                    {
                        switch (CardType.ToString())
                        {
                            case "1":
                                resultat = string.IsNullOrEmpty(idtechnostid)? "1B" + resultat:idtechnostid+resultat;
                                break;
                            case "2":
                                resultat = string.IsNullOrEmpty(idtechnostid) ? "1A" + resultat : idtechnostid + resultat;
                                break;
                            default:
                                resultat = string.IsNullOrEmpty(idtechnostid) ? "03" + resultat : idtechnostid + resultat;
                                break;
                        }

                        Program.LogFich.Info("STID : avec prefixe : [" + resultat+"]");
                    }


                }
                else
                {

                    SSCPlibMIFARE.SSCP_GetErrorMsg(0, status, ref msg);
                    Program.LogFich.Error("STID : GetTag = " + msg);

                }

            }
            timerForReader.Stop();
            return resultat;
        }
    }


}
