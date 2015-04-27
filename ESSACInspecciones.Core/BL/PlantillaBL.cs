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
    public class PlantillaBL : Base
    {
        public IList<PlantillaDTO> getPlantillas()
        {
            using (var context = getContext())
            {
                var result = from r in context.Plantilla
                             where r.Active == true
                             select new PlantillaDTO
                             {
                                 IdPlantilla = r.IdPlantilla,
                                 Nombre = r.Nombre,
                                 Nombre2 = r.Nombre2,
                                 Active = r.Active
                             };
                return result.AsEnumerable<PlantillaDTO>().OrderBy(x => x.IdPlantilla).ToList<PlantillaDTO>();
            }
        }

        public PlantillaDTO getPlantilla(int id)
        {
            using(var context = getContext())
            {
                var result = from r in context.Plantilla
                             where r.IdPlantilla == id
                             select new PlantillaDTO
                             {
                                 IdPlantilla = r.IdPlantilla,
                                 Nombre = r.Nombre,
                                 Nombre2 = r.Nombre2,
                                 Active = r.Active
                             };
                return result.SingleOrDefault();
            }
        }
    }
}
