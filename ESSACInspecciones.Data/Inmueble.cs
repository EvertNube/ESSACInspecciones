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
    
    public partial class Inmueble
    {
        public Inmueble()
        {
            this.Tarea = new HashSet<Tarea>();
            this.Plantilla = new HashSet<Plantilla>();
            this.Protocolo = new HashSet<Protocolo>();
        }
    
        public int IdInmueble { get; set; }
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public bool Active { get; set; }
    
        public virtual Cliente Cliente { get; set; }
        public virtual ICollection<Tarea> Tarea { get; set; }
        public virtual ICollection<Plantilla> Plantilla { get; set; }
        public virtual ICollection<Protocolo> Protocolo { get; set; }
    }
}
