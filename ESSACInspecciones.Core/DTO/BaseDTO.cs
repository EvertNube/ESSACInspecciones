using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ESSACInspecciones.Core.DTO
{
    public class BaseDTO
    {
        public IList<OpcionDTO> fillHoras()
        {
            var lista = new List<OpcionDTO>();
            //lista.Insert(0, new OpcionDTO() { IdOpcion = -1, NombreOpcion = "" });
            lista.Insert(0, new OpcionDTO() { IdOpcion = 0, NombreOpcion = "00" });
            lista.Insert(1, new OpcionDTO() { IdOpcion = 1, NombreOpcion = "01" });
            lista.Insert(2, new OpcionDTO() { IdOpcion = 2, NombreOpcion = "02" });
            lista.Insert(3, new OpcionDTO() { IdOpcion = 3, NombreOpcion = "03" });
            lista.Insert(4, new OpcionDTO() { IdOpcion = 4, NombreOpcion = "04" });
            lista.Insert(5, new OpcionDTO() { IdOpcion = 5, NombreOpcion = "05" });
            lista.Insert(6, new OpcionDTO() { IdOpcion = 6, NombreOpcion = "06" });
            lista.Insert(7, new OpcionDTO() { IdOpcion = 7, NombreOpcion = "07" });
            lista.Insert(8, new OpcionDTO() { IdOpcion = 8, NombreOpcion = "08" });
            lista.Insert(9, new OpcionDTO() { IdOpcion = 9, NombreOpcion = "09" });
            lista.Insert(10, new OpcionDTO() { IdOpcion = 10, NombreOpcion = "10" });
            lista.Insert(11, new OpcionDTO() { IdOpcion = 11, NombreOpcion = "11" });
            lista.Insert(12, new OpcionDTO() { IdOpcion = 12, NombreOpcion = "12" });
            lista.Insert(13, new OpcionDTO() { IdOpcion = 13, NombreOpcion = "13" });
            lista.Insert(14, new OpcionDTO() { IdOpcion = 14, NombreOpcion = "14" });
            lista.Insert(15, new OpcionDTO() { IdOpcion = 15, NombreOpcion = "15" });
            lista.Insert(16, new OpcionDTO() { IdOpcion = 16, NombreOpcion = "16" });
            lista.Insert(17, new OpcionDTO() { IdOpcion = 17, NombreOpcion = "17" });
            lista.Insert(18, new OpcionDTO() { IdOpcion = 18, NombreOpcion = "18" });
            lista.Insert(19, new OpcionDTO() { IdOpcion = 19, NombreOpcion = "19" });
            lista.Insert(20, new OpcionDTO() { IdOpcion = 20, NombreOpcion = "20" });
            lista.Insert(21, new OpcionDTO() { IdOpcion = 21, NombreOpcion = "21" });
            lista.Insert(22, new OpcionDTO() { IdOpcion = 22, NombreOpcion = "22" });
            lista.Insert(23, new OpcionDTO() { IdOpcion = 23, NombreOpcion = "23" });
            return lista;
        }
        public IList<OpcionDTO> fillMinutos()
        {
            var lista = new List<OpcionDTO>();
            //lista.Insert(0, new ItemDTO() { id = -1, name = "" });
            //lista.Insert(0, new OpcionDTO() { IdOpcion = -1, NombreOpcion = "" });
            lista.Insert(0, new OpcionDTO() { IdOpcion = 0, NombreOpcion = "00" });
            lista.Insert(1, new OpcionDTO() { IdOpcion = 5, NombreOpcion = "05" });
            lista.Insert(2, new OpcionDTO() { IdOpcion = 10, NombreOpcion = "10" });
            lista.Insert(3, new OpcionDTO() { IdOpcion = 15, NombreOpcion = "15" });
            lista.Insert(4, new OpcionDTO() { IdOpcion = 20, NombreOpcion = "20" });
            lista.Insert(5, new OpcionDTO() { IdOpcion = 25, NombreOpcion = "25" });
            lista.Insert(6, new OpcionDTO() { IdOpcion = 30, NombreOpcion = "30" });
            lista.Insert(7, new OpcionDTO() { IdOpcion = 35, NombreOpcion = "35" });
            lista.Insert(8, new OpcionDTO() { IdOpcion = 40, NombreOpcion = "40" });
            lista.Insert(9, new OpcionDTO() { IdOpcion = 45, NombreOpcion = "45" });
            lista.Insert(10, new OpcionDTO() { IdOpcion = 50, NombreOpcion = "50" });
            lista.Insert(11, new OpcionDTO() { IdOpcion = 55, NombreOpcion = "55" });
            return lista;
        }
        public IList<ItemDTO> fillSelectSINO()
        {
            var lista = new List<ItemDTO>();
            lista.Insert(0, new ItemDTO() { id = 0, name = "Seleccione" });
            lista.Insert(1, new ItemDTO() { id = 1, name = "SI" });
            lista.Insert(2, new ItemDTO() { id = 2, name = "NO" });
            lista.Insert(3, new ItemDTO() { id = 3, name = "N/A" });
            return lista;
        }
        public IList<ItemDTO> fillSelectBomba()
        {
            var lista = new List<ItemDTO>();
            lista.Insert(0, new ItemDTO() { id = 0, name = "Seleccione" });
            lista.Insert(1, new ItemDTO() { id = 1, name = "Horizontal" });
            lista.Insert(2, new ItemDTO() { id = 2, name = "Vertical" });
            lista.Insert(3, new ItemDTO() { id = 3, name = "Sumergible" });
            return lista;
        }
        public IList<ItemDTO> fillSelectNivelTanque()
        {
            var lista = new List<ItemDTO>();
            lista.Insert(0, new ItemDTO() { id = 0, name = "Seleccione" });
            lista.Insert(1, new ItemDTO() { id = 1, name = "Lleno" });
            lista.Insert(2, new ItemDTO() { id = 2, name = "3/4" });
            lista.Insert(3, new ItemDTO() { id = 3, name = "1/2" });
            lista.Insert(4, new ItemDTO() { id = 4, name = "1/4" });
            lista.Insert(5, new ItemDTO() { id = 5, name = "Vacio" });
            lista.Insert(6, new ItemDTO() { id = 6, name = "Otros" });
            return lista;
        }
        public IList<ItemDTO> fillSelectAccesorios()
        {
            var lista = new List<ItemDTO>();
            lista.Insert(0, new ItemDTO() { id = 0, name = "Seleccione" });
            lista.Insert(1, new ItemDTO() { id = 1, name = "Soldados" });
            lista.Insert(2, new ItemDTO() { id = 2, name = "Roscados" });
            lista.Insert(3, new ItemDTO() { id = 3, name = "Ranurados" });
            lista.Insert(4, new ItemDTO() { id = 4, name = "Bridados" });
            return lista;
        }
        public IList<ItemDTO> fillSelectPresiones()
        {
            var lista = new List<ItemDTO>();
            lista.Insert(0, new ItemDTO() { id = 0, name = "Seleccione" });
            lista.Insert(1, new ItemDTO() { id = 1, name = "Registro en discos" });
            lista.Insert(2, new ItemDTO() { id = 2, name = "Registro electrónico" });
            return lista;
        }
    }
}
