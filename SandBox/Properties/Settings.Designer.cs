﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SandBox.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Demandez le transfert et posez le rondier...")]
        public string LibelleMessageDeister {
            get {
                return ((string)(this["LibelleMessageDeister"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Demandez le transfert et posez le rondier...")]
        public string LibelleMessageMagneta {
            get {
                return ((string)(this["LibelleMessageMagneta"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("*************** ALERTE ***************")]
        public string LibelleTitreFenetreSRI {
            get {
                return ((string)(this["LibelleTitreFenetreSRI"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8000")]
        public int PortSocket {
            get {
                return ((int)(this["PortSocket"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Veuillez passer un badge...")]
        public string LibelleMessageLecteurPortSerie {
            get {
                return ((string)(this["LibelleMessageLecteurPortSerie"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Pour fermer l\'application, veuillez cliquer sur le bouton Quitter.")]
        public string LibelleMessageQuitter {
            get {
                return ((string)(this["LibelleMessageQuitter"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Veuillez passer une pièce d\'identité...")]
        public string LibelleMessagePieceID {
            get {
                return ((string)(this["LibelleMessagePieceID"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Problème concernant le réseau.")]
        public string LibelleTitreFenetreErreurReseau {
            get {
                return ((string)(this["LibelleTitreFenetreErreurReseau"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Problème concernant le périphérique")]
        public string LibelleTitreFenetreErreurPeripherique {
            get {
                return ((string)(this["LibelleTitreFenetreErreurPeripherique"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Problème concernant les requêtes.")]
        public string LibelleTitreFenetreErreurRequete {
            get {
                return ((string)(this["LibelleTitreFenetreErreurRequete"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool debug {
            get {
                return ((bool)(this["debug"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("En attente de lecture")]
        public string LibelleMessageDataLogicPortSerie {
            get {
                return ((string)(this["LibelleMessageDataLogicPortSerie"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0043")]
        public string CodeSiteWIEGAND {
            get {
                return ((string)(this["CodeSiteWIEGAND"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool isMachineName {
            get {
                return ((bool)(this["isMachineName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string forceMachineName {
            get {
                return ((string)(this["forceMachineName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("##")]
        public string CharNewLine {
            get {
                return ((string)(this["CharNewLine"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string UrlImageScreenSign {
            get {
                return ((string)(this["UrlImageScreenSign"]));
            }
            set {
                this["UrlImageScreenSign"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int indexScreen {
            get {
                return ((int)(this["indexScreen"]));
            }
            set {
                this["indexScreen"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TELEMAQUE")]
        public string nomApplication {
            get {
                return ((string)(this["nomApplication"]));
            }
            set {
                this["nomApplication"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost/safeware/telemaque_new/")]
        public string urlApplication {
            get {
                return ((string)(this["urlApplication"]));
            }
            set {
                this["urlApplication"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int indexScreenSign {
            get {
                return ((int)(this["indexScreenSign"]));
            }
            set {
                this["indexScreenSign"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool showLoading {
            get {
                return ((bool)(this["showLoading"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool cookieHttpOnly {
            get {
                return ((bool)(this["cookieHttpOnly"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Veuillez cliquer sur cette barre pour connecter les lecteurs de badge et d\'identi" +
            "té")]
        public string labelRedDevice {
            get {
                return ((string)(this["labelRedDevice"]));
            }
        }
    }
}