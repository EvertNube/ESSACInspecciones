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
    public class ServiciosBL: Base
    {
        public IList<ServicioDTO> getServicios(bool activeOnly = false) {
            using (var context = getContext())
            {
                var result = !activeOnly ? from r in context.Servicio
                                           select new ServicioDTO
                                           {
                                               IdServicio = r.IdServicio,
                                               NombreServicio = r.Nombre,
                                               NombreCorto = r.NombreCorto,
                                               Descripcion = r.Descripcion,
                                               Active = r.Active
                                           } : from r in context.Servicio
                                               where r.Active == true
                                               select new ServicioDTO
                                               {
                                                   IdServicio = r.IdServicio,
                                                   NombreServicio = r.Nombre,
                                                   NombreCorto = r.NombreCorto,
                                                   Descripcion = r.Descripcion,
                                                   Active = r.Active
                                               };

                return result.ToList<ServicioDTO>();
            }
        }

        public bool add(ServicioDTO servicioDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    Servicio servicio = new Servicio();
                    servicio.Nombre = servicioDTO.NombreServicio;
                    servicio.NombreCorto = servicioDTO.NombreCorto;
                    servicio.Descripcion = servicioDTO.Descripcion;
                    servicio.Color = servicioDTO.ColorServicio;
                    servicio.Active = true;
                    context.Servicio.Add(servicio);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                    //return false;
                }
            }
        }

        public bool update(ServicioDTO servicioDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    var servicio = context.Servicio.Where(x => x.IdServicio == servicioDTO.IdServicio).SingleOrDefault();
                    servicio.Nombre = servicioDTO.NombreServicio;
                    servicio.NombreCorto = servicioDTO.NombreCorto;
                    servicio.Descripcion = servicioDTO.Descripcion;
                    servicio.Color = servicioDTO.ColorServicio;
                    servicio.Active = servicioDTO.Active;
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    //throw e;
                    return false;
                }
            }
        }

        public ServicioDTO getServicio(int id)
        {
            using (var context = getContext())
            {
                var result = from r in context.Servicio.AsEnumerable()
                             where r.IdServicio == id
                             select new ServicioDTO
                             {
                                 IdServicio = r.IdServicio,
                                 NombreServicio = r.Nombre,
                                 NombreCorto = r.NombreCorto,
                                 Descripcion = r.Descripcion,
                                 ColorServicio = r.Color,
                                 Active = r.Active
                             };
                return result.SingleOrDefault();
            }
        }

        public IList<ServicioDTO> searchServicios(string busqueda)
        {
            using (var context = getContext())
            {
                return (from r in context.Servicio
                        where (r.Nombre.Contains(busqueda) | r.NombreCorto.Contains(busqueda) | r.Descripcion.Contains(busqueda)) & r.Active == true
                        select new ServicioDTO
                        {
                            IdServicio = r.IdServicio,
                            NombreServicio = r.Nombre,
                            NombreCorto = r.NombreCorto,
                            Descripcion = r.Descripcion
                        }).ToList();
            }
        }
    }
}
