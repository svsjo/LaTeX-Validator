﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LaTeX_Validator {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\entep04\\Desktop\\Infos\\EigeneProjektarbeiten\\T2000\\T2000_Latex")]
        public string RootDirectoryPath {
            get {
                return ((string)(this["RootDirectoryPath"]));
            }
            set {
                this["RootDirectoryPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\entep04\\Desktop\\Infos\\EigeneProjektarbeiten\\T2000\\T2000_Latex\\03_Preambl" +
            "e\\glossaries.tex")]
        public string GlossaryPath {
            get {
                return ((string)(this["GlossaryPath"]));
            }
            set {
                this["GlossaryPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Users\\entep04\\Desktop\\Infos\\EigeneProjektarbeiten\\T2000\\T2000_Latex\\03_Preambl" +
            "e")]
        public string PreambleDirectoryPath {
            get {
                return ((string)(this["PreambleDirectoryPath"]));
            }
            set {
                this["PreambleDirectoryPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IgnoreSectionLabels {
            get {
                return ((bool)(this["IgnoreSectionLabels"]));
            }
            set {
                this["IgnoreSectionLabels"] = value;
            }
        }
    }
}
