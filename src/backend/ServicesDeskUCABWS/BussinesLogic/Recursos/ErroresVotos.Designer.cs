﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServicesDeskUCABWS.BussinesLogic.Recursos {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ErroresVotos {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErroresVotos() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ServicesDeskUCABWS.BussinesLogic.Recursos.ErroresVotos", typeof(ErroresVotos).Assembly);
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
        ///   Looks up a localized string similar to El comentario no puede ser mayor a 300 caracteres.
        /// </summary>
        public static string COMENTARIO_FUERA_RANGO {
            get {
                return ResourceManager.GetString("COMENTARIO_FUERA_RANGO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to El ticket ingresado no se encuentra registrado en el sistema .
        /// </summary>
        public static string ERROR_TICKET_DESC {
            get {
                return ResourceManager.GetString("ERROR_TICKET_DESC", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to El usuario no está registrado en el sistema .
        /// </summary>
        public static string ERROR_USUARIO_DESC {
            get {
                return ResourceManager.GetString("ERROR_USUARIO_DESC", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to El formato ingresado para el ID del usuario  o elticket no es valido.
        /// </summary>
        public static string FORMATO_NO_VALIDO {
            get {
                return ResourceManager.GetString("FORMATO_NO_VALIDO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Su turno de votación ha expirado, no puede ingresar este voto.
        /// </summary>
        public static string VOTACION_EXPIRADA {
            get {
                return ResourceManager.GetString("VOTACION_EXPIRADA", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usted no tiene los permisos para participar en esta votación.
        /// </summary>
        public static string VOTO_NO_PERMITIDO {
            get {
                return ResourceManager.GetString("VOTO_NO_PERMITIDO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to El voto ingresado no es valido, el voto debe ser &quot;Aprobado&quot;, &quot;Rechazado&quot; o &quot;Pendiente&quot;.
        /// </summary>
        public static string VOTO_NO_VALIDO {
            get {
                return ResourceManager.GetString("VOTO_NO_VALIDO", resourceCulture);
            }
        }
    }
}
