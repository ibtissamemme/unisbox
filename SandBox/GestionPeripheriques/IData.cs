using System;
using System.Windows.Forms;

namespace SandBox
{
    public interface IData
    {
        WebBrowser webBrowser
        {
            get;
            set;
        }

        event ChangeLabelStatusIDataEventArgs ChangeLabelStatusIData;

        //Fonction: On recoit la trame brute et on effectue le traitement
        void traiter(string s);

        //Fonction: Les données sont exploitables, on envoie au site web
        bool envoyer(Uri url, string nomMachine);
    }
}
