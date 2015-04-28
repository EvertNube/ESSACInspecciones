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
    public class TareaBL : Base
    {
        public IList<TareaDTO> searchTareas(string busqueda)
        {
            using (var context = getContext())
            {
                var result = (from r in context.Tarea
                              from z in r.Usuario
                              join x in context.Servicio on r.IdServicio equals x.IdServicio
                              join y in context.Cliente on r.IdCliente equals y.IdCliente
                              join w in context.Inmueble on r.IdInmueble equals w.IdInmueble
                              where ((r.Nombre.Contains(busqueda) | r.Descripcion.Contains(busqueda)) |
                              (x.Nombre.Contains(busqueda) | x.NombreCorto.Contains(busqueda)) |
                              (y.Nombre.Contains(busqueda) | y.Telefono_1.Contains(busqueda) | y.Telefono_2.Contains(busqueda)) |
                              (z.Nombre.Contains(busqueda) | z.InicialesNombre.Contains(busqueda) | z.Email.Contains(busqueda)) |
                              (w.Nombre.Contains(busqueda) | w.Direccion.Contains(busqueda)))
                              select new TareaDTO
                              {
                                  IdTarea = r.IdTarea,
                                  NombreTarea = r.Nombre,
                                  Descripcion = r.Descripcion,
                                  Cliente = new ClienteDTO { NombreEmpresa = y.Nombre, Telefono1 = y.Telefono_1, Telefono2 = y.Telefono_2 },
                                  Inmueble = new InmuebleDTO { NombreInmueble = w.Nombre, Direccion = w.Direccion },
                                  //Responsables = r.Usuario.Select(u => new UsuarioDTO { IdUsuario = u.IdUsuario, Nombre = u.Nombre, InicialesNombre = u.InicialesNombre, Email = u.Email }).ToList(),
                                  Servicio = new ServicioDTO { NombreServicio = x.Nombre, NombreCorto = x.NombreCorto }
                              }).ToList();

                if (result != null)
                {
                    foreach (var tarea in result)
                    {
                        var resp = context.Tarea.Where(x => x.IdTarea == tarea.IdTarea)
                                                .FirstOrDefault().Usuario
                                                .Select(x => new UsuarioDTO { IdUsuario = x.IdUsuario, Nombre = x.Nombre, InicialesNombre = x.InicialesNombre, Email = x.Email }).ToList();
                        tarea.Responsables = resp;
                    }
                    return result;
                }
                return null;

            }
        }
        public IList getBolsaTareas()
        {
            using (var context = getContext())
            {
                return (from r in context.Tarea
                        join x in context.Servicio on r.IdServicio equals x.IdServicio
                        where (r.FechaInicio == null | r.FechaFin == null) & r.Active == true
                        select new
                        {
                            IdTarea = r.IdTarea,
                            NombreTarea = r.Nombre,
                            Descripcion = r.Descripcion,
                            Servicio = new ServicioDTO { NombreServicio = x.Nombre, NombreCorto = x.NombreCorto, ColorServicio = x.Color }
                        }).ToList();
            }
        }

        public IList getTareasResponsables(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var context = getContext())
            {
                var result = context.SP_GetTareasResponsables(fechaInicio, fechaFin).Select(x => new EventDTO { title = x.title, start = x.start.GetValueOrDefault(), end = x.end.GetValueOrDefault(), idTarea = x.idTarea, color = x.color, strResource = x.strResource }).ToList();
                if (result != null)
                    foreach (var item in result)
                        item.resource = item.strResource != null ? item.strResource.Split(',').Select(r => Convert.ToInt32(r)).ToList() : new List<int>();
                return result;
            }
        }

        public IList getTareasResponsables(int[] responsables)
        {
            using (var context = getContext())
            {
                var result = (from r in context.Usuario.AsEnumerable()
                              from a in r.Tarea
                              join b in context.Servicio on a.IdServicio equals b.IdServicio
                              where a.FechaInicio != null & a.FechaFin != null & a.Active == true & responsables.Contains(r.IdUsuario) //c.IdRol == CONSTANTES.ROL_RESPONSABLE
                              select new
                              {
                                  title = a.Nombre,
                                  start = a.FechaInicio,
                                  end = a.FechaFin,
                                  allDay = true,
                                  //resource = a.Usuario.Select(y => y.IdUsuario).ToList(),//['resource1','resource2']
                                  idTarea = a.IdTarea,
                                  idResponsable = r.IdUsuario,
                                  color = "#" + b.Color
                              }).ToList();
                return result;
            }
        }

        public int updateTareaResponsable(int tipoCorreo, TareaDTO oTareaDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    var Tarea = context.Tarea.Where(x => x.IdTarea == oTareaDTO.IdTarea).SingleOrDefault();

                    var horaInicio = Tarea.FechaInicio != null ? Tarea.FechaInicio.Value.Hour : 0;
                    var horaFin = Tarea.FechaFin != null ? Tarea.FechaFin.Value.Hour : 0;
                    var minutoInicio = Tarea.FechaInicio != null ? Tarea.FechaInicio.Value.Minute : 0;
                    var minutoFin = Tarea.FechaFin != null ? Tarea.FechaFin.Value.Minute : 0;

                    Tarea.FechaInicio = (oTareaDTO.FechaInicio != null ? Convert.ToDateTime(oTareaDTO.FechaInicio.Value.ToString("dd/MM/yyyy") + " " + horaInicio + ":" + minutoInicio) : oTareaDTO.FechaInicio);
                    Tarea.FechaFin = (oTareaDTO.FechaFin != null ? Convert.ToDateTime(oTareaDTO.FechaFin.Value.ToString("dd/MM/yyyy") + " " + horaFin + ":" + minutoFin) : oTareaDTO.FechaFin);
                    Tarea.IdEstado = oTareaDTO.Responsables == null ? 1 : 2;//1 : Por Asignar, 2 : Asignado

                    var oldResponsables = Tarea.Usuario.Select(x => x.IdUsuario).ToList();
                    var newResponsables = oTareaDTO.Responsables.Select(x => x.IdUsuario).ToList();
                    var responsablesToRemove = oldResponsables.Except(newResponsables).ToList();
                    var responsablesToAdd = newResponsables.Except(oldResponsables).ToList();

                    foreach (var resp in responsablesToRemove)
                    {
                        var responsable = context.Usuario.Where(x => x.IdUsuario == resp).SingleOrDefault();
                        Tarea.Usuario.Remove(responsable);
                    }
                    foreach (var resp in responsablesToAdd)
                    {
                        var responsable = context.Usuario.Where(x => x.IdUsuario == resp).SingleOrDefault();
                        Tarea.Usuario.Add(responsable);
                    }
                    context.SaveChanges();

                    oTareaDTO.NombreTarea = Tarea.Nombre;
                    oTareaDTO.Descripcion = Tarea.Descripcion;
                    oTareaDTO.FechaInicio = Tarea.FechaInicio;
                    oTareaDTO.FechaFin = Tarea.FechaFin;
                    oTareaDTO.Cliente = context.Cliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ClienteDTO { NombreEmpresa = y.Nombre }).FirstOrDefault();
                    oTareaDTO.Cliente.Contactos = context.ContactoCliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ContactoDTO { Nombre = y.Nombre }).ToList();
                    oTareaDTO.Inmueble = context.Inmueble.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new InmuebleDTO { NombreInmueble = y.Nombre }).FirstOrDefault();
                    oTareaDTO.Responsables = context.Tarea.Where(x => x.IdTarea == Tarea.IdTarea).SingleOrDefault().Usuario.Select(x => new UsuarioDTO { IdUsuario = x.IdUsuario, Nombre = x.Nombre, Email = x.Email }).ToList();
                    oTareaDTO.Servicio = context.Servicio.Where(x => x.IdServicio == Tarea.IdServicio).Select(y => new ServicioDTO { NombreServicio = y.Nombre, NombreCorto = y.NombreCorto }).FirstOrDefault();
                    oTareaDTO.Estado = context.Estado.Where(x => x.IdEstado == Tarea.IdEstado).Select(y => new EstadoDTO { NombreEstado = y.NombreEstado }).FirstOrDefault();

                    SendMailResponsable(oTareaDTO, responsablesToRemove, responsablesToAdd);
                    return Tarea.IdTarea;
                }
                catch (Exception e)
                {
                    //throw e;
                    return 0;
                }
            }
        }
        private void SendMailResponsable(TareaDTO oTareaDTO, List<int> responsablesRemoved, List<int> responsablesAdded)
        {
            /*
             * Evento
             * 1 : Asignado
             * 2 : Modificado -> Debe ser Re-agendado 
             * 3 : Reasignado (Desasignacion + Asignacion)
             * 4 : Desagendado
            */

            //Modificado: Aplica a la modificacion de todos los campos todos los campos de Tarea menos Responsables.
            if (responsablesRemoved.Count == 0 && responsablesAdded.Count == 0)
            {
                enviarCorreo("modificado", oTareaDTO, new List<int>());
            }
            //Nuevo Responsable (Asignacion)
            if (responsablesAdded.Count > 0)
            {
                enviarCorreo("asignado", oTareaDTO, responsablesAdded);
            }
            //Removido Responsable (Desasignacion)
            if (responsablesRemoved.Count > 0)
            {
                enviarCorreo("desasignado", oTareaDTO, responsablesRemoved);
            }

            //UsuariosBL oBL = new UsuariosBL();
            //string to1 = "";
            //int cont = 0;
            //string accion1 = (evento == 2 ? "modificado " : "asignado ");
            //string subject1 = "Se ha " + accion1 + "una tarea: " + oTareaDTO.NombreTarea;
            //string body = "<div>Nombre Tarea : " + oTareaDTO.NombreTarea + " </div>" +
            //    "<div>Descripción : " + oTareaDTO.Descripcion + " </div>" +
            //    "<div>Fecha Inicio : " + oTareaDTO.FechaInicio + " </div>" +
            //    "<div>Fecha Fin : " + oTareaDTO.FechaFin + " </div>" +
            //    "<div>Cliente : " + oTareaDTO.Cliente.NombreEmpresa + " </div>" +
            //    "<div>Inmueble : " + oTareaDTO.Inmueble.NombreInmueble + " </div>" +
            //    "<div>Responsable(s) : ";
            //foreach (var resp in oTareaDTO.Responsables)
            //{
            //    body += "<br/> - " + resp.Nombre;
            //    if (cont == 0) to1 = resp.Email; else to1 += "," + resp.Email;
            //    cont++;
            //}
            //body += " </div>" + "<div>Servicio : " + oTareaDTO.Servicio.NombreServicio + " </div>" + "<div>Estado : " + oTareaDTO.Estado.NombreEstado + " </div>";

            //MailHandler.Send(to1, subject1, body);
            //if (evento == 3)
            //{
            //    string to2 = "";
            //    cont = 0;
            //    foreach (var resp in responsablesRemoved)
            //    {
            //        //UsuarioDTO oOldUser = oBL.getUsuario(resp);
            //        if (cont == 0)
            //        {
            //            to2 += resp.Email;
            //        }
            //        else { to2 += ", " + resp.Email; }
            //        cont++;
            //    }

            //    string subject2 = "Se ha desasignado una tarea: " + oTareaDTO.NombreTarea;
            //    MailHandler.Send(to2, subject2, body);
            //}
        }

        private void enviarCorreo(string accion, TareaDTO oTareaDTO, List<int> toResponsables)
        {
            string to = string.Empty, copy = string.Empty, subject = string.Empty, body = string.Empty;
            subject = "Se ha " + accion + " una tarea: " + oTareaDTO.NombreTarea;
            body = "<div>Nombre Tarea : " + oTareaDTO.NombreTarea + " </div>" +
                "<div>Descripción : " + oTareaDTO.Descripcion + " </div>" +
                "<div>Fecha Inicio : " + oTareaDTO.FechaInicio + " </div>" +
                "<div>Fecha Fin : " + oTareaDTO.FechaFin + " </div>" +
                "<div>Cliente : " + oTareaDTO.Cliente.NombreEmpresa + " </div>" +
                "<div>Contactos : ";

            foreach (var contacto in oTareaDTO.Cliente.Contactos)
            {
                body += "<br/> - " + contacto.Nombre;
            }

            body += "<div>Inmueble : " + oTareaDTO.Inmueble.NombreInmueble + " </div>" +
            "<div>Responsable(s) : ";

            //if (toResponsables != null)
            UsuariosBL oBL = new UsuariosBL();
            foreach (var resp in toResponsables)
            {
                to += oBL.getUsuario(resp).Email + ",";
            }
            foreach (var resp in oTareaDTO.Responsables)
            {
                body += "<br/> - " + resp.Nombre;
                if (toResponsables.Count > 0)//(toResponsables != null)
                {
                    if (toResponsables.IndexOf(resp.IdUsuario) == -1)
                        copy += resp.Email + ",";
                }
                else
                    to += resp.Email + ",";
            }
            to = to.Substring(0, to.Length - 1);
            copy = copy.Substring(0, copy.Length - (copy.Length == 0 ? 0 : 1));
            body += " </div>" + "<div>Servicio : " + oTareaDTO.Servicio.NombreServicio + " </div>" + "<div>Estado : " + oTareaDTO.Estado.NombreEstado + " </div>";
            if(oTareaDTO.Plantilla.IdPlantilla != 0)
                body += "<div>Protocolo : " + oTareaDTO.Plantilla.Nombre + " - " + oTareaDTO.Plantilla.Nombre2 + " </div>";

            MailHandler.Send(to, copy, subject, body);
            //MailHandler.sendEmail(body);
        }

        #region GET
        public IList getResponsables2(bool AsSelectList = false)
        {
            UsuariosBL oBL = new UsuariosBL();
            if (!AsSelectList)
                return oBL.getUsuarios2(CONSTANTES.ROL_RESPONSABLE);
            else
            {
                var lista = oBL.getUsuarios2(CONSTANTES.ROL_RESPONSABLE);
                lista.Insert(0, new UsuarioDTO() { IdUsuario = 0, Nombre = "TODOS" });
                return lista;
            }
        }
        public IList<UsuarioDTO> getResponsables(bool AsSelectList = false)
        {
            UsuariosBL oBL = new UsuariosBL();
            if (!AsSelectList)
                return oBL.getUsuarios(CONSTANTES.ROL_RESPONSABLE);
            else
            {
                var lista = oBL.getUsuarios(CONSTANTES.ROL_RESPONSABLE).OrderBy(x => x.IdUsuario).ToList();
                lista.Insert(0, new UsuarioDTO() { IdUsuario = 0, Nombre = "Seleccione un Responsable" });
                return lista;
            }
        }
        public IList<ServicioDTO> getServicios(bool AsSelectList = false)
        {
            ServiciosBL oBL = new ServiciosBL();
            if (!AsSelectList)
                return oBL.getServicios();
            else
            {
                var lista = oBL.getServicios();
                lista.Insert(0, new ServicioDTO() { IdServicio = 0, NombreServicio = "Seleccione un Servicio" });
                return lista;
            }
        }
        public IList<ClienteDTO> getClientes(bool AsSelectList = false)
        {
            ClienteBL oBL = new ClienteBL();
            if (!AsSelectList)
                return oBL.getClientes();
            else
            {
                var lista = oBL.getClientes();
                lista.Insert(0, new ClienteDTO() { IdCliente = 0, NombreEmpresa = "Seleccione un Cliente" });
                return lista;
            }
        }
        public IList<ClienteDTO> getComboClientes()
        {
            ClienteBL oBL = new ClienteBL();
            var lista = oBL.getComboClientes();
            lista.Insert(0, new ClienteDTO() { IdCliente = 0, NombreEmpresa = "Seleccione un Cliente" });
            return lista;
        }
        public IList<InmuebleDTO> getInmuebles(int IdCliente, bool AsSelectList = false)
        {
            ClienteBL oBL = new ClienteBL();
            if (!AsSelectList)
                return oBL.getInmuebles(IdCliente);
            else
            {
                var lista = oBL.getInmuebles(IdCliente);
                lista.Insert(0, new InmuebleDTO() { IdInmueble = 0, NombreInmueble = "Seleccione un Inmueble" });
                return lista;
            }
        }
        public IList<EstadoDTO> getEstados(int IdServicio, bool AsSelectList = false)
        {
            ClienteBL oBL = new ClienteBL();
            if (!AsSelectList)
                return oBL.getEstados(IdServicio);
            else
            {
                var lista = oBL.getEstados(IdServicio);
                lista.Insert(0, new EstadoDTO() { IdEstado = 0, NombreEstado = "Seleccione un Estado" });
                return lista;
            }
        }
        public IList GetContadorEstados(TareaDTO oTareaDTO)
        {
            using (var context = getContext())
            {
                var result = context.SP_CountEstados(oTareaDTO.FechaInicio, oTareaDTO.FechaFin, oTareaDTO.IdResponsable).Select(x => new { x.NombreEstado, x.CuentaEstado }).ToList();
                return result;
            }
        }
        #endregion

        #region CRUD Tarea
        public IList<TareaDTO> getTareas(bool activeOnly = false)
        {
            using (var context = getContext())
            {
                var result = context.SP_GetTareas().Select(r => new TareaDTO
                {
                    IdTarea = r.IdTarea,
                    NombreTarea = r.NombreTarea,
                    Cliente = new ClienteDTO { NombreEmpresa = r.NombreCliente },
                    Inmueble = new InmuebleDTO { NombreInmueble = r.NombreInmueble },
                    Estado = new EstadoDTO { NombreEstado = r.NombreEstado },
                    FechaInicio = r.FechaInicio,
                    FechaFin = r.FechaFin,
                    Active = r.Active,
                    StrResponsables = r.Responsables
                }).ToList();
                //string[] x;

                UsuariosBL objBL = new UsuariosBL();
                List<UsuarioDTO> listaUsuarios = new List<UsuarioDTO>();
                listaUsuarios = objBL.getUsuariosTodos();

                if (result != null)
                { 
                    foreach (var tarea in result)
                    {
                        tarea.Responsables = tarea.StrResponsables != null ? tarea.StrResponsables.Split(',').Select(r => new UsuarioDTO { IdUsuario = Convert.ToInt32(r) }).ToList() : new List<UsuarioDTO>();
                        foreach (var responsable in tarea.Responsables)
                        {
                            if (responsable != null)
                            {
                                UsuarioDTO obj = listaUsuarios.Single(u => u.IdUsuario == responsable.IdUsuario);
                                responsable.Nombre = obj.Nombre;
                                responsable.Email = obj.Email;
                                responsable.Cuenta = obj.Cuenta;
                                responsable.Active = obj.Active;
                                responsable.IdRolUsuario = obj.IdRolUsuario;
                                responsable.IdCliente = obj.IdCliente;
                            }
                        }
                    }
                }
                return result;
            }
        }

        public int add(TareaDTO TareaDTO)
        {

            using (var context = getContext())
            {
                try
                {
                    Tarea Tarea = new Tarea();
                    Tarea.Nombre = TareaDTO.NombreTarea;
                    Tarea.Descripcion = TareaDTO.Descripcion;
                    Tarea.Observaciones = TareaDTO.Observaciones;
                    Tarea.FechaInicio = (TareaDTO.FechaInicio != null ? Convert.ToDateTime(TareaDTO.FechaInicio.Value.ToString("dd/MM/yyyy") + " " + TareaDTO.HoraInicio + ":" + TareaDTO.MinutoInicio) : TareaDTO.FechaInicio);
                    Tarea.FechaFin = (TareaDTO.FechaFin != null ? Convert.ToDateTime(TareaDTO.FechaFin.Value.ToString("dd/MM/yyyy") + " " + TareaDTO.HoraFin + ":" + TareaDTO.MinutoFin) : TareaDTO.FechaFin);
                    Tarea.IdCliente = TareaDTO.IdCliente;
                    Tarea.IdInmueble = TareaDTO.IdInmueble;
                    Tarea.IdServicio = TareaDTO.IdServicio;
                    if (TareaDTO.IdServicio == 4 || TareaDTO.IdServicio == 5)
                        Tarea.IdPlantilla = (TareaDTO.IdPlantilla.GetValueOrDefault() != 0) ? TareaDTO.IdPlantilla : null;
                    else
                        Tarea.IdPlantilla = null;

                    Tarea.IdEstado = TareaDTO.Responsables == null ? 1 : 2;//1 : Por Asignar, 2 : Asignado
                    Tarea.Active = true;
                    context.Tarea.Add(Tarea);

                    foreach (var resp in TareaDTO.Responsables)
                    {
                        var responsable = context.Usuario.Where(x => x.IdUsuario == resp.IdUsuario).SingleOrDefault();
                        Tarea.Usuario.Add(responsable);
                    }
                    context.SaveChanges();

                    TareaDTO.FechaInicio = Tarea.FechaInicio;
                    TareaDTO.FechaFin = Tarea.FechaFin;
                    TareaDTO.Cliente = context.Cliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ClienteDTO { NombreEmpresa = y.Nombre }).FirstOrDefault();
                    TareaDTO.Cliente.Contactos = context.ContactoCliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ContactoDTO { Nombre = y.Nombre }).ToList();
                    TareaDTO.Inmueble = context.Inmueble.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new InmuebleDTO { NombreInmueble = y.Nombre }).FirstOrDefault();
                    TareaDTO.Responsables = context.Tarea.Where(x => x.IdTarea == Tarea.IdTarea).SingleOrDefault().Usuario.Select(x => new UsuarioDTO { IdUsuario = x.IdUsuario, Nombre = x.Nombre, Email = x.Email }).ToList();
                    TareaDTO.Servicio = context.Servicio.Where(x => x.IdServicio == Tarea.IdServicio).Select(y => new ServicioDTO { NombreServicio = y.Nombre, NombreCorto = y.NombreCorto }).FirstOrDefault();
                    TareaDTO.Estado = context.Estado.Where(x => x.IdEstado == Tarea.IdEstado).Select(y => new EstadoDTO { NombreEstado = y.NombreEstado }).FirstOrDefault();
                    TareaDTO.Plantilla = context.Plantilla.Where(x => x.IdPlantilla == Tarea.IdPlantilla).Select(y => new PlantillaDTO { IdPlantilla = y.IdPlantilla, Nombre = y.Nombre, Nombre2 = y.Nombre2 }).FirstOrDefault();

                    if (TareaDTO.Responsables.Count > 0)
                        SendMailResponsable(TareaDTO, new List<int>(), TareaDTO.Responsables.Select(x => x.IdUsuario).ToList()); //SendMailResponsable(1, TareaDTO, null, null);
                    return Tarea.IdTarea;
                }
                catch (Exception e)
                {
                    throw e;
                    //return false;
                }
            }
        }

        public bool update(TareaDTO oTareaDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    var Tarea = context.Tarea.Where(x => x.IdTarea == oTareaDTO.IdTarea).SingleOrDefault();
                    Tarea.Nombre = oTareaDTO.NombreTarea;
                    Tarea.Descripcion = oTareaDTO.Descripcion;
                    Tarea.Observaciones = oTareaDTO.Observaciones;
                    Tarea.FechaInicio = (oTareaDTO.FechaInicio != null ? Convert.ToDateTime(oTareaDTO.FechaInicio.Value.ToString("dd/MM/yyyy") + " " + oTareaDTO.HoraInicio + ":" + oTareaDTO.MinutoInicio) : oTareaDTO.FechaInicio);
                    Tarea.FechaFin = (oTareaDTO.FechaFin != null ? Convert.ToDateTime(oTareaDTO.FechaFin.Value.ToString("dd/MM/yyyy") + " " + oTareaDTO.HoraFin + ":" + oTareaDTO.MinutoFin) : oTareaDTO.FechaFin);
                    Tarea.IdCliente = oTareaDTO.IdCliente;
                    Tarea.IdInmueble = oTareaDTO.IdInmueble;
                    Tarea.IdServicio = oTareaDTO.IdServicio;
                    if (oTareaDTO.IdServicio == 4 || oTareaDTO.IdServicio == 5)
                        Tarea.IdPlantilla = (oTareaDTO.IdPlantilla.GetValueOrDefault() != 0) ? oTareaDTO.IdPlantilla : null;
                    else
                        Tarea.IdPlantilla = null;

                    Tarea.IdEstado = oTareaDTO.IdEstado;//oTareaDTO.Responsables == null ? 1 : 2;//1 : Por Asignar, 2 : Asignado
                    Tarea.Active = oTareaDTO.Active;

                    var oldResponsables = Tarea.Usuario.Select(x => x.IdUsuario).ToList();
                    var newResponsables = oTareaDTO.Responsables.Select(x => x.IdUsuario).ToList();
                    var responsablesToRemove = oldResponsables.Except(newResponsables).ToList();
                    var responsablesToAdd = newResponsables.Except(oldResponsables).ToList();

                    foreach (var resp in responsablesToRemove)
                    {
                        var responsable = context.Usuario.Where(x => x.IdUsuario == resp).SingleOrDefault();
                        Tarea.Usuario.Remove(responsable);
                    }
                    foreach (var resp in responsablesToAdd)
                    {
                        var responsable = context.Usuario.Where(x => x.IdUsuario == resp).SingleOrDefault();
                        Tarea.Usuario.Add(responsable);
                    }
                    context.SaveChanges();

                    oTareaDTO.FechaInicio = Tarea.FechaInicio;
                    oTareaDTO.FechaFin = Tarea.FechaFin;
                    oTareaDTO.Cliente = context.Cliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ClienteDTO { NombreEmpresa = y.Nombre }).FirstOrDefault();
                    oTareaDTO.Cliente.Contactos = context.ContactoCliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ContactoDTO { Nombre = y.Nombre }).ToList();
                    oTareaDTO.Inmueble = context.Inmueble.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new InmuebleDTO { NombreInmueble = y.Nombre }).FirstOrDefault();
                    oTareaDTO.Responsables = context.Tarea.Where(x => x.IdTarea == Tarea.IdTarea).SingleOrDefault().Usuario.Select(x => new UsuarioDTO { IdUsuario = x.IdUsuario, Nombre = x.Nombre, Email = x.Email }).ToList();
                    oTareaDTO.Servicio = context.Servicio.Where(x => x.IdServicio == Tarea.IdServicio).Select(y => new ServicioDTO { NombreServicio = y.Nombre, NombreCorto = y.NombreCorto }).FirstOrDefault();
                    oTareaDTO.Estado = context.Estado.Where(x => x.IdEstado == Tarea.IdEstado).Select(y => new EstadoDTO { NombreEstado = y.NombreEstado }).FirstOrDefault();
                    oTareaDTO.Plantilla = context.Plantilla.Where(x => x.IdPlantilla == Tarea.IdPlantilla).Select(y => new PlantillaDTO { IdPlantilla = y.IdPlantilla, Nombre = y.Nombre, Nombre2 = y.Nombre2 }).FirstOrDefault();

                    if (oTareaDTO.Responsables.Count > 0)
                        SendMailResponsable(oTareaDTO, responsablesToRemove, responsablesToAdd);//SendMailResponsable(2, TareaDTO, null, null);
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                    return false;
                }
            }
        }

        public bool deleteTareaCalendario(int id)
        {
            using (var context = getContext())
            {
                try
                {
                    var Tarea = context.Tarea.Where(x => x.IdTarea == id).SingleOrDefault();
                    Tarea.FechaInicio = null;
                    Tarea.FechaFin = null;

                    var oldResponsables = Tarea.Usuario.Select(x => new UsuarioDTO { IdUsuario = x.IdUsuario, Nombre = x.Nombre, Email = x.Email }).ToList();
                    var responsablesToRemove = Tarea.Usuario.Select(x => x.IdUsuario).ToList();

                    foreach (var resp in responsablesToRemove)
                    {
                        var responsable = context.Usuario.Where(x => x.IdUsuario == resp).SingleOrDefault();
                        Tarea.Usuario.Remove(responsable);
                    }
                    context.SaveChanges();

                    if (responsablesToRemove.Count > 0)
                    {
                        TareaDTO oTareaDTO = new TareaDTO
                        {
                            NombreTarea = Tarea.Nombre,
                            Descripcion = Tarea.Descripcion,
                            FechaInicio = Tarea.FechaInicio,
                            FechaFin = Tarea.FechaFin,
                            Cliente = context.Cliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ClienteDTO { NombreEmpresa = y.Nombre }).FirstOrDefault(),
                            Contactos = context.ContactoCliente.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new ContactoDTO { Nombre = y.Nombre }).ToList(),
                            Inmueble = context.Inmueble.Where(x => x.IdCliente == Tarea.IdCliente).Select(y => new InmuebleDTO { NombreInmueble = y.Nombre }).FirstOrDefault(),
                            Responsables = oldResponsables,//context.Usuario.AsEnumerable().Where(y => responsablesToRemove.IndexOf(y.IdUsuario) > -1).Select(x => new UsuarioDTO { IdUsuario = x.IdUsuario, Nombre = x.Nombre, Email = x.Email }).ToList(),
                            Servicio = context.Servicio.Where(x => x.IdServicio == Tarea.IdServicio).Select(y => new ServicioDTO { NombreServicio = y.Nombre, NombreCorto = y.NombreCorto }).FirstOrDefault(),
                            Estado = context.Estado.Where(x => x.IdEstado == Tarea.IdEstado).Select(y => new EstadoDTO { NombreEstado = y.NombreEstado }).FirstOrDefault()
                        };
                        SendMailResponsable(oTareaDTO, responsablesToRemove, new List<int>());
                    }

                    return true;
                }
                catch (Exception e)
                {
                    //throw e;
                    return false;
                }
            }
        }

        public bool deleteTareaBolsa(int id)
        {
            using (var context = getContext())
            {
                try
                {
                    var Tarea = context.Tarea.Where(x => x.IdTarea == id).SingleOrDefault();
                    Tarea.Active = false;
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

        public TareaDTO getTarea(int id)
        {
            using (var context = getContext())
            {

                var result = (context.Tarea.Where(x => x.IdTarea == id).AsEnumerable()
                    .Select(r => new TareaDTO
                    {
                        IdTarea = r.IdTarea,
                        NombreTarea = r.Nombre,
                        Descripcion = r.Descripcion,
                        Observaciones = r.Observaciones,
                        FechaInicio = r.FechaInicio,
                        FechaFin = r.FechaFin,
                        HoraInicio = r.FechaInicio != null ? Convert.ToInt32(r.FechaInicio.Value.ToString("HH")) : -1,
                        MinutoInicio = r.FechaInicio != null ? Convert.ToInt32(r.FechaInicio.Value.ToString("mm")) : -1,
                        HoraFin = r.FechaFin != null ? Convert.ToInt32(r.FechaFin.Value.ToString("HH")) : -1,
                        MinutoFin = r.FechaFin != null ? Convert.ToInt32(r.FechaFin.Value.ToString("mm")) : -1,
                        IdCliente = r.IdCliente,
                        IdInmueble = r.IdInmueble,
                        IdPlantilla = r.IdPlantilla,
                        IdServicio = r.IdServicio,
                        IdEstado = r.IdEstado,
                        Cliente = context.Cliente.Where(x => x.IdCliente == r.IdCliente).Select(y => new ClienteDTO { NombreEmpresa = y.Nombre }).FirstOrDefault(),
                        Inmueble = context.Inmueble.Where(x => x.IdCliente == r.IdCliente).Select(y => new InmuebleDTO { NombreInmueble = y.Nombre }).FirstOrDefault(),
                        Responsables = r.Usuario.Select(x => new UsuarioDTO { IdUsuario = x.IdUsuario, Nombre = x.Nombre }).ToList(),
                        Servicio = context.Servicio.Where(x => x.IdServicio == r.IdServicio).Select(y => new ServicioDTO { NombreServicio = y.Nombre, NombreCorto = y.NombreCorto }).FirstOrDefault(),
                        Estado = context.Estado.Where(x => x.IdEstado == r.IdEstado).Select(y => new EstadoDTO { NombreEstado = y.NombreEstado }).FirstOrDefault(),
                        Active = r.Active
                    })).SingleOrDefault();
                return result;
            }
        }
        #endregion

        public List<PlantillaDTO> getPlantillasViewBag()
        {
            using (var context = getContext())
            {
                var result = context.Plantilla.Select(x => new PlantillaDTO
                {
                    IdPlantilla = x.IdPlantilla,
                    Nombre = x.Nombre,
                    Nombre2 = x.Nombre2,
                    Active = x.Active
                }).ToList();
                return result;
            }
        }

        public IList<PlantillaDTO> getPlantillasBag(bool AsSelectList = false)
        {
            if (!AsSelectList)
                return getPlantillasViewBag();
            else
            {
                var lista = getPlantillasViewBag();
                lista.Insert(0, new PlantillaDTO() { IdPlantilla = 0, Nombre = "Seleccione un protocolo" });
                return lista;
            }
        }
    }
}
