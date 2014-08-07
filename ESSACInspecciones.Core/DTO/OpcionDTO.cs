using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class OpcionDTO
    {
        public int IdOpcion { get; set; }
        public int IdTipoRespuesta { get; set; }
        public string NombreOpcion { get; set; }
        public int Puntaje { get; set; }
        public int Orden { get; set; }
        public bool Active { get; set; }
    }
}
