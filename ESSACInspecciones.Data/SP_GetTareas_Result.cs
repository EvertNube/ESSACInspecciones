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
    
    public partial class SP_GetTareas_Result
    {
        public int IdTarea { get; set; }
        public string NombreTarea { get; set; }
        public string NombreCliente { get; set; }
        public string NombreInmueble { get; set; }
        public string NombreEstado { get; set; }
        public Nullable<System.DateTime> FechaInicio { get; set; }
        public Nullable<System.DateTime> FechaFin { get; set; }
        public bool Active { get; set; }
        public string Responsables { get; set; }
    }
}
