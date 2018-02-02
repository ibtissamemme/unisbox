using System.Text;

namespace SandBox
{
    public class PeripheriqueInfos
    {
        private string nom;
        private string type_port;
        private int bit_seconde;
        private int bit_donnee;
        private string parite;
        private string bit_arret;
        private string controle_flux;
        private string port;
        private string webservice;
        private string username;
        private string password;
        private string traitement;
        private string avant;
        private string apres;
        private int debut;
        private int longueur;
        private int longueur_max;
        private int decoupe;
        private int conversion;

        #region Constructeur
        public PeripheriqueInfos()
        {
        }

        public PeripheriqueInfos(string _nom, string _type_port, int _bit_seconde, int _bit_donnee,
            string _parite, string _bit_arret, string _controle_flux, string _port, string _webservice, string _username, string _password, string _traitement,
            string _avant, string _apres, int _debut, int _longueur, int _longueur_max,
            int _decoupe, int _conversion)
        {
            this.nom = _nom;
            this.type_port = _type_port;
            this.bit_seconde = _bit_seconde;
            this.bit_donnee = _bit_donnee;
            this.parite = _parite;
            this.bit_arret = _bit_arret;
            this.controle_flux = _controle_flux;
            this.port = _port;
            this.webservice = _webservice;
            this.username = _username;
            this.password = _password;
            this.traitement = _traitement;
            this.avant = _avant;
            this.apres = _apres;
            this.debut = _debut;
            this.longueur = _longueur;
            this.longueur_max = _longueur_max;
            this.decoupe = _decoupe;
            this.conversion = _conversion;
        }
        #endregion

        #region Properties
        public string Nom
        {
            get { return nom; }
            set { nom = value; }
        }
        public string Type_port
        {
            get { return type_port; }
            set { type_port = value; }
        }
        public int Bit_seconde
        {
            get { return bit_seconde; }
            set { bit_seconde = value; }
        }
        public int Bit_donnee
        {
            get { return bit_donnee; }
            set { bit_donnee = value; }
        }
        public string Parite
        {
            get { return parite; }
            set { parite = value; }
        }
        public string Bit_arret
        {
            get { return bit_arret; }
            set { bit_arret = value; }
        }
        public string Controle_flux
        {
            get { return controle_flux; }
            set { controle_flux = value; }
        }
        public string Port
        {
            get { return port; }
            set { port = value; }
        }
        public string Webservice
        {
            get { return webservice; }
            set { webservice = value; }
        }
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        public string Traitement
        {
            get { return traitement; }
            set { traitement = value; }
        }
        public string Avant
        {
            get { return avant; }
            set { avant = value; }
        }
        public string Apres
        {
            get { return apres; }
            set { apres = value; }
        }
        public int Debut
        {
            get { return debut; }
            set { debut = value; }
        }
        public int Longueur
        {
            get { return longueur; }
            set { longueur = value; }
        }
        public int Longueur_max
        {
            get { return longueur_max; }
            set { longueur_max = value; }
        }
        public int Decoupe
        {
            get { return decoupe; }
            set { decoupe = value; }
        }

        public int Conversion
        {
            get { return conversion; }
            set { conversion = value; }
        }

        #endregion

        //Fonction: creer un string pour les logs 
        public void afficher()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(" Nom:");
            sb.Append(nom);
            sb.Append(" | type_port:");
            sb.Append(type_port);
            sb.Append(" | bit_seconde:");
            sb.Append(bit_seconde);
            sb.Append(" | bit_donnees:");
            sb.Append(bit_donnee);
            sb.Append(" | parite:");
            sb.Append(parite);
            sb.Append(" | bit_arret:");
            sb.Append(bit_arret);
            sb.Append(" | controle_flux:");
            sb.Append(controle_flux);
            sb.Append(" | port:");
            sb.Append(port);
            sb.Append(" | webservice:");
            sb.Append(webservice);
            sb.Append(" | ws_username:");
            sb.Append(username);
            sb.Append(" | ws_password:");
            sb.Append(password);
            sb.Append(" | traitement:");
            sb.Append(traitement);
            sb.Append(" | avant:");
            sb.Append(avant);
            sb.Append(" | apres:");
            sb.Append(apres);
            sb.Append(" | debut:");
            sb.Append(debut);
            sb.Append(" | longueur:");
            sb.Append(longueur);
            sb.Append(" | longueur_max:");
            sb.Append(longueur_max);
            sb.Append(" | decoupe:");
            sb.Append(decoupe);
            sb.Append(" | conversion:");
            sb.Append(conversion);
            Program.LogFich.Info("[PeripheriqueInfos] " + sb.ToString());
        }

        //Fonction: créer un string pour le popup infos périphériques (lorsqu'on clique sur la barre d'état de la sandbox)
        public string ToStringPeriph()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Name\t\t: " + nom + "\n");
            sb.Append("Processing\t: " + traitement + "\n");
            sb.Append("Port type\t\t: " + type_port + "\n");
            try
            {
                sb.Append("Port\t\t: " + port + "\n");
                sb.Append("Bits per seconde\t: " + bit_seconde + "\n");
                sb.Append("Data bits\t\t: " + bit_donnee + "\n");
                if (parite != null)
                    sb.Append("Parity\t\t: " + parite + "\n");
                else
                    sb.Append("Parity\t\t: none\n");
                sb.Append("Stop bit\t\t: " + bit_arret + "\n");
                if (controle_flux != null)
                    sb.Append("Flow control\t: " + controle_flux + "\n");
                else
                    sb.Append("Flow control\t: none\n");

                if (traitement.Equals("BADGE"))
                {
                    sb.Append("\n\nBefore\t\t: " + avant + "\n");
                    sb.Append("After\t\t: " + apres + "\n");
                    sb.Append("Begin\t\t: " + debut + "\n");
                    sb.Append("Lentgh\t\t: " + longueur + "\n");
                    sb.Append("Lentgh max\t: " + longueur_max + "\n");
                    sb.Append("Split\t\t: " + decoupe + "\n");
                    sb.Append("Conversion\t: " + conversion + "\n");
                }
            }
            catch
            {
                //return "Erreur dans les paramétrages";
            }
            return sb.ToString();
        }
    }
}
