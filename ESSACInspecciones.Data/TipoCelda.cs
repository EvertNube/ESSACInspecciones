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
    
    public partial class TipoCelda
    {
        public TipoCelda()
        {
            this.SeccionBody = new HashSet<SeccionBody>();
        }
    
        public int IdTipoCelda { get; set; }
        public string Nombre { get; set; }
        public bool Active { get; set; }
    
        public virtual ICollection<SeccionBody> SeccionBody { get; set; }
    }
}
