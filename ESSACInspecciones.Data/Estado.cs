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
    
    public partial class Estado
    {
        public Estado()
        {
            this.ServicioEstado = new HashSet<ServicioEstado>();
            this.Tarea = new HashSet<Tarea>();
        }
    
        public int IdEstado { get; set; }
        public string NombreEstado { get; set; }
        public bool Active { get; set; }
    
        public virtual ICollection<ServicioEstado> ServicioEstado { get; set; }
        public virtual ICollection<Tarea> Tarea { get; set; }
    }
}
