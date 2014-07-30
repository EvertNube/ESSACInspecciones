using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    public class TableHeaderDTO
    {
        public int IdTableBody { get; set; }
        public int IdSeccion { get; set; }
        public string Descripcion { get; set; }
        public int Rowspan { get; set; }
        public int Colspan { get; set; }
        public int NumeroFila { get; set; }
        public int Orden { get; set; }
        public bool Active { get; set; }
    }
}
