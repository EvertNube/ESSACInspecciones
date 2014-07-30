using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class EstadoDTO
    {
        public int IdEstado { get; set; }
        //[Required]
        //[StringLength(100)]
        public string NombreEstado { get; set; }
        //[Required]
        public bool Active { get; set; }
    }
}
