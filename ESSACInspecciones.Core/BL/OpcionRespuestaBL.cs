using ESSACInspecciones.Core.DTO;
using ESSACInspecciones.Data;
using ESSACInspecciones.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.BL
{
    public class OpcionRespuestaBL : Base
    {
        public IList getOpcionRespuesta(int IdTipoRespuesta)
        {
            using (var context = getContext())
            {
                var result = context.OpcionRespuesta.Where(x => x.IdTipoRespuesta == IdTipoRespuesta).Select(x => new OpcionDTO { IdOpcion = x.IdOpcion, NombreOpcion = x.Nombre, Puntaje = x.Puntaje }).ToList();
                result.Insert(0, new OpcionDTO() { IdOpcion = 0, NombreOpcion = "Seleccione" });
                return result;
            }
        }
    }
}
