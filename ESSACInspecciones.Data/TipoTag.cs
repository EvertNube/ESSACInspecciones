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
    
    public partial class TipoTag
    {
        public TipoTag()
        {
            this.OpcionRespuesta = new HashSet<OpcionRespuesta>();
            this.SeccionBody = new HashSet<SeccionBody>();
        }
    
        public int IdTipoTag { get; set; }
        public string Nombre { get; set; }
        public bool Active { get; set; }
    
        public virtual ICollection<OpcionRespuesta> OpcionRespuesta { get; set; }
        public virtual ICollection<SeccionBody> SeccionBody { get; set; }
    }
}
