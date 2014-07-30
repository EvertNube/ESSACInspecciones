using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    public class ServicioDTO
    {
        public int IdServicio { get; set; }
        //[StringLength(100)]
        public string NombreServicio { get; set; }
        //[StringLength(50)]
        public string NombreCorto { get; set; }
        //[StringLength(500)]
        public string Descripcion { get; set; }
        //[StringLength(6)]
        public string ColorServicio { get; set; }
        //[Required]
        public bool Active { get; set; }
    }
}
