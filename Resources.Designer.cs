﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17626
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cloudsdale {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Cloudsdale.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to 213007855430354.
        /// </summary>
        internal static string facebookAppId {
            get {
                return ResourceManager.GetString("facebookAppId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://assets.cloudsdale.org/assets/fallback/.
        /// </summary>
        internal static string fallbackBaseUrl {
            get {
                return ResourceManager.GetString("fallbackBaseUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.cloudsdale.org/v1/users/{0}.json.
        /// </summary>
        internal static string getUserEndpoint {
            get {
                return ResourceManager.GetString("getUserEndpoint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to $2a$10$7Pfcv89Q9c/9WMAk6ySfhu.
        /// </summary>
        internal static string InternalToken {
            get {
                return ResourceManager.GetString("InternalToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://www.cloudsdale.org/v1/sessions.
        /// </summary>
        internal static string loginUrl {
            get {
                return ResourceManager.GetString("loginUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {{&quot;oauth&quot;:{{&quot;client_type&quot;:&quot;android&quot;,&quot;provider&quot;:&quot;{0}&quot;,&quot;token&quot;:&quot;{1}&quot;,&quot;uid&quot;:&quot;{2}&quot;}}}}.
        /// </summary>
        internal static string OAuthFormat {
            get {
                return ResourceManager.GetString("OAuthFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ws://push01.cloudsdale.org:80/push.
        /// </summary>
        internal static string pushUrl {
            get {
                return ResourceManager.GetString("pushUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string sendEndpoint {
            get {
                return ResourceManager.GetString("sendEndpoint", resourceCulture);
            }
        }
    }
}
