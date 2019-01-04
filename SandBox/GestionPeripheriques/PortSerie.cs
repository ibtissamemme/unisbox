using System.IO.Ports;

namespace SandBox
{
    public class PortSerie : Peripherique
    {
        protected SerialPort serialPort;
        protected bool connection;

        #region Constructeur
        public PortSerie(string nomPort, int bitsSeconde, int bitsDonnees, string parite, string bitsArret, string controleFlux, int timeout)
        {
            serialPort = new SerialPort();
            if (nomPort != null && !nomPort.Equals(""))
            {
                serialPort.PortName = nomPort;
                serialPort.BaudRate = bitsSeconde;
                serialPort.DataBits = bitsDonnees;
                switch (parite)
                {
                    case "Aucun":
                        serialPort.Parity = Parity.None;
                        break;
                    case "Pair":
                        serialPort.Parity = Parity.Even;
                        break;
                    case "Impair":
                        serialPort.Parity = Parity.Odd;
                        break;
                }

                switch (bitsArret)
                {
                    case "1":
                        serialPort.StopBits = StopBits.One;
                        break;
                    case "2":
                        serialPort.StopBits = StopBits.Two;
                        break;
                    case "1.5":
                        serialPort.StopBits = StopBits.OnePointFive;
                        break;
                    default:
                        serialPort.StopBits = StopBits.None;
                        break;
                }

                switch (controleFlux)
                {
                    case "Xon / Xoff":
                        serialPort.Handshake = Handshake.XOnXOff;
                        break;
                    case "Matériel":
                        serialPort.Handshake = Handshake.RequestToSend;
                        break;
                    default:
                        serialPort.Handshake = Handshake.None;
                        break;
                }
                serialPort.ReadTimeout = timeout;
            }
        }
        #endregion

        #region Properties
        public SerialPort SerialPort
        {
            get { return serialPort; }
            set { serialPort = value; }
        }
        #endregion

        public override bool OpenConnection()
        {
            return true;
        }

        public override void CloseConnection()
        {
        }

        public override void SendMessage(byte[] b)
        {
        }

        public override void SendMessageInLocal()
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
            if (serialPort.IsOpen == true)
            {
                serialPort.Close();
            }
            serialPort.Dispose();
        }
    }
}