using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class InmuebleDTO
    {
        public int IdInmueble { get; set; }
        //[StringLength(300)]
        public string NombreInmueble { get; set; }
        //[StringLength(500)]
        public string Direccion { get; set; }
        //[Required]
        public int IdCliente { get; set; }
        //[Required]
        public bool Active { get; set; }

        public ClienteDTO Cliente { get; set; }

        public List<PlantillaDTO> Plantillas { get; set; }
        public int IdPlantilla { get; set; }
    }
}
