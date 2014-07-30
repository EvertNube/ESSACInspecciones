using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class ContactoDTO
    {
        public int IdContacto { get; set; }
        //[StringLength(100)]
        public string Nombre { get; set; }
        //[StringLength(50)]
        public string Telefono { get; set; }

        //[StringLength(100)]
        public string Email { get; set; }

        //[StringLength(200)]
        public string Area { get; set; }

        //[StringLength(100)]
        public string Cargo { get; set; }

        //[StringLength(50)]
        public string Celular { get; set; }

        //[StringLength(10)]
        public string Anexo { get; set; }
        public int IdCliente { get; set; }
        //[Required]
        public bool Active { get; set; }
        //[Required]
        public bool Default { get; set; }
    }
}
