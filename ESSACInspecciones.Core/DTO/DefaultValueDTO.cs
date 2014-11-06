using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    [Serializable]
    public class DefaultValueDTO
    {
        public int IdPlantilla { get; set; }
        public int IdSeccionBody { get; set; }
        public string Descripcion { get; set; }
    }
}