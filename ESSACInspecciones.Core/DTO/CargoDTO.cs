using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class CargoDTO
    {
        public int IdCargo { get; set; }
        public string Nombre { get; set; }
        public bool Active { get; set; }
    }
}
