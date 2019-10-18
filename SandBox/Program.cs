using log4net;
using log4net.Config;
using System;
using System.Deployment.Application;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace SandBox
{
    static class Program
    {
        public static readonly log4net.ILog LogFich = LogManager.GetLogger(typeof(Program));

        public static int webcam_resolution = -1;

        private static Mutex m_Mutex;

        [STAThread]
        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveHandler;

            //SSL                        
            //System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            //SplashForm sf;

            XmlConfigurator.Configure(new FileInfo(Directory.GetCurrentDirectory() + ".\\log4net.config"));
            log4net.Config.BasicConfigurator.Configure();

            LogFich.Info("=============================================================================================");
            LogFich.Info("Program.Main() Execution");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (ApplicationDeployment.IsNetworkDeployed)
            {

                Cursor cur = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    if (ApplicationDeployment.CurrentDeployment.CheckForUpdate())
                    {
                        Cursor.Current = cur;
                        MessageBox.Show("An updated version is available.");
                        LogFich.Info("An updated version is available.");

                        Cursor.Current = Cursors.WaitCursor;
                        ApplicationDeployment.CurrentDeployment.Update();

                        Cursor.Current = cur;
                        MessageBox.Show("Update downloaded, application will now restart.");

                        Application.Restart();
                        Environment.Exit(0);
                        LogFich.Info("Application will now restart.");

                    }
                    else
                    {
                        Cursor.Current = cur;
                        //MessageBox.Show("No updates available at this time.");
                    }
                }
                catch (Exception e)
                {
                    LogFich.Error("Mise à jour non effectuée : " + e.ToString());
                }

                finally
                {
                    Cursor.Current = cur;
                }
            }

            //sf = new SplashForm();
            //sf.affiche("Loading, please wait...");

            // String[] urls = new String[5];         

            //string getVars = "?machine=" + Functions.getHost();
            String tmpurl, nomApplication, tmpurlPortal;

            // By URL
            if (Functions.GetQueryStringParameters().Count > 0 && Functions.GetQueryStringParameters().Get("url").Trim().Length > 0)
            {
                SandBox.Properties.Settings.Default.urlApplication = Functions.GetQueryStringParameters().Get("url");
                SandBox.Properties.Settings.Default.Save();
                SandBox.Properties.Settings.Default.Reload();
            }

            // By URL
            if (Functions.GetQueryStringParameters().Count > 0 && Functions.GetQueryStringParameters().Get("urlPortal").Trim().Length > 0)
            {
                SandBox.Properties.Settings.Default.urlPortal = Functions.GetQueryStringParameters().Get("urlPortal");
                SandBox.Properties.Settings.Default.Save();
                SandBox.Properties.Settings.Default.Reload();
            }

            if (Functions.GetQueryStringParameters().Count > 0 && Functions.GetQueryStringParameters().Get("nomApplication").Trim().Length > 0)
            {
                SandBox.Properties.Settings.Default.nomApplication = Functions.GetQueryStringParameters().Get("nomApplication");
                SandBox.Properties.Settings.Default.Save();
                SandBox.Properties.Settings.Default.Reload();
            }

            // By Args Parameters
            if (args != null)
            {
                if (args.Length >= 1 && args[0] != null)
                {
                    SandBox.Properties.Settings.Default.urlApplication = args[0];
                    SandBox.Properties.Settings.Default.Save();
                    SandBox.Properties.Settings.Default.Reload();
                }
                if (args.Length >= 2 && args[1] != null)
                {
                    SandBox.Properties.Settings.Default.nomApplication = args[1];
                    SandBox.Properties.Settings.Default.Save();
                    SandBox.Properties.Settings.Default.Reload();
                }
                if (args.Length >= 3 && args[0] != null)
                {
                    SandBox.Properties.Settings.Default.urlPortal = args[2];
                    SandBox.Properties.Settings.Default.Save();
                    SandBox.Properties.Settings.Default.Reload();
                }
            }

            if (SandBox.Properties.Settings.Default.urlApplication.Substring(SandBox.Properties.Settings.Default.urlApplication.Length - 1, 1) != "/")
            {
                SandBox.Properties.Settings.Default.urlApplication = SandBox.Properties.Settings.Default.urlApplication + "/";
                SandBox.Properties.Settings.Default.Save();
                SandBox.Properties.Settings.Default.Reload();
            }

            if (SandBox.Properties.Settings.Default.urlApplication.Substring(SandBox.Properties.Settings.Default.urlPortal.Length - 1, 1) != "/")
            {
                SandBox.Properties.Settings.Default.urlPortal = SandBox.Properties.Settings.Default.urlPortal + "/";
                SandBox.Properties.Settings.Default.Save();
                SandBox.Properties.Settings.Default.Reload();
            }

            tmpurl = SandBox.Properties.Settings.Default.urlApplication;
            LogFich.Info("urlApplication=" + tmpurl);

            tmpurlPortal = SandBox.Properties.Settings.Default.urlApplication;
            LogFich.Info("urlPortal=" + tmpurlPortal);

            nomApplication = SandBox.Properties.Settings.Default.nomApplication;
            LogFich.Info("nomApplication=" + nomApplication);


            SandBox.Properties.Settings.Default.indexScreenSign = SandBox.Properties.Settings.Default.indexScreenSign;
            SandBox.Properties.Settings.Default.UrlImageScreenSign = SandBox.Properties.Settings.Default.UrlImageScreenSign;
            SandBox.Properties.Settings.Default.Save();
            SandBox.Properties.Settings.Default.Reload();

            //----------téléchargement des fichiers xml---------------            
            /*CookieAwareWebClient wc = new CookieAwareWebClient();

            try
            {
                wc.Credentials = CredentialCache.DefaultCredentials;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.ToString());
                LogFich.Error("Erreur sur \"DefaultCredentials\" : " + ex.ToString());
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                LogFich.Error("Erreur sur \"DefaultCredentials\" : " + ex.ToString());
                return;
            }*/
            //}*/            

            string nomfichier;
            string[] tmp;
            string urldownload;

            //----------téléchargement du fichier xml devices---------------

            urldownload = tmpurl;
            tmp = urldownload.Split('/');
            nomfichier = tmp[tmp.Length - 2];
            //"http://asimov/safeware/accueil/?machine=FERAOUN"
            //http://asimov/safeware/accueil/devices/devices.asp?machine=FERAOUN"                

            //Byte[] requestHtml = null;
            /*try
            {
                requestHtml = wc.DownloadData(urldownload + "devices/devices.asp" + getVars);
            }
            catch (WebException we)
            {
                LogFich.Error(we.ToString());
                MessageBox.Show(we.ToString());
                return;
            }

            //try
            //{
            System.IO.FileStream fs = new System.IO.FileStream(Environment.ExpandEnvironmentVariables("%TEMP%\\") + nomfichier + "_devices.xml", System.IO.FileMode.Create);
            fs.Write(requestHtml, 0, requestHtml.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();*/


            //string adresse;
            //----------téléchargement des fichiers xml option admin---------------
            /*
            for (int i = 0; i < nburl; i++)
            {
                urldownload = urls[i];
                tmp = urldownload.Split('/');
                nomfichier = tmp[tmp.Length - 2];
                //"http://asimov/safeware/omnigarde/admin/optionadmin.asp?machine=FERAOUN"
                //"http://asimov/safeware/accueil/admin/optionadmin.asp?machine=FERAOUN"
                if (nomfichier.ToUpper().Contains("OMNIGARDE"))
                {
                    Byte[] requestHtml = null;
                    try
                    {
                        adresse = urldownload + "admin/optionadmin.asp" + getVars;
                        requestHtml = wc.DownloadData(adresse);
                    }
                    catch (WebException we)
                    {
                        LogFich.Error(we.ToString());
                        MessageBox.Show(we.ToString());
                        return;
                    }

                    System.IO.FileStream fs = new System.IO.FileStream(Environment.ExpandEnvironmentVariables("%TEMP%\\") + nomfichier + "_optionadmin.xml", System.IO.FileMode.Create);
                    fs.Write(requestHtml, 0, requestHtml.Length);
                    fs.Flush();
                    fs.Close();
                    fs.Dispose();
                }

            }*/

            //----------téléchargement des icones---------------

            //"http://asimov/safeware/omnigarde/admin/optionadmin.asp?machine=FERAOUN"
            //"http://asimov/safeware/accueil/admin/optionadmin.asp?machine=FERAOUN"

            //Byte[] requestHtml = null;
            /*try
            {
                adresse = urldownload + "favicon.ico";
                requestHtml = wc.DownloadData(adresse);
            }
            catch (WebException we)
            {
                LogFich.Error(we.ToString());
                MessageBox.Show(we.ToString());
                return;
            }

            System.IO.FileStream fsfav = new System.IO.FileStream(Environment.ExpandEnvironmentVariables("%TEMP%\\") + nomfichier + "_ico.ico", System.IO.FileMode.Create);
            fsfav.Write(requestHtml, 0, requestHtml.Length);
            fsfav.Flush();
            fsfav.Close();
            fsfav.Dispose();
            */

            /*for (int i = 0; i < nburl; i++)
            {
                urldownload = urls[i];
                try
                {
                    wc.DownloadData(urldownload + "gestion/session_abandon.asp");
                }
                catch (WebException we)
                {
                    LogFich.Error(we.ToString());
                }
            }*/

            //wc.Dispose();

            //sf.Close();
            //sf.Dispose();

            bool createdNew;
            m_Mutex = new Mutex(true, nomApplication, out createdNew);
            if (createdNew)
            {
                Application.Run(new Main());
            }
            else
            {
                MessageBox.Show("The application is already running.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }


            /*for (int i = 0; i < SandBox.Properties.Settings.Default.NbApp; i++)
            {
                Thread main1 = new Thread(new ThreadStart(SandBox.Main.foo));
                main1.SetApartmentState(ApartmentState.STA);
                main1.Name = "Thread" + (i + 1);
                main1.Start();
            }

            while (SandBox.Main.AppNumberPublic <= SandBox.Properties.Settings.Default.NbApp)
            {
                if (SandBox.Main.AppNumberPublic >= SandBox.Properties.Settings.Default.NbApp)
                {
                    sf.Close();
                    sf.Dispose();
                    break;
                }
            }*/
        }

        internal static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            string libPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            Path.DirectorySeparatorChar +
            "Resources" +
            Path.DirectorySeparatorChar;
            var assembly = Assembly.LoadFrom(libPath + args.Name + ".dll");
            return assembly;
        }
    }
}
