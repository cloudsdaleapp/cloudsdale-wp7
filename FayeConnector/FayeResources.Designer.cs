﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17626
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cloudsdale.FayeConnector {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class FayeResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FayeResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Cloudsdale.FayeConnector.FayeResources", typeof(FayeResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{&quot;channel&quot;:&quot;/meta/connect&quot;,&quot;clientId&quot;:&quot;%CLIENTID%&quot;,&quot;connectionType&quot;:&quot;websocket&quot;,&quot;ext&quot;:{&quot;auth_token&quot;:&quot;:auth&quot;}}].
        /// </summary>
        internal static string Connect {
            get {
                return ResourceManager.GetString("Connect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{&quot;channel&quot;:&quot;/meta/disconnect&quot;,&quot;clientId&quot;:&quot;%CLIENTID%&quot;}].
        /// </summary>
        internal static string Disconnect {
            get {
                return ResourceManager.GetString("Disconnect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{&quot;channel&quot;:&quot;/meta/handshake&quot;,&quot;version&quot;:&quot;1.0&quot;,&quot;supportedConnectionTypes&quot;:[&quot;websocket&quot;],&quot;ext&quot;:{&quot;auth_token&quot;:&quot;:auth&quot;}}].
        /// </summary>
        internal static string Handshake {
            get {
                return ResourceManager.GetString("Handshake", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{&quot;channel&quot;:&quot;/meta/subscribe&quot;,&quot;clientId&quot;:&quot;%CLIENTID%&quot;,&quot;subscription&quot;:&quot;%CHANNEL%&quot;,&quot;ext&quot;:{&quot;auth_token&quot;:&quot;:auth&quot;}}].
        /// </summary>
        internal static string Subscribe {
            get {
                return ResourceManager.GetString("Subscribe", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{&quot;channel&quot;:&quot;/meta/unsubscribe&quot;,&quot;clientId&quot;:&quot;%CLIENTID%&quot;,&quot;subscription&quot;:&quot;%CHANNEL%&quot;,&quot;ext&quot;:{&quot;auth_token&quot;:&quot;:auth&quot;}}].
        /// </summary>
        internal static string Unsubscribe {
            get {
                return ResourceManager.GetString("Unsubscribe", resourceCulture);
            }
        }
    }
}
