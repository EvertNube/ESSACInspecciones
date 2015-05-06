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
                        Codigo = r.Codigo,
                        NombreAreaProtegida = r.NombreAreaProtegida,
                        Fecha = r.Fecha,
                        Direccion = r.Direccion,
                        TipoPrueba = r.TipoPrueba,
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

        public List<ProtocoloDTO> getProtocolos(int idUsuario, int idInmueble, int idPeriodo)
        {
            using (var context = getContext())
            {
                var result = context.SP_GetPlantillas2(idUsuario, idInmueble, idPeriodo)
                    .Select(x => new ProtocoloDTO
                    {
                        IdProtocolo = x.IdProtocolo,
                        IdPlantilla = x.IdPlantilla,
                        IdInmueble = x.IdInmueble.GetValueOrDefault(),
                        IdEstado = x.IdEstado,
                        Codigo = x.Codigo,
                        Plantilla = new PlantillaDTO { Nombre = x.Nombre },
                        Estado = new EstadoDTO { NombreEstado = x.NombreEstado },
                        Active = x.Active,
                        IdPeriodo = x.IdPeriodo.GetValueOrDefault()
                    }).OrderBy(y => y.IdPlantilla).ToList();
                return result;
            }
        }

        public ProtocoloDTO getProtocolo(int idInmueble, int idPeriodo, int? idProtocolo, int? idPlantilla)
        {
            using (var context = getContext())
            {
                ProtocoloDTO result = new ProtocoloDTO();
                
                if (idProtocolo != null && idProtocolo != 0)
                {
                    result = context.Protocolo.Where(x => x.IdProtocolo == idProtocolo).AsEnumerable()
                    .Select(r => new ProtocoloDTO
                    {
                        IdProtocolo = r.IdProtocolo,
                        IdPlantilla = r.IdPlantilla,
                        IdInmueble = r.IdInmueble,
                        IdEstado = r.IdEstado,
                        Codigo = r.Codigo,
                        NombreAreaProtegida = r.NombreAreaProtegida,
                        Fecha = r.Fecha,
                        HoraInicio = r.Fecha != null ? Convert.ToInt32(r.Fecha.Value.ToString("HH")) : 0,
                        MinutoInicio = r.Fecha != null ? Convert.ToInt32(r.Fecha.Value.ToString("mm")) : 0,
                        Direccion = r.Direccion,
                        TipoPrueba = r.TipoPrueba,
                        Active = r.Active,
                        IdPeriodo = r.IdPeriodo,
                        TotalPaginas = r.Plantilla.Seccion.GroupBy(x => x.Pagina).Count(),
                        Plantilla = new PlantillaDTO { Nombre = r.Plantilla.Nombre, Nombre2 = r.Plantilla.Nombre2 },
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
                    }).SingleOrDefault();
                }
                else
                {
                    //var IdProtocoloAnterior = context.Protocolo.Where(p => p.IdPlantilla == idPlantilla && p.IdInmueble == idInmueble).OrderByDescending(p => p.FechaCreacion).FirstOrDefault().IdProtocolo;
                    var DefaultValues = context.SP_GetDefaultValues(idPlantilla, idInmueble).Select(x => new DefaultValueDTO { IdSeccionBody = x.IdSeccionBody, Descripcion = x.Descripcion }).ToList();
                    result = context.Plantilla.Where(x => x.IdPlantilla == idPlantilla)
                        .Select(x => new ProtocoloDTO
                        {
                            IdProtocolo = 0,
                            IdPlantilla = x.IdPlantilla,
                            IdInmueble = idInmueble,//
                            IdEstado = 0,
                            Codigo = null,
                            NombreAreaProtegida = null,
                            Fecha = null,
                            HoraInicio = 0,
                            MinutoInicio = 0,
                            Active = true,
                            IdPeriodo = idPeriodo,
                            TotalPaginas = x.Seccion.GroupBy(t => t.Pagina).Count(),
                            Plantilla = new PlantillaDTO { Nombre = x.Nombre, Nombre2 = x.Nombre2 },
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
                        seccion.SeccionBodys = context.SeccionBody.AsEnumerable().Where(w => w.IdSeccion == seccion.IdSeccion)
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
                                            Respuesta = DefaultValues.Where(a => a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).FirstOrDefault() //context.Respuesta.Where(a => a.IdProtocolo == IdProtocoloAnterior && a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).FirstOrDefault()
                                        }).OrderBy(w => w.Orden).ToList();
                    }
                    foreach (var seccion in result.Secciones)
                    {
                        foreach (var subseccion in seccion.SubSecciones)
                        {
                            subseccion.SeccionBodys = context.SeccionBody.AsEnumerable().Where(w => w.IdSeccion == subseccion.IdSeccion)
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
                                            Respuesta = DefaultValues.Where(a => a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).FirstOrDefault()//context.Respuesta.Where(a => a.IdProtocolo == IdProtocoloAnterior && a.IdSeccionBody == w.IdSeccionBody).Select(a => a.Descripcion).FirstOrDefault()
                                        }).OrderBy(w => w.Orden).ToList();
                        }
                    }

                }

                return result;
            }
        }

        public bool add(ProtocoloDTO oProtocoloDTO)
        {

            using (var context = getContext())
            {
                try
                {
                    //int contaEstado = oProtocoloDTO.GroupDescripcion.Where(x => x == null || x == "0").FirstOrDefault().Count(); --> Validacion cuando los 0 no eran permitidos
                    int contaEstado = EstadoProtocolo(oProtocoloDTO.GroupDescripcion);
                    Protocolo protocolo = new Protocolo();
                    protocolo.IdPlantilla = oProtocoloDTO.IdPlantilla;
                    protocolo.IdInmueble = oProtocoloDTO.IdInmueble;
                    protocolo.IdPeriodo = oProtocoloDTO.IdPeriodo;
                    //protocolo.IdUsuario = oProtocoloDTO.IdUsuario;
                    protocolo.IdEstado = (oProtocoloDTO.IdEstado != 0 ? oProtocoloDTO.IdEstado : (contaEstado != 0 ? 2 : 3));    // 2: Incompleto, 3: Completo, 4: Finalizado
                    protocolo.Codigo = oProtocoloDTO.Codigo;
                    protocolo.NombreAreaProtegida = oProtocoloDTO.NombreAreaProtegida;
                    protocolo.Direccion = oProtocoloDTO.Direccion;
                    protocolo.TipoPrueba = oProtocoloDTO.TipoPrueba;
                    protocolo.FechaCreacion = DateTime.Now;
                    protocolo.Fecha = (oProtocoloDTO.Fecha != null ? Convert.ToDateTime(oProtocoloDTO.Fecha.Value.ToString("dd/MM/yyyy") + " " + oProtocoloDTO.HoraInicio + ":" + oProtocoloDTO.MinutoInicio) : oProtocoloDTO.Fecha);
                    protocolo.Active = true;
                    context.Protocolo.Add(protocolo);

                    for (int i = 0; i < oProtocoloDTO.GroupIdTableBody.Count; i++)
                    {
                        //&& oProtocoloDTO.GroupDescripcion[i] != "0"
                        if (!string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) && oProtocoloDTO.GroupDescripcion[i] != "-1")
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

        public bool update(ProtocoloDTO oProtocoloDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    //int contaEstado = oProtocoloDTO.GroupDescripcion.Where(x => x == null || x == "0").FirstOrDefault().Count();
                    int contaEstado = EstadoProtocolo(oProtocoloDTO.GroupDescripcion);
                    var protocolo = context.Protocolo.Where(x => x.IdProtocolo == oProtocoloDTO.IdProtocolo).SingleOrDefault();
                    protocolo.IdEstado = (oProtocoloDTO.IdEstado != 0 ? oProtocoloDTO.IdEstado : (contaEstado != 0 ? 2 : 3));    // 2: Incompleto, 3: Completo, 4: Finalizado
                    protocolo.Codigo = oProtocoloDTO.Codigo;
                    protocolo.NombreAreaProtegida = oProtocoloDTO.NombreAreaProtegida;
                    protocolo.Direccion = oProtocoloDTO.Direccion;
                    protocolo.TipoPrueba = oProtocoloDTO.TipoPrueba;
                    protocolo.Fecha = (oProtocoloDTO.Fecha != null ? Convert.ToDateTime(oProtocoloDTO.Fecha.Value.ToString("dd/MM/yyyy") + " " + oProtocoloDTO.HoraInicio + ":" + oProtocoloDTO.MinutoInicio) : oProtocoloDTO.Fecha);
                    protocolo.Active = oProtocoloDTO.Active;

                    var oldRespuesta = protocolo.Respuesta;
                    for (int i = 0; i < oProtocoloDTO.GroupIdTableBody.Count; i++)
                    {
                        var respuesta = oldRespuesta.Where(x => x.IdSeccionBody == oProtocoloDTO.GroupIdTableBody[i]).SingleOrDefault();
                        if (respuesta != null)
                        {
                            //Esto no permite guardar valores en la caja de texto con Null o 0
                            //if (string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) || oProtocoloDTO.GroupDescripcion[i] == "0")
                            if (string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) || oProtocoloDTO.GroupDescripcion[i] == "-1")
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
                            //&& oProtocoloDTO.GroupDescripcion[i] != "0"
                            if (!string.IsNullOrEmpty(oProtocoloDTO.GroupDescripcion[i].Trim()) && oProtocoloDTO.GroupDescripcion[i] != "-1")
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
            }
        }
        #endregion

        private int EstadoProtocolo(List<string> lista)
        {
            foreach (string item in lista)
            {
                if (string.IsNullOrEmpty(item) || item == "-1")
                    return 1;
            }
            return 0;
        }
    }
}
