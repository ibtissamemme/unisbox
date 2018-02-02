using System;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Web;
using System.Windows.Forms;

namespace LauncherUnisBox
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string url = "http://asimov/safeware/telemaque/";
            string nameApp = "TELEMAQUE";

            // By URL
            if (GetQueryStringParameters().Count > 0 && GetQueryStringParameters().Get("url").Trim().Length > 0)
            {
                url = GetQueryStringParameters().Get("url");
            }

            if (GetQueryStringParameters().Count > 0 && GetQueryStringParameters().Get("nomApplication").Trim().Length > 0)
            {
                nameApp = GetQueryStringParameters().Get("nomApplication");
            }

            string location = "";
            string urlSetup = "";

            if (nameApp == "TELEMAQUE")
            {
                location = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\setupTELEMAQUEUnisBox.exe";
                urlSetup = url + "install/setupTELEMAQUEUnisBox.exe";
            }
            else
            {
                location = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\setupOMNIGARDEUnisBox.exe";
                urlSetup = url + "install/setupOMNIGARDEUnisBox.exe";
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(url, urlSetup, location));

        }

        public static NameValueCollection GetQueryStringParameters()
        {
            NameValueCollection nameValueTable = new NameValueCollection();

            if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.ActivationUri != null)
            {
                string queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
                nameValueTable = HttpUtility.ParseQueryString(queryString);
            }

            return nameValueTable;
        }
    }
}
