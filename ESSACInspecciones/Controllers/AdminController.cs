using ESSACInspecciones.Core.BL;
using ESSACInspecciones.Core.DTO;
using ESSACInspecciones.Helpers;
using ESSACInspecciones.Helpers.Razor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PagedList;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;

namespace ESSACInspecciones.Controllers
{
    public class AdminController : Controller
    {
        private bool currentUser()
        {
            if (System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session["User"] != null) { return true; }
            else { return false; }
        }
        private UsuarioDTO getCurrentUser()
        {
            if (this.currentUser())
            {
                return (UsuarioDTO)System.Web.HttpContext.Current.Session["User"];
            }
            return null;
        }
        private bool isSuperAdministrator()
        {
            if (getCurrentUser().IdRol == 1) return true;
            return false;
        }
        private bool isAdministrator()
        {
            if (getCurrentUser().IdRol <= 2) return true;
            return false;
        }
        private void createResponseMessage(string status, string message = "", string status_field = "status", string message_field = "message")
        {
            TempData[status_field] = status;
            if (!String.IsNullOrWhiteSpace(message))
            {
                TempData[message_field] = message;
            }
        }

        public AdminController()
        {
            UsuarioDTO user = getCurrentUser();
            if (user != null)
            {
                ViewBag.currentUser = user;
                ViewBag.EsAdmin = isAdministrator();
                ViewBag.EsSuperAdmin = isSuperAdministrator();
                ViewBag.IdRol = user.IdRol;
            }
            else { ViewBag.EsAdmin = false; ViewBag.EsSuperAdmin = false; }
        }

        public ActionResult Index(int? searchResponsable)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }

            TareaBL objBL = new TareaBL();
            var model = objBL.getResponsables(true);
            model[0].Nombre = "Todos los responsables";
            ViewBag.IdResponsable = 0;

            if (searchResponsable != null && searchResponsable != 0)
            {
                ViewBag.IdResponsable = searchResponsable;
                ViewBag.Nombre = model.Where(x => x.IdUsuario == searchResponsable).Select(y => y.Nombre).SingleOrDefault();
            }
            if (searchResponsable == null && searchResponsable != 0)
            {
                UsuarioDTO user = getCurrentUser();
                if (user.IdRol == CONSTANTES.ROL_RESPONSABLE)
                {
                    ViewBag.IdResponsable = user.IdUsuario;
                    ViewBag.Nombre = user.Nombre;
                }
            }

            return View(model);
        }

        public ActionResult Usuarios()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            UsuariosBL usuariosBL = new UsuariosBL();
            UsuarioDTO currentUser = getCurrentUser();
            return View(usuariosBL.getUsuarios(currentUser.IdRol));//(CONSTANTES.ROL_RESPONSABLE));
        }

        public ActionResult Usuario(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            UsuarioDTO currentUser = getCurrentUser();
            if (!this.isAdministrator() && id != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            if (id == 1 && !this.isSuperAdministrator()) { return RedirectToAction("Index"); }
            UsuariosBL usuariosBL = new UsuariosBL();
            IList<RolDTO> roles = usuariosBL.getRoles();
            //var rolesList = roles.ToList();
            roles.Insert(0, new RolDTO() { IdRol = 0, Nombre = "Seleccione un Rol" });
            ViewBag.Roles = roles;//.AsEnumerable();
            var objSent = TempData["Usuario"];
            if (objSent != null) { TempData["Usuario"] = null; return View(objSent); }
            if (id != null)
            {
                UsuarioDTO usuario = usuariosBL.getUsuario((int)id);
                return View(usuario);
            }
            return View();
        }

        public ActionResult AddUser(UsuarioDTO user, string passUser = "", string passChange = "")
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    string nomnbreImagen = user.Nombre + user.InicialesNombre;
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExt = Path.GetExtension(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/"), nomnbreImagen + fileExt);
                    file.SaveAs(path);
                }
            }
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            UsuarioDTO currentUser = getCurrentUser();
            if (!this.isAdministrator() && user.IdUsuario != currentUser.IdUsuario) { return RedirectToAction("Index"); }
            if (user.IdUsuario == 1 && !this.isSuperAdministrator()) { return RedirectToAction("Index"); }
            try
            {
                UsuariosBL usuariosBL = new UsuariosBL();
                if (user.IdUsuario == 0 && usuariosBL.validateUsuario(user))
                {
                    usuariosBL.add(user);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Usuarios");
                }
                else if (user.IdUsuario != 0)
                {
                    if (usuariosBL.update(user, passUser, passChange, this.getCurrentUser()))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        if (user.IdUsuario == this.getCurrentUser().IdUsuario)
                        {
                            System.Web.HttpContext.Current.Session["User"] = usuariosBL.getUsuario(user.IdUsuario);
                            if (!this.getCurrentUser().Active) System.Web.HttpContext.Current.Session["User"] = null;
                        }
                        return RedirectToAction("Usuarios");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE + "<br>Si está intentando actualizar la contraseña, verifique que ha ingresado la contraseña actual correctamente.");
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (user.IdUsuario != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Usuario"] = user;
            return RedirectToAction("Usuario");
        }

        public ActionResult Clientes()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ClienteBL objBL = new ClienteBL();
            return View(objBL.getClientes());
        }

        public ActionResult Cliente(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            ClienteBL objBL = new ClienteBL();
            ViewBag.IdCliente = id;
            var objSent = TempData["Cliente"];
            if (objSent != null) { TempData["Cliente"] = null; return View(objSent); }
            if (id != null)
            {
                ViewBag.Inmuebles = objBL.getInmuebles((int)id);
                ViewBag.Contactos = objBL.getContactos((int)id);
                ClienteDTO obj = objBL.getCliente((int)id);
                return View(obj);
            }
            return View();
        }

        public ActionResult AddCliente(ClienteDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ClienteBL objBL = new ClienteBL();
                if (dto.IdCliente == 0)
                {
                    objBL.add(dto);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Clientes");
                }
                else if (dto.IdCliente != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Clientes");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (dto.IdCliente != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Cliente"] = dto;
            return RedirectToAction("Cliente");
        }

        public ActionResult Inmuebles(int id)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            return View();
        }

        public ActionResult Inmueble(int IdCliente, int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ClienteBL objBL = new ClienteBL();
            ViewBag.IdCliente = IdCliente;
            var objSent = TempData["Inmueble"];
            if (objSent != null) { TempData["Inmueble"] = null; return View(objSent); }
            if (id != null)
            {
                InmuebleDTO obj = objBL.getInmueble((int)id);
                return View(obj);
            }
            return View();
        }

        public ActionResult AddInmueble(InmuebleDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ClienteBL objBL = new ClienteBL();
                if (dto.IdInmueble == 0)
                {
                    objBL.add(dto);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Cliente", new { id = dto.IdCliente });
                }
                else if (dto.IdInmueble != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Cliente", new { id = dto.IdCliente });
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (dto.IdInmueble != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Inmueble"] = dto;
            return RedirectToAction("Inmueble", new { IdCliente = dto.IdCliente });
        }

        public ActionResult Contacto(int IdCliente, int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ClienteBL objBL = new ClienteBL();
            ViewBag.IdCliente = IdCliente;
            var objSent = TempData["Contacto"];
            if (objSent != null) { TempData["Contacto"] = null; return View(objSent); }
            if (id != null)
            {
                ContactoDTO obj = objBL.getContacto((int)id);
                return View(obj);
            }
            return View();
        }

        public ActionResult AddContacto(ContactoDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ClienteBL objBL = new ClienteBL();
                if (dto.IdContacto == 0)
                {
                    objBL.add(dto);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Cliente", new { id = dto.IdCliente });
                }
                else if (dto.IdContacto != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Cliente", new { id = dto.IdCliente });
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (dto.IdContacto != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Contacto"] = dto;
            return RedirectToAction("Contacto", new { IdCliente = dto.IdCliente });
        }

        public ActionResult Servicios()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ServiciosBL objBL = new ServiciosBL();
            return View(objBL.getServicios());
        }

        public ActionResult Servicio(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ServiciosBL objBL = new ServiciosBL();
            var objSent = TempData["Servicio"];
            if (objSent != null) { TempData["Servicio"] = null; return View(objSent); }
            if (id != null)
            {
                ServicioDTO obj = objBL.getServicio((int)id);
                return View(obj);
            }
            return View();
        }

        public ActionResult AddServicio(ServicioDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ServiciosBL objBL = new ServiciosBL();
                if (dto.IdServicio == 0)
                {
                    objBL.add(dto);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Servicios");
                }
                else if (dto.IdServicio != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Servicios");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }

                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (dto.IdServicio != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Servicio"] = dto;
            return RedirectToAction("Servicio");
        }

        public ActionResult Tareas(int? searchResponsable, int? page)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }

            TareaBL objBL = new TareaBL();
            var model = objBL.getTareas();
            var responsables = objBL.getResponsables(true);
            responsables[0].Nombre = "Todos los responsables";
            ViewBag.Responsables = responsables;
            ViewBag.IdResponsable = searchResponsable ?? 0;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (searchResponsable != null && searchResponsable != 0)
                model = model.Where(x => x.Responsables.Any(y=>y.IdUsuario == searchResponsable)).ToList();

            return View(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Tarea(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }

            TareaBL objBL = new TareaBL();
            var dataResponsables = objBL.getResponsables(true);//ViewBag.Responsables = objBL.getResponsables(true);
            ViewBag.Servicios = objBL.getServicios(true);
            ViewBag.Clientes = objBL.getComboClientes();//objBL.getClientes(true);
            ViewBag.Horas = new TareaDTO().fillHoras();
            ViewBag.Minutos = new TareaDTO().fillMinutos();

            var objSent = (TareaDTO)TempData["Tarea"];
            if (objSent != null)
            {
                UsuariosBL oUsuariosBL = new UsuariosBL();
                TempData["Tarea"] = null;
                ViewBag.Responsables = removeResponsables(dataResponsables, objSent.Responsables);//ViewBag.Responsables = dataResponsables;
                ViewBag.Resources = objSent.Responsables.AsEnumerable().Select(r => new UsuarioDTO { IdUsuario = r.IdUsuario, Nombre = oUsuariosBL.getUsuario(r.IdUsuario).Nombre }).ToList();
                return View(objSent);
            }
            if (id != null)
            {
                TareaDTO obj = objBL.getTarea((int)id);
                //var resource = obj.Responsables;
                //List<UsuarioDTO> responsablesLeft = new List<UsuarioDTO>();//.ToList();
                //responsablesLeft = dataResponsables.ToList();
                //foreach (var resp in dataResponsables)
                //    foreach (var resptarea in resource)
                //        if (resp.IdUsuario > 0 && resp.IdUsuario == resptarea.IdUsuario)
                //            responsablesLeft.Remove(resp);
                //ViewBag.Responsables = responsablesLeft;
                ViewBag.Responsables = removeResponsables(dataResponsables, obj.Responsables);
                ViewBag.Resources = obj.Responsables;
                return View(obj);
            }
            else
            {
                ViewBag.Responsables = dataResponsables;
            }
            return View();
        }

        public List<UsuarioDTO> removeResponsables(IList<UsuarioDTO> AllResponsables, List<UsuarioDTO> SeldResponsables)
        {
            List<UsuarioDTO> responsablesLeft = new List<UsuarioDTO>();//.ToList();
            responsablesLeft = AllResponsables.ToList();
            foreach (var resp in AllResponsables)
                foreach (var resptarea in SeldResponsables)
                    if (resp.IdUsuario > 0 && resp.IdUsuario == resptarea.IdUsuario)
                        responsablesLeft.Remove(resp);
            return responsablesLeft;
        }

        public ActionResult AddTarea(TareaDTO dto, int[] responsables)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                if (responsables != null)
                    dto.Responsables = responsables.Select(x => new UsuarioDTO { IdUsuario = x }).ToList();
                else
                    dto.Responsables = new List<UsuarioDTO>();

                TareaBL objBL = new TareaBL();
                if (dto.IdTarea == 0)
                {
                    objBL.add(dto);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Tareas");
                }
                else if (dto.IdTarea != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Tareas");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }
                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (dto.IdTarea != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Tarea"] = dto;
            return RedirectToAction("Tarea");
        }

        public ActionResult Ingresar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UsuarioDTO user)
        {
            if (ModelState.IsValid)
            {
                UsuariosBL usuariosBL = new UsuariosBL();
                if (usuariosBL.isValidUser(user))
                {
                    System.Web.HttpContext.Current.Session["User"] = usuariosBL.getUsuarioByCuenta(user);//new UsuarioDTO() { Nombre = "NubeLabs", IdUsuario = 1, IdRol = 1 }; //{ Nombre = "Responsable 1", IdUsuario = 2, IdRol = 3 };  //usuariosBL.getUsuarioByCuenta(user);
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Ingresar");
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Ingresar");
        }

        public ActionResult RecuperaContrasena()
        {
            return View();
        }
        public ActionResult RecoverPassword(string cuentaContrasena)
        {
            UsuariosBL objUsuariosBL = new UsuariosBL();
            var result = objUsuariosBL.recoverPassword(cuentaContrasena);
            if (result)
            {
                createResponseMessage(CONSTANTES.SUCCESS, CONSTANTES.SUCCESS_RECOVERY_PASSWORD);
                return RedirectToAction("Ingresar");
            }
            else
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_RECOVERY_PASSWORD);
                return View();
            }
        }

        public ActionResult Busqueda(string search)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ClienteBL objClienteBL = new ClienteBL();
            UsuariosBL objUsuariosBL = new UsuariosBL();
            TareaBL objTareaBL = new TareaBL();
            ServiciosBL objServiciosBL = new ServiciosBL();
            if (!string.IsNullOrEmpty(search.Trim()))
            {
                ViewBag.Clientes = objClienteBL.searchClientes(search);
                ViewBag.Inmuebles = objClienteBL.searchInmuebles(search);
                ViewBag.Responsables = objUsuariosBL.searchResponsables(search);
                ViewBag.Tareas = objTareaBL.searchTareas(search);
                ViewBag.Servicios = objServiciosBL.searchServicios(search);
            }
            return View();
        }

        public ActionResult Responsables()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            UsuariosBL usuariosBL = new UsuariosBL();
            var responsables = usuariosBL.getUsuarios(CONSTANTES.ROL_RESPONSABLE).OrderBy(x => x.IdUsuario).ToList();
            /* IDictionary<string, UsuarioDTO> dictResponsables = new Dictionary<string, UsuarioDTO>();
             foreach (var responsable in responsables){
                 dictResponsables.Add(responsable.InicialesNombre + " - " + responsable.Nombre, responsable);
             }*/
            ViewBag.Responsables = responsables;

            return View();
        }

        [HttpPost]
        public ActionResult ReporteMes(int[] responsables)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }

            if (responsables != null && responsables.Length > 0)
            {
                ViewBag.Responsables = responsables;
                UsuariosBL usuariosBL = new UsuariosBL();
                ViewBag.ResponsablesDTO = usuariosBL.getUsuarios(CONSTANTES.ROL_RESPONSABLE, responsables);
                ViewBag.ResponsablesString = String.Join(",", responsables.Select(x => x.ToString()).ToArray());
                return View();
            }
            else
            {
                createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_SELECT_RESPONSABLE);
                return RedirectToAction("Responsables");
            }
        }

        public ActionResult Protocolos(int? inmueble, int? page)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            List<ProtocoloDTO> model = new List<ProtocoloDTO>();
            ProtocoloBL objBL = new ProtocoloBL();
            int idUsuario = getCurrentUser().IdUsuario;
            ViewBag.Clientes = objBL.getClientesTarea(idUsuario);
            if (inmueble != 0 && inmueble != null)
            {
                model = objBL.getProtocolos(idUsuario, (int)inmueble);
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(model.ToPagedList(pageNumber, pageSize));
            //return View();
        }
        //public ActionResult Protocolo(bool conexion, int idInmueble, int? idProtocolo = null, int? idPlantilla = null)
        //{
        //    if (conexion)
        //    {
        //        return RedirectToAction("Protocolo_server", new { idInmueble = idInmueble, idProtocolo = idProtocolo, idPlantilla = idPlantilla });
        //    }
        //    else
        //    {
        //        return RedirectToAction("Protocolo_client");
        //    }
        //}
        public ActionResult Protocolo_server(int idInmueble, int? idProtocolo = null, int? idPlantilla = null)
        {
            ViewBag.Horas = new BaseDTO().fillHoras();
            ViewBag.Minutos = new BaseDTO().fillMinutos();
            ViewBag.Items_SelectSINO = new BaseDTO().fillSelectSINO();
            ViewBag.Items_SelectBomba = new BaseDTO().fillSelectBomba();
            ViewBag.Items_SelectNivelTanque = new BaseDTO().fillSelectNivelTanque();
            ViewBag.Items_SelectAccesorios = new BaseDTO().fillSelectAccesorios();
            ViewBag.Items_SelectPresiones = new BaseDTO().fillSelectPresiones();
            ProtocoloBL objBL = new ProtocoloBL();
            ProtocoloDTO obj = objBL.getProtocolo(idInmueble, idProtocolo, idPlantilla);
            return View(obj);
        }
        public ActionResult Protocolo()
        {
            OpcionRespuestaBL obj = new OpcionRespuestaBL();
            ViewBag.Horas = new BaseDTO().fillHoras().ToJSON();
            ViewBag.Minutos = new BaseDTO().fillMinutos().ToJSON();
            ViewBag.Items_SelectSINO = obj.getOpcionRespuesta(3).ToJSON();//new BaseDTO().fillSelectSINO().ToJSON();
            ViewBag.Items_SelectBomba = obj.getOpcionRespuesta(4).ToJSON(); //new BaseDTO().fillSelectBomba().ToJSON();
            ViewBag.Items_SelectNivelTanque = obj.getOpcionRespuesta(5).ToJSON(); //new BaseDTO().fillSelectNivelTanque().ToJSON();
            ViewBag.Items_SelectAccesorios = obj.getOpcionRespuesta(6).ToJSON(); //new BaseDTO().fillSelectAccesorios().ToJSON();
            ViewBag.Items_SelectPresiones = obj.getOpcionRespuesta(7).ToJSON(); //new BaseDTO().fillSelectPresiones().ToJSON();

            return View();
        }

        public ActionResult ProtocoloReporte()
        {
            OpcionRespuestaBL obj = new OpcionRespuestaBL();
            ViewBag.Horas = new BaseDTO().fillHoras().ToJSON();
            ViewBag.Minutos = new BaseDTO().fillMinutos().ToJSON();
            ViewBag.Items_SelectSINO = obj.getOpcionRespuesta(3).ToJSON();//new BaseDTO().fillSelectSINO().ToJSON();
            ViewBag.Items_SelectBomba = obj.getOpcionRespuesta(4).ToJSON(); //new BaseDTO().fillSelectBomba().ToJSON();
            ViewBag.Items_SelectNivelTanque = obj.getOpcionRespuesta(5).ToJSON(); //new BaseDTO().fillSelectNivelTanque().ToJSON();
            ViewBag.Items_SelectAccesorios = obj.getOpcionRespuesta(6).ToJSON(); //new BaseDTO().fillSelectAccesorios().ToJSON();
            ViewBag.Items_SelectPresiones = obj.getOpcionRespuesta(7).ToJSON(); //new BaseDTO().fillSelectPresiones().ToJSON();

            return View();
        }

        public ActionResult AddProtocolo(ProtocoloDTO dto)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ProtocoloBL objBL = new ProtocoloBL();
                dto.IdUsuario = getCurrentUser().IdUsuario;
                if (dto.IdProtocolo == 0)
                {
                    //int idProtocolo = objBL.add_053(dto);
                    if (objBL.add(dto))
                    {
                        //dto.IdProtocolo = idProtocolo;
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Protocolos");
                    }
                }
                else if (dto.IdProtocolo != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Protocolos");
                    }
                    else
                    {
                        createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                    }
                }
                else
                {
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
                }
            }
            catch
            {
                if (dto.IdProtocolo != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Protocolo"] = dto;
            return RedirectToAction("Protocolo_server");
        }

        #region APIS
        public void CrearPDF(ProtocoloDTO protocolo)
        {
            Document doc = new Document();

            string path = Server.MapPath("~/Content/pdfs");
            PdfWriter.GetInstance(doc, new FileStream(path + "/Doc1.pdf", FileMode.Create));

            //Mis Fonts
            Font myFontTitle18 = FontFactory.GetFont("Open Sans", 18);
            Font myFontTitle18_B = FontFactory.GetFont("Open Sans", 18, Font.BOLD);
            Font myFontTitle15 = FontFactory.GetFont("Open Sans", 15);
            Font myFontTitle15_B = FontFactory.GetFont("Open Sans", 15, Font.BOLD);
            Font myFontTextH12 = FontFactory.GetFont("Open Sans", 12);
            Font myFontTextH12_B = FontFactory.GetFont("Open Sans", 12, Font.BOLD);
            Font myFontText10 = FontFactory.GetFont("Open Sans", 10);
            Font myFontText10_B = FontFactory.GetFont("Open Sans", 10, Font.BOLD);
            int numColumns = 12;

            doc.Open();
            doc.Add(new Paragraph("Protocolo de Pruebas", myFontTitle18_B));
            doc.Add(new Paragraph(protocolo.NombreAreaProtegida + " según NFPA 20 - Motobomba de agua Contra Incendio", myFontTitle15));
            doc.Add(new Paragraph(" "));

            //Cabecera del protocolo
            PdfPTable tableHeader = new PdfPTable(numColumns);
            PdfPCell cellHeader = new PdfPCell();
            cellHeader.Colspan = 6;
            cellHeader.Phrase = new Phrase("Nombre del Área Protegida:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase("Fecha:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase(protocolo.NombreAreaProtegida, myFontTextH12);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase(protocolo.Fecha.ToString(), myFontTextH12);
            tableHeader.AddCell(cellHeader);

            cellHeader.Phrase = new Phrase("Dirección:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase("Hora de Inicio:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase(protocolo.Direccion, myFontTextH12);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase(protocolo.HoraInicio + ":" + protocolo.MinutoInicio, myFontTextH12);
            tableHeader.AddCell(cellHeader);
            doc.Add(tableHeader);
            //Fin de la cabecera del protocolo

            doc.Add(new Paragraph(" "));

            OpcionRespuestaBL obj = new OpcionRespuestaBL();
            ViewBag.Horas = new BaseDTO().fillHoras().ToJSON();
            ViewBag.Minutos = new BaseDTO().fillMinutos().ToJSON();
            List<OpcionDTO> opciones3 = (List<OpcionDTO>)obj.getOpcionRespuesta(3);
            List<OpcionDTO> opciones4 = (List<OpcionDTO>)obj.getOpcionRespuesta(4);
            List<OpcionDTO> opciones5 = (List<OpcionDTO>)obj.getOpcionRespuesta(5);
            List<OpcionDTO> opciones6 = (List<OpcionDTO>)obj.getOpcionRespuesta(6);
            List<OpcionDTO> opciones7 = (List<OpcionDTO>)obj.getOpcionRespuesta(7);

            //Inicio Tabla Reporte
            PdfPTable tableSeccion = new PdfPTable(numColumns);
            foreach(var Seccion in protocolo.Secciones)
            {
                PdfPCell cellSeccion = new PdfPCell();
                cellSeccion.Colspan = numColumns;
                cellSeccion.Phrase = new Phrase(Seccion.Nombre, myFontTextH12_B);
                tableSeccion.AddCell(cellSeccion);

                //PdfPTable tableSeccionBody = new PdfPTable(numColumns);
                foreach (var SeccionBody in Seccion.SeccionBodys)
                {
                    PdfPCell cellSeccionBody = new PdfPCell();
                    cellSeccionBody.Rowspan = SeccionBody.Rowspan;
                    cellSeccionBody.Colspan = SeccionBody.Colspan;
                    if (SeccionBody.IdTipoCelda == 1)
                    {
                        cellSeccionBody.Phrase = new Phrase(SeccionBody.Descripcion);
                    }
                    else
                    {
                        string rpta = "";
                        List<OpcionDTO> auxOpc = new List<OpcionDTO>();
                        switch(SeccionBody.IdTipoTag)
                        {
                            case 3: auxOpc = opciones3; break;
                            case 4: auxOpc = opciones4; break;
                            case 5: auxOpc = opciones5; break;
                            case 6: auxOpc = opciones6; break;
                            case 7: auxOpc = opciones7; break;
                            default: auxOpc = null; break;
                        }
                        //NO HAY CASE 2 PORQUE ESTA CONTEMPLADO EN "ELSE" DE ABAJO
                        if (auxOpc != null)
                            rpta = auxOpc[Convert.ToInt32(SeccionBody.Respuesta)].NombreOpcion;
                        else
                            rpta = SeccionBody.Respuesta;
                        
                        cellSeccionBody.Phrase = new Phrase(rpta);
                    }
                    tableSeccion.AddCell(cellSeccionBody);
                }
                //tableSeccion.AddCell(tableSeccionBody);
            }
            doc.Add(tableSeccion);

            doc.Add(new Paragraph(" "));

            PdfPTable table = new PdfPTable(3);
            PdfPCell cell = new PdfPCell(new Phrase("Header spanning 3 columns"));
            cell.Colspan = 3;
            cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.AddCell(cell);
            table.AddCell("Col 1 Row 1");
            table.AddCell("Col 2 Row 1");
            table.AddCell("Col 3 Row 1");
            table.AddCell("Col 1 Row 2");
            table.AddCell("Col 2 Row 2");
            table.AddCell("Col 3 Row 2");
            doc.Add(table);


            doc.Add(new Paragraph(" "));
            doc.Close();
        }

        [HttpGet]
        public ActionResult GetUsuarios(string descripcion)
        {
            UsuariosBL objBL = new UsuariosBL();
            var lista = objBL.getUsuarios();
            if (!string.IsNullOrEmpty(descripcion))
            {
                lista = lista.Where(x => x.Nombre.Contains(descripcion.Trim())).ToList();
            }
            return Json(lista, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveProtocolo(string protocolo)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            var objProtocolo = new JavaScriptSerializer().Deserialize<ProtocoloDTO>(protocolo);
            objProtocolo.IdUsuario = getCurrentUser().IdUsuario;
            if (!string.IsNullOrEmpty(objProtocolo.StrFecha)) objProtocolo.Fecha = Convert.ToDateTime(objProtocolo.StrFecha);
            ProtocoloBL objBL = new ProtocoloBL();
            bool response = false;
            if (objProtocolo.IdProtocolo == 0) response = objBL.add(objProtocolo);
            else response = objBL.update(objProtocolo);
            return Json(response, JsonRequestBehavior.AllowGet);
            //return Json(new { Response = response }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetProtocolos(int inmueble)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ProtocoloBL objBL = new ProtocoloBL();
            int idUsuario = getCurrentUser().IdUsuario;
            var model = objBL.getProtocolos(idUsuario, inmueble);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetProtocolo(int idInmueble, int? idProtocolo = null, int? idPlantilla = null)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ProtocoloBL objBL = new ProtocoloBL();
            //int idUsuario = getCurrentUser().IdUsuario;
            var model = objBL.getProtocolo(idInmueble, idProtocolo, idPlantilla);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProtocoloReporte(int idInmueble, int? idProtocolo = null, int? idPlantilla = null)
        {
            ProtocoloBL objBL = new ProtocoloBL();
            //int idUsuario = getCurrentUser().IdUsuario;
            var model = objBL.getProtocolo(idInmueble, idProtocolo, idPlantilla);
            CrearPDF((ProtocoloDTO)model);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddTareaCalendario(string tarea)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            var _tarea = new JavaScriptSerializer().Deserialize<TareaDTO>(tarea);
            if (!string.IsNullOrEmpty(_tarea.StrFechaInicio)) _tarea.FechaInicio = Convert.ToDateTime(_tarea.StrFechaInicio);
            if (!string.IsNullOrEmpty(_tarea.StrFechaFin)) _tarea.FechaFin = Convert.ToDateTime(_tarea.StrFechaFin);
            TareaBL objBL = new TareaBL();
            ServiciosBL oServiciosBL = new ServiciosBL();
            //var color = oServiciosBL.getServicio(_tarea.IdServicio).ColorServicio;
            int idTarea = 0;
            if (_tarea.IdTarea == 0)
            {
                idTarea = objBL.add(_tarea);
            }
            else
            {
                objBL.update(_tarea);
            }
            return Json(new { Response = idTarea, ColorServicio = oServiciosBL.getServicio(_tarea.IdServicio).ColorServicio }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteTareaBolsa(int id)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(new { Response = objBL.deleteTareaBolsa(id) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteTareaCalendario(int id)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(new { Response = objBL.deleteTareaCalendario(id) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        //public ActionResult UpdateTareaResponsable(int evento, int idTarea, int[] idResponsable, string fechaIni, string fechaFin)
        public ActionResult UpdateTareaResponsable(int tipoCorreo, string tarea)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            var objTarea = new JavaScriptSerializer().Deserialize<TareaDTO>(tarea);
            if (!string.IsNullOrEmpty(objTarea.StrFechaInicio)) objTarea.FechaInicio = Convert.ToDateTime(objTarea.StrFechaInicio);
            if (!string.IsNullOrEmpty(objTarea.StrFechaFin)) objTarea.FechaFin = Convert.ToDateTime(objTarea.StrFechaFin);
            TareaBL objBL = new TareaBL();
            return Json(new { Response = objBL.updateTareaResponsable(tipoCorreo, objTarea) }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetBolsaTareas()
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getBolsaTareas(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetResponsablesCalendario()
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getResponsables2(), JsonRequestBehavior.AllowGet);
            //return Json(Newtonsoft.Json.JsonConvert.SerializeObject(objBL.getResponsables2(), new Newtonsoft.Json.Converters.IsoDateTimeConverter()));
        }
        [HttpGet]
        public ActionResult GetTareasResponsables(string fechaInicio, string fechaFin)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            DateTime FechaInicio = Convert.ToDateTime(fechaInicio);
            DateTime FechaFin = Convert.ToDateTime(fechaFin);
            return Json(objBL.getTareasResponsables(FechaInicio, FechaFin), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetTareasPorResponsable(int[] responsables)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getTareasResponsables(responsables), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetTarea(int id)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getTarea(id), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetServicios()
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getServicios(true), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetResponsables()
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getResponsables(true), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetClientes()
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getComboClientes(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetListInmueblesByClienteId(int id)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getInmuebles(id, true), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetEstadosByServicioId(int id)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getEstados(id), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetContadorEstados(string tarea)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            var _tarea = new JavaScriptSerializer().Deserialize<TareaDTO>(tarea);
            if (!string.IsNullOrEmpty(_tarea.StrFechaInicio)) _tarea.FechaInicio = Convert.ToDateTime(_tarea.StrFechaInicio);
            if (!string.IsNullOrEmpty(_tarea.StrFechaFin)) _tarea.FechaFin = Convert.ToDateTime(_tarea.StrFechaFin);
            TareaBL objBL = new TareaBL();
            return Json(objBL.GetContadorEstados(_tarea), JsonRequestBehavior.AllowGet);
        }
        #endregion
        public ActionResult Formulario()
        { return View(); }
    }
}
