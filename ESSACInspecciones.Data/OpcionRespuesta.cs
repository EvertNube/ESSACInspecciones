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
    
    public partial class OpcionRespuesta
    {
        public int IdOpcion { get; set; }
        public int IdTipoRespuesta { get; set; }
        public string Nombre { get; set; }
        public int Puntaje { get; set; }
        public int Orden { get; set; }
        public bool Active { get; set; }
    
        public virtual TipoTag TipoTag { get; set; }
    }
}
