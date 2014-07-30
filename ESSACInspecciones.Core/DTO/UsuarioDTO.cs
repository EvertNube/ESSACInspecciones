using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        //[Required]
        //[StringLength(50)]
        public string Nombre { get; set; }
        //[StringLength(50)]
        public string Email { get; set; }
        //[StringLength(10)]
        public string InicialesNombre { get; set; }
        //[Required]
        //[StringLength(40)]
        public string Cuenta { get; set; }
        //[Required]
        //[StringLength(64)]
        public string Pass { get; set; }
        public bool Active { get; set; }
        //[Required]
        public int IdRol { get; set; }
        public int? IdCargo { get; set; }
    }
}
