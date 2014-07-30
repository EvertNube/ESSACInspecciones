using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    public class SeccionDTO
    {
        public int IdSeccion { get; set; }
        public int? IdSeccionPadre { get; set; }
        public int IdPlantilla { get; set; }
        public string Nombre { get; set; }
        //public int Colspan { get; set; }
        public int Pagina { get; set; }
        public int Orden { get; set; }
        public bool Active { get; set; }

        public List<SeccionDTO> SubSecciones { get; set; }
        //public List<TableHeaderDTO> TableHeaders { get; set; }
        public List<SeccionBodyDTO> SeccionBodys { get; set; }
    }

}
