﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SubSonic.Core.Remoting {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class RemotingResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RemotingResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SubSonic.Core.Remoting.RemotingResources", typeof(RemotingResources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remoting channel can not be secured: {0}.
        /// </summary>
        public static string CannotBeSecured {
            get {
                return ResourceManager.GetString("CannotBeSecured", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can not create channel sink for remote url: {0}.
        /// </summary>
        public static string CannotCreateChannelSink {
            get {
                return ResourceManager.GetString("CannotCreateChannelSink", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remoting channel has already been registered: {0}.
        /// </summary>
        public static string ChannelNameAlreadyRegistered {
            get {
                return ResourceManager.GetString("ChannelNameAlreadyRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Cast From {0} To {1}.
        /// </summary>
        public static string InvalidCast {
            get {
                return ResourceManager.GetString("InvalidCast", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid url {0}.
        /// </summary>
        public static string InvalidUrl {
            get {
                return ResourceManager.GetString("InvalidUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type is not remotable by reference: {0}.
        /// </summary>
        public static string NotRemotableByReference {
            get {
                return ResourceManager.GetString("NotRemotableByReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Uri already exists: {0}.
        /// </summary>
        public static string SetObjectUriForMarshal__UriExists {
            get {
                return ResourceManager.GetString("SetObjectUriForMarshal__UriExists", resourceCulture);
            }
        }
    }
}
