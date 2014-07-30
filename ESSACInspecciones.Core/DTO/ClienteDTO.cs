using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class ClienteDTO
    {
        public int IdCliente { get; set; }
        //[Required]
        //[StringLength(300)]
        public string NombreEmpresa { get; set; }
        //[Required]
        //[StringLength(50)]
        public string Telefono1 { get; set; }
        //[Required]
        //[StringLength(50)]
        public string Telefono2 { get; set; }
        public ContactoDTO contacto { get; set; }
        public List<ContactoDTO> Contactos { get; set; }
        public bool Active { get; set; }
    }
}
