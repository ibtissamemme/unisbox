using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SandBox
{
    public abstract class Peripherique
    {
        private FormPeripherique fenetreMere;

        public FormPeripherique FenetreMere
        {
            set { this.fenetreMere = value; }
            get { return this.fenetreMere; }
        }

        private LecteurManager _lecteurManager;

        public LecteurManager lecteurManager
        {
            set { this._lecteurManager = value; }
            get { return this._lecteurManager; }
        }

        public abstract bool OpenConnection();
        public abstract void CloseConnection();
        public abstract void SendMessage(byte[] b);
        public abstract bool ReceiveMessage();
        public abstract void Echange();
        public abstract void Nettoyer();
        public abstract void Annuler();
        public abstract void SendMessageInLocal();

        //Event "En cours de réception"
        public delegate void PeripheriqueDataReceivingEventHandler(object sender, PeripheriqueDataReceivingEventArgs e);
        public event PeripheriqueDataReceivingEventHandler DataReceiving;

        //Event "Réception terminée"
        public delegate void PeripheriqueDataReceivedEventHandler(object sender, PeripheriqueDataReceivedEventArgs e);
        public event PeripheriqueDataReceivedEventHandler DataReceived;

        //Event "Changer le message dans le statusbar de la fenetre
        public delegate void ChangeLabelStatusEventHandler(string s);
        public event ChangeLabelStatusEventHandler ChangeLabelStatus;

        //The event-invoking method that derived classes can override.
        protected virtual void OnChangeLabelStatus(string s)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            if (ChangeLabelStatus != null)
            {
                ChangeLabelStatus(s);
            }
        }

        //The event-invoking method that derived classes can override.
        protected virtual void OnDataReceiving(PeripheriqueDataReceivingEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            PeripheriqueDataReceivingEventHandler handler = DataReceiving;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //The event-invoking method that derived classes can override.
        protected virtual void OnDataReceived(PeripheriqueDataReceivedEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            PeripheriqueDataReceivedEventHandler handler = DataReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
