using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    public class RespuestaDTO
    {
        public int IdProtocolo { get; set; }
        public int IdSeccionBody { get; set; }
        public string Respuesta { get; set; }
        public bool Active { get; set; }
    }
}
