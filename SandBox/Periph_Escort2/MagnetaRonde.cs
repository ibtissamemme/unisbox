using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SandBox
{
    public class MagnetaRonde
    {
        private DateTime date;
        private string numero;
        private string libelle;
        private string agent;
        private string sortieString;

        #region Constructeur
        public MagnetaRonde(string s)
        {
            sortieString = s;
            string[] ss = s.Split('\n');
            Regex rondeRegex = new Regex(@"^([\d]{2}.[\d]{2}.[\d]{2} [\d]{2}:[\d]{2})[\s]+R #([\d]{2})[\s]+(.{24})[\s]+(.+)$");
            Match m; string ligne;

            for (int i = 0; i < ss.Length; i++)
            {
                ligne = ss[i].Trim();
                m = rondeRegex.Match(ligne);
                if (m.Success)
                {
                    try
                    {
                        date = DateTime.Parse(m.Groups[1].Value + ":00");
                    }
                    catch
                    {
                        date = DateTime.Now;
                        Program.LogFich.Info("[Change date jour]");
                    }

                    numero = m.Groups[2].Value;
                    libelle = m.Groups[3].Value.Trim();
                    agent = m.Groups[4].Value.Trim();
                    break;
                }
            }
        }
        #endregion

        #region Properties
        public DateTime Date
        {
            get { return this.date; }
        }
        public string Numero
        {
            get { return this.numero; }
        }
        public string Libelle
        {
            get { return this.libelle; }
        }
        public string Agent
        {
            get { return this.agent; }
        }
        public string SortieString
        {
            get { return this.sortieString; }
            set { this.sortieString = value; }
        }
        #endregion
    }
}
