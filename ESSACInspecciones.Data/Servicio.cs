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
    
    public partial class Servicio
    {
        public Servicio()
        {
            this.ServicioEstado = new HashSet<ServicioEstado>();
            this.Tarea = new HashSet<Tarea>();
        }
    
        public int IdServicio { get; set; }
        public string Nombre { get; set; }
        public string NombreCorto { get; set; }
        public string Descripcion { get; set; }
        public string Color { get; set; }
        public bool Active { get; set; }
    
        public virtual ICollection<ServicioEstado> ServicioEstado { get; set; }
        public virtual ICollection<Tarea> Tarea { get; set; }
    }
}
