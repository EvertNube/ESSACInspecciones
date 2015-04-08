//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ESSACInspecciones.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Usuario
    {
        public Usuario()
        {
            this.Protocolo = new HashSet<Protocolo>();
            this.Tarea = new HashSet<Tarea>();
        }
    
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string InicialesNombre { get; set; }
        public string Email { get; set; }
        public string Cuenta { get; set; }
        public string Pass { get; set; }
        public bool Estado { get; set; }
        public Nullable<System.DateTime> FechaRegistro { get; set; }
        public int IdRol { get; set; }
        public Nullable<int> IdCargo { get; set; }
        public string RutaFirma { get; set; }
        public Nullable<int> IdCliente { get; set; }
    
        public virtual Cargo Cargo { get; set; }
        public virtual Cliente Cliente { get; set; }
        public virtual ICollection<Protocolo> Protocolo { get; set; }
        public virtual Rol Rol { get; set; }
        public virtual ICollection<Tarea> Tarea { get; set; }
    }
}
