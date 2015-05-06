using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    public class ProtocoloDTO
    {
        public int IdProtocolo { get; set; }
        public int IdPlantilla { get; set; }
        public int IdInmueble { get; set; }
        //public int IdUsuario { get; set; }
        public int IdEstado { get; set; }
        public string Codigo { get; set; }
        [Required]
        public string NombreAreaProtegida { get; set; }
        public string Direccion { get; set; }
        [Required]
        public Nullable<DateTime> Fecha { get; set; }
        public string TipoPrueba { get; set; }
        public bool Active { get; set; }
        public int IdPeriodo { get; set; }
        public List<SeccionDTO> Secciones { get; set; }
        public List<RespuestaDTO> Respuestas { get; set; }

        public int HoraInicio { get; set; }
        public int MinutoInicio { get; set; }
        public int TotalPaginas { get; set; }
        public List<string> GroupDescripcion { get; set; }
        public List<int> GroupIdTableBody { get; set; }
        public EstadoDTO Estado { get; set; }
        public PlantillaDTO Plantilla { get; set; }
        public string StrFecha { get; set; }
        //public List<string> GroupTextBox { get; set; }
        //public List<int> GroupSelect { get; set; }
        //public List<int> GroupIdTableBody { get; set; }
        //public List<ESSACInspecciones.Data.Seccion> Secciones2 { get; set; }


        public IList<ItemDTO> fillHoras()
        {
            var lista = new List<ItemDTO>();
            //lista.Insert(0, new ItemDTO() { id = -1, name = "" });
            lista.Insert(0, new ItemDTO() { id = 0, name = "00" });
            lista.Insert(1, new ItemDTO() { id = 1, name = "01" });
            lista.Insert(2, new ItemDTO() { id = 2, name = "02" });
            lista.Insert(3, new ItemDTO() { id = 3, name = "03" });
            lista.Insert(4, new ItemDTO() { id = 4, name = "04" });
            lista.Insert(5, new ItemDTO() { id = 5, name = "05" });
            lista.Insert(6, new ItemDTO() { id = 6, name = "06" });
            lista.Insert(7, new ItemDTO() { id = 7, name = "07" });
            lista.Insert(8, new ItemDTO() { id = 8, name = "08" });
            lista.Insert(9, new ItemDTO() { id = 9, name = "09" });
            lista.Insert(10, new ItemDTO() { id = 10, name = "10" });
            lista.Insert(11, new ItemDTO() { id = 11, name = "11" });
            lista.Insert(12, new ItemDTO() { id = 12, name = "12" });
            lista.Insert(13, new ItemDTO() { id = 13, name = "13" });
            lista.Insert(14, new ItemDTO() { id = 14, name = "14" });
            lista.Insert(15, new ItemDTO() { id = 15, name = "15" });
            lista.Insert(16, new ItemDTO() { id = 16, name = "16" });
            lista.Insert(17, new ItemDTO() { id = 17, name = "17" });
            lista.Insert(18, new ItemDTO() { id = 18, name = "18" });
            lista.Insert(19, new ItemDTO() { id = 19, name = "19" });
            lista.Insert(20, new ItemDTO() { id = 20, name = "20" });
            lista.Insert(21, new ItemDTO() { id = 21, name = "21" });
            lista.Insert(22, new ItemDTO() { id = 22, name = "22" });
            lista.Insert(23, new ItemDTO() { id = 23, name = "23" });
            return lista;
        }
        public IList<ItemDTO> fillMinutos()
        {
            var lista = new List<ItemDTO>();
            //lista.Insert(0, new ItemDTO() { id = -1, name = "" });
            lista.Insert(0, new ItemDTO() { id = 0, name = "00" });
            lista.Insert(1, new ItemDTO() { id = 5, name = "05" });
            lista.Insert(2, new ItemDTO() { id = 10, name = "10" });
            lista.Insert(3, new ItemDTO() { id = 15, name = "15" });
            lista.Insert(4, new ItemDTO() { id = 20, name = "20" });
            lista.Insert(5, new ItemDTO() { id = 25, name = "25" });
            lista.Insert(6, new ItemDTO() { id = 30, name = "30" });
            lista.Insert(7, new ItemDTO() { id = 35, name = "35" });
            lista.Insert(8, new ItemDTO() { id = 40, name = "40" });
            lista.Insert(9, new ItemDTO() { id = 45, name = "45" });
            lista.Insert(10, new ItemDTO() { id = 50, name = "50" });
            lista.Insert(11, new ItemDTO() { id = 55, name = "55" });
            return lista;
        }
    }

}
