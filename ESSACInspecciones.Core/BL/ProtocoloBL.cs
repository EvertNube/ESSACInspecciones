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
    public class ProtocoloBL : Base
    {
        #region CRUD Tarea
        public ProtocoloDTO getInfoProtocolo(int idInmueble, int? idProtocolo, int? idPlantilla)
        {
            using (var context = getContext())
            {
                ProtocoloDTO result = new ProtocoloDTO();
                if (idProtocolo != null && idProtocolo != 0)
                {
                    result = (context.Protocolo.Where(x => x.IdProtocolo == idProtocolo).AsEnumerable()
                    .Select(r => new ProtocoloDTO
                    {
                        IdProtocolo = r.IdProtocolo,
                        IdPlantilla = r.IdPlantilla,
                        IdInmueble = r.IdInmueble,
                        NombreAreaProtegida = r.NombreAreaProtegida,
                        Fecha = r.Fecha,
                        HoraInicio = Convert.ToInt32(r.Fecha.Value.ToString("HH")),
                        MinutoInicio = Convert.ToInt32(r.Fecha.Value.ToString("mm")),
                        Active = r.Active,
                        TotalPaginas = r.Plantilla.Seccion.GroupBy(x => x.Pagina).Count(),
                        Plantilla = new PlantillaDTO { Nombre = r.Plantilla.Nombre },
                    })).SingleOrDefault();
                }
                return result;
            }
        }

        public List<ProtocoloDTO> getProtocolos(int idUsuario, int idInmueble)
        {
            using (var context = getContext())
            {
                var result = context.SP_GetPlantillas(idUsuario, idInmueble)
                    .Select(x => new ProtocoloDTO
                    {
                        IdProtocolo = x.IdProtocolo,
                        IdPlantilla = x.IdPlantilla,
                        IdInmueble = x.IdInmueble,
                        IdEstado = x.IdEstado,
                        Plantilla = new PlantillaDTO { Nombre = x.Nombre },
                        Estado = new EstadoDTO { NombreEstado = x.NombreEstado },
                        Active = x.Active
                    }).ToList();
                return result;
            }
        }

        public ProtocoloDTO getProtocolo(int idInmueble, int? idProtocolo, int? idPlantilla)
        {
            using (var context = getContext())
            {
                ProtocoloDTO result = new ProtocoloDTO();
                
                if (idProtocolo != null && idProtocolo != 0)
                {
                    result = (context.Protocolo.Where(x => x.IdProtocolo == idProtocolo).AsEnumerable()
                    .Select(r => new ProtocoloDTO
                    {
                        IdProtocolo = r.IdProtocolo,
                        IdPlantilla = r.IdPlantilla,
                        IdInmueble = r.IdInmueble,
                        IdEstado = r.IdEstado,
                        NombreAreaProtegida = r.NombreAreaProtegida,
                        Fecha = r.Fecha,
                        HoraInicio = r.Fecha != null ? Convert.ToInt32(r.Fecha.Value.ToString("HH")) : 0,
                        MinutoInicio = r.Fecha != null ? Convert.ToInt32(r.Fecha.Value.ToString("mm")) : 0,
                        Active = r.Active,
                        TotalPaginas = r.Plantilla.Seccion.GroupBy(x => x.Pagina).Count(),
                        Plantilla = new PlantillaDTO { Nombre = r.Plantilla.Nombre },
                        Secciones = r.Plantilla.Seccion.Where(y => y.IdSeccionPadre == null).Select(y => new SeccionDTO
                        {
                            IdSeccion = y.IdSeccion,
                            Nombre = y.Nombre,
                            Pagina = y.Pagina,
                            Orden = y.Orden,
                            SubSecciones = r.Plantilla.Seccion.Where(z => z.IdSeccionPadre == y.IdSeccion).Select(z => new SeccionDTO
                            {
                                IdSeccion = z.IdSeccion,
                                Nombre = z.Nombre,
                                Orden = z.Orden,
                                SeccionBodys = z.SeccionBody.Select(w => new SeccionBodyDTO
                                {
                                    IdSeccionBody = w.IdSeccionBody,
                                    Descripcion = w.Descripcion,
                                    Rowspan = w.Rowspan,
                                    Colspan = w.Colspan,
                                    BackgroundColor = w.BackgroundColor,
                                    NumeroFila = w.NumeroFila,
                                    IdTipoCelda = w.IdTipoCelda,
                                    IdTipoTag = w.IdTipoTag ?? 0,
                                    Orden = w.Orden,
                                    Respuesta = r.Respuesta.Where(a => a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).SingleOrDefault()
                                    //Respuesta = context.Respuesta.Where(a => a.IdProtocolo == IdProtocoloAnterior && a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).SingleOrDefault()
                                }).OrderBy(w => w.Orden).ToList()
                            }).OrderBy(z => z.Orden).ToList(),
                            SeccionBodys = y.SeccionBody.Select(w => new SeccionBodyDTO
                            {
                                IdSeccionBody = w.IdSeccionBody,
                                Descripcion = w.Descripcion,
                                Rowspan = w.Rowspan,
                                Colspan = w.Colspan,
                                BackgroundColor = w.BackgroundColor,
                                NumeroFila = w.NumeroFila,
                                IdTipoCelda = w.IdTipoCelda,
                                IdTipoTag = w.IdTipoTag ?? 0,
                                Orden = w.Orden,
                                Respuesta = r.Respuesta.Where(a => a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).SingleOrDefault()
                                //Respuesta = context.Respuesta.Where(a => a.IdProtocolo == IdProtocoloAnterior && a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).SingleOrDefault()
                            }).OrderBy(w => w.Orden).ToList()
                        }).OrderBy(y => y.Orden).ToList(),
                        ////Respuestas = r.Respuesta.Select(z => new RespuestaDTO { IdSeccionBody = z.IdSeccionBody, Respuesta = z.Descripcion }).ToList()
                    })).SingleOrDefault();
                }
                else
                {
                    var IdProtocoloAnterior = context.Protocolo.Where(p => p.IdPlantilla == idPlantilla && p.IdInmueble == idInmueble).OrderByDescending(p => p.FechaCreacion).FirstOrDefault().IdProtocolo;
                    result = context.Plantilla.Where(x => x.IdPlantilla == idPlantilla)
                        .Select(x => new ProtocoloDTO
                        {
                            IdProtocolo = 0,
                            IdPlantilla = x.IdPlantilla,
                            IdInmueble = idInmueble,//
                            IdEstado = 0,
                            NombreAreaProtegida = null,
                            Fecha = null,
                            HoraInicio = 0,
                            MinutoInicio = 0,
                            Active = true,
                            TotalPaginas = x.Seccion.GroupBy(t => t.Pagina).Count(),
                            Plantilla = new PlantillaDTO { Nombre = x.Nombre },
                        }).SingleOrDefault();
                    result.Secciones = context.Seccion.Where(y => y.IdPlantilla == idPlantilla && y.IdSeccionPadre == null)
                        .Select(y => new SeccionDTO
                        {
                            IdSeccion = y.IdSeccion,
                            Nombre = y.Nombre,
                            Pagina = y.Pagina,
                            Orden = y.Orden
                        }).OrderBy(y => y.Orden).ToList();
                    foreach (var seccion in result.Secciones)
                    {
                        seccion.SubSecciones = context.Seccion.Where(z => z.IdSeccionPadre == seccion.IdSeccion)
                                        .Select(z => new SeccionDTO
                                        {
                                            IdSeccion = z.IdSeccion,
                                            Nombre = z.Nombre,
                                            Orden = z.Orden
                                        }).OrderBy(z => z.Orden).ToList();
                        seccion.SeccionBodys = context.SeccionBody.Where(w => w.IdSeccion == seccion.IdSeccion)
                                        .Select(w => new SeccionBodyDTO
                                        {
                                            IdSeccionBody = w.IdSeccionBody,
                                            Descripcion = w.Descripcion,
                                            Rowspan = w.Rowspan,
                                            Colspan = w.Colspan,
                                            BackgroundColor = w.BackgroundColor,
                                            NumeroFila = w.NumeroFila,
                                            IdTipoCelda = w.IdTipoCelda,
                                            IdTipoTag = w.IdTipoTag ?? 0,
                                            Orden = w.Orden,
                                            Respuesta = context.Respuesta.Where(a => a.IdProtocolo == IdProtocoloAnterior && a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).FirstOrDefault()
                                        }).OrderBy(w => w.Orden).ToList();
                    }
                    foreach (var seccion in result.Secciones)
                    {
                        foreach (var subseccion in seccion.SubSecciones)
                        {
                            subseccion.SeccionBodys = context.SeccionBody.Where(w => w.IdSeccion == subseccion.IdSeccion)
                                        .Select(w => new SeccionBodyDTO
                                        {
                                            IdSeccionBody = w.IdSeccionBody,
                                            Descripcion = w.Descripcion,
                                            Rowspan = w.Rowspan,
                                            Colspan = w.Colspan,
                                            BackgroundColor = w.BackgroundColor,
                                            NumeroFila = w.NumeroFila,
                                            IdTipoCelda = w.IdTipoCelda,
                                            IdTipoTag = w.IdTipoTag ?? 0,
                                            Orden = w.Orden,
                                            Respuesta = context.Respuesta.Where(a => a.IdProtocolo == IdProtocoloAnterior && a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).FirstOrDefault()
                                        }).OrderBy(w => w.Orden).ToList();
                        }
                    }

                }

                return result;
            }
        }

        //public ProtocoloDTO getPlantilla_PP053(int id)
        //{
        //    using (var context = getContext())
        //    {

        //        var result = (context.Protocolo.Where(x => x.IdProtocolo == id).AsEnumerable()
        //            .Select(r => new ProtocoloDTO
        //            {
        //                IdProtocolo = r.IdProtocolo,
        //                IdPlantilla = r.IdPlantilla,
        //                NombreAreaProtegida = r.NombreAreaProtegida,
        //                Fecha = r.Fecha,
        //                HoraInicio = Convert.ToInt32(r.Fecha.Value.ToString("HH")),
        //                MinutoInicio = Convert.ToInt32(r.Fecha.Value.ToString("mm")),
        //                Active = r.Active,
        //                TotalPaginas = r.Plantilla.Seccion.GroupBy(x => x.Pagina).Count(),
        //                Secciones = r.Plantilla.Seccion.Where(y => y.IdSeccionPadre == null).Select(y => new SeccionDTO
        //                {
        //                    IdSeccion = y.IdSeccion,
        //                    Nombre = y.Nombre,
        //                    Pagina = y.Pagina,
        //                    Orden = y.Orden,
        //                    SubSecciones = r.Plantilla.Seccion.Where(z => z.IdSeccionPadre == y.IdSeccion).Select(z => new SeccionDTO
        //                    {
        //                        IdSeccion = z.IdSeccion,
        //                        Nombre = z.Nombre,
        //                        Orden = z.Orden,
        //                        SeccionBodys = z.SeccionBody.Select(w => new SeccionBodyDTO
        //                        {
        //                            IdSeccionBody = w.IdSeccionBody,
        //                            Descripcion = w.Descripcion,
        //                            Rowspan = w.Rowspan,
        //                            Colspan = w.Colspan,
        //                            BackgroundColor = w.BackgroundColor,
        //                            NumeroFila = w.NumeroFila,
        //                            IdTipoCelda = w.IdTipoCelda,
        //                            IdTipoTag = w.IdTipoTag ?? 0,
        //                            Orden = w.Orden,
        //                            Respuesta = r.Respuesta.Where(a => a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).SingleOrDefault()
        //                        }).OrderBy(w => w.Orden).ToList()
        //                    }).OrderBy(z => z.Orden).ToList(),
        //                    SeccionBodys = y.SeccionBody.Select(w => new SeccionBodyDTO
        //                    {
        //                        IdSeccionBody = w.IdSeccionBody,
        //                        Descripcion = w.Descripcion,
        //                        Rowspan = w.Rowspan,
        //                        Colspan = w.Colspan,
        //                        BackgroundColor = w.BackgroundColor,
        //                        NumeroFila = w.NumeroFila,
        //                        IdTipoCelda = w.IdTipoCelda,
        //                        IdTipoTag = w.IdTipoTag ?? 0,
        //                        Orden = w.Orden
        //                    }).OrderBy(w => w.Orden).ToList()
        //                }).OrderBy(y => y.Orden).ToList(),
        //                //Respuestas = r.Respuesta.Select(z => new RespuestaDTO { IdSeccionBody = z.IdSeccionBody, Respuesta = z.Descripcion }).ToList()
        //            })).SingleOrDefault();
        //        return result;
        //    }
        //}

        public bool add(ProtocoloDTO oProtocoloDTO)
        {

            using (var context = getContext())
            {
                try
                {
                    int contaEstado = oProtocoloDTO.GroupDescripcion.Where(x => x == null || x == "0").FirstOrDefault().Count();
                    Protocolo protocolo = new Protocolo();
                    protocolo.IdPlantilla = oProtocoloDTO.IdPlantilla;
                    protocolo.IdInmueble = oProtocoloDTO.IdInmueble;
                    protocolo.IdUsuario = oProtocoloDTO.IdUsuario;
                    protocolo.IdEstado = (oProtocoloDTO.IdEstado != 0 ? oProtocoloDTO.IdEstado : (contaEstado != 0 ? 2 : 3));    // 2: Incompleto, 3: Completo, 4: Finalizado
                    protocolo.NombreAreaProtegida = oProtocoloDTO.NombreAreaProtegida;
                    protocolo.Direccion = oProtocoloDTO.Direccion;
                    protocolo.Fecha = (oProtocoloDTO.Fecha != null ? Convert.ToDateTime(oProtocoloDTO.Fecha.Value.ToString("dd/MM/yyyy") + " " + oProtocoloDTO.HoraInicio + ":" + oProtocoloDTO.MinutoInicio) : oProtocoloDTO.Fecha);
                    protocolo.Active = true;
                    context.Protocolo.Add(protocolo);

                    for (int i = 0; i < oProtocoloDTO.GroupIdTableBody.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) && oProtocoloDTO.GroupDescripcion[i] != "0")
                        {
                            Respuesta respuesta = new Respuesta();
                            //respuesta.IdProtocolo = protocolo.IdProtocolo;
                            respuesta.IdSeccionBody = oProtocoloDTO.GroupIdTableBody[i];
                            respuesta.Descripcion = oProtocoloDTO.GroupDescripcion[i];
                            respuesta.Active = true;
                            protocolo.Respuesta.Add(respuesta);
                        }
                    }
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

        //public bool add_Respuesta(ProtocoloDTO oProtocoloDTO)
        //{
        //    using (var context = getContext())
        //    {
        //        try
        //        {
        //            var protocolo = context.Protocolo.Where(x => x.IdProtocolo == oProtocoloDTO.IdProtocolo).SingleOrDefault();
        //            for (int i = 0; i < oProtocoloDTO.GroupIdTableBody.Count; i++)
        //            {
        //                if (string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) && oProtocoloDTO.GroupDescripcion[i] != "0" )
        //                {
        //                    Respuesta respuesta = new Respuesta();
        //                    respuesta.IdProtocolo = protocolo.IdProtocolo;
        //                    respuesta.IdSeccionBody = oProtocoloDTO.GroupIdTableBody[i];
        //                    respuesta.Descripcion = oProtocoloDTO.GroupDescripcion[i];
        //                    respuesta.Active = true;
        //                    context.Respuesta.Add(respuesta);
        //                }
        //            }
        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            //throw e;
        //            return false;
        //        }
        //    }
        //}

        public bool update(ProtocoloDTO oProtocoloDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    int contaEstado = oProtocoloDTO.GroupDescripcion.Where(x => x == null || x == "0").FirstOrDefault().Count();
                    var protocolo = context.Protocolo.Where(x => x.IdProtocolo == oProtocoloDTO.IdProtocolo).SingleOrDefault();
                    protocolo.IdEstado = (oProtocoloDTO.IdEstado != 0 ? oProtocoloDTO.IdEstado : (contaEstado != 0 ? 2 : 3));    // 2: Incompleto, 3: Completo, 4: Finalizado
                    protocolo.NombreAreaProtegida = oProtocoloDTO.NombreAreaProtegida;
                    protocolo.Direccion = oProtocoloDTO.Direccion;
                    protocolo.Fecha = (oProtocoloDTO.Fecha != null ? Convert.ToDateTime(oProtocoloDTO.Fecha.Value.ToString("dd/MM/yyyy") + " " + oProtocoloDTO.HoraInicio + ":" + oProtocoloDTO.MinutoInicio) : oProtocoloDTO.Fecha);
                    protocolo.Active = oProtocoloDTO.Active;

                    var oldRespuesta = protocolo.Respuesta;
                    for (int i = 0; i < oProtocoloDTO.GroupIdTableBody.Count; i++)
                    {
                        var respuesta = oldRespuesta.Where(x => x.IdSeccionBody == oProtocoloDTO.GroupIdTableBody[i]).SingleOrDefault();
                        if (respuesta != null)
                        {
                            if (string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) || oProtocoloDTO.GroupDescripcion[i] == "0")
                            {
                                //Elimino
                                //var respuestaToRemove = oldRespuesta.Where(x => x.IdSeccionBody == oProtocoloDTO.GroupIdTableBody[i]).SingleOrDefault();
                                protocolo.Respuesta.Remove(respuesta);
                            }
                            else if (oProtocoloDTO.GroupDescripcion[i].Trim() != respuesta.Descripcion)
                            {
                                //Edito
                                respuesta.Descripcion = oProtocoloDTO.GroupDescripcion[i].Trim();
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) && oProtocoloDTO.GroupDescripcion[i] != "0")
                            {
                                Respuesta respuestaToAdd = new Respuesta();
                                respuestaToAdd.IdSeccionBody = oProtocoloDTO.GroupIdTableBody[i];
                                respuestaToAdd.Descripcion = oProtocoloDTO.GroupDescripcion[i];
                                respuestaToAdd.Active = true;
                                protocolo.Respuesta.Add(respuestaToAdd);
                            }
                        }
                    }
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
        #endregion
        #region
        public IList<ClienteDTO> getClientesTarea(int idUsuario)//2
        {
            using (var context = getContext())
            {
                var lista = (from r in context.Tarea
                             from u in context.Usuario
                             where u.IdUsuario == idUsuario && r.IdEstado != 5 && r.Active == true
                             select new ClienteDTO
                             {
                                 IdCliente = r.IdCliente,
                                 NombreEmpresa = r.Cliente.Nombre
                             }).Distinct().ToList();
                lista.Insert(0, new ClienteDTO() { IdCliente = 0, NombreEmpresa = "Seleccione" });
                return lista;
                //return (from r in context.Tarea
                //        from u in context.Usuario
                //        where u.IdUsuario == idUsuario && r.IdEstado != 5 && r.Active == true
                //        select new TareaDTO
                //        {
                //            IdCliente = r.IdCliente,
                //            Cliente = new ClienteDTO { NombreEmpresa = r.Cliente.Nombre },
                //            IdInmueble = r.IdInmueble,
                //            Inmueble = new InmuebleDTO { NombreInmueble = r.Inmueble.Nombre }
                //        }).Distinct().ToList();
            }
        }
        #endregion

    }
}
