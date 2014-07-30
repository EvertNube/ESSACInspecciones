using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    public class SeccionBodyDTO
    {
        public int IdSeccionBody { get; set; }
        public int IdSeccion { get; set; }
        public int IdTipoCelda { get; set; }
        public int IdTipoTag { get; set; }
        public string Descripcion { get; set; }
        public int Rowspan { get; set; }
        public int Colspan { get; set; }
        public bool BackgroundColor { get; set; }
        public int NumeroFila { get; set; }
        public int Orden { get; set; }
        public bool Active { get; set; }

        public List<string> GroupTextBox { get; set; }
        public List<int> GroupSelect { get; set; }
        public string Respuesta { get; set; }
        //public RespuestaDTO Respuesta { get; set; }
    }
}
