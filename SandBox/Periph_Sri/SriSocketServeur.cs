using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SandBox
{
    public class SriSocketServeur
    {
        //Une messageBox spéciale qui permet d'être afficher en 1er plan même si l'application est réduit
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);

        private TcpListener serverSocket;
        private TcpClient clientSocket;
        private bool connecter;
        private Thread thread;

        #region Constructeur
        public SriSocketServeur()
        {
            clientSocket = default(TcpClient);
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }
        #endregion

        #region Properties
        public TcpListener ServerSocket
        {
            get { return serverSocket; }
            set { serverSocket = value; }
        }
        public TcpClient ClientSocket
        {
            get { return clientSocket; }
            set { clientSocket = value; }
        }
        public Thread Thread
        {
            get { return thread; }
            set { thread = value; }
        }
        #endregion

        //Fonction: On arrete l'écoute du port
        public void Stop()
        {
            Program.LogFich.Info("[SriSocketServeur] Stop() [Debut]");
            try
            {
                if (thread != null)
                {
                    if (thread.IsAlive)
                    {
                        thread.Join(100);
                        Program.LogFich.Info("[SriSocketServeur] Stop() - Thread en join");
                    }
                }

                if (serverSocket != null)
                {
                    serverSocket.Stop();
                    Program.LogFich.Info("[SriSocketServeur] Stop() [Fin] - Socket Termine");
                }
            }
            catch (Exception e)
            {
                Program.LogFich.Error("[SriSocketServeur] Stop() - ERREUR:" + e.ToString());
            }

        }

        //Fonction: On lance l'écoute du port
        public void Listen()
        {
            Program.LogFich.Info("[SriSocketServeur] Listen() [Debut]");
            try
            {
                IPAddress ip = Dns.GetHostEntry(Functions.getHost()).AddressList[0];
                serverSocket = new TcpListener(ip, SandBox.Properties.Settings.Default.PortSocket);
                serverSocket.Start();
                connecter = true;
                Program.LogFich.Info("[SriSocketServeur] Listen() [Fin] - OK");
            }
            catch (Exception e)
            {
                connecter = false;
                Program.LogFich.Error("[SriSocketServeur] Listen() - ERREUR: " + e.ToString());
            }

            while (connecter)
            {
                try
                {
                    Program.LogFich.Info("[SriSocketServeur] Listen() - Waiting connection from client");
                    clientSocket = serverSocket.AcceptTcpClient();
                    Program.LogFich.Info("[SriSocketServeur] Listen() - Connection accepted");

                    NetworkStream networkStream = clientSocket.GetStream();
                    byte[] bytesFrom = new byte[10025];
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);

                    StringBuilder sb = new StringBuilder();
                    sb.Append(dataFromClient);
                    MessageBox(IntPtr.Zero, sb.ToString(), SandBox.Properties.Settings.Default.LibelleTitreFenetreSRI, 0x001030);
                }
                catch (SocketException e)
                {
                    serverSocket.Stop();
                    connecter = false;
                    Program.LogFich.Error("[SriSocketServeur] Listen() - Erreur SocketException: " + e.ToString());
                }
                catch (Exception ex)
                {
                    Program.LogFich.Error("[SriSocketServeur] Listen() - Erreur: " + ex.ToString());
                }
            }
        }
    }
}
