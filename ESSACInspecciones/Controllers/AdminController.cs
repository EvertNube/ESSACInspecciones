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
using ESSACInspecciones.Models;
using System.Web.UI.DataVisualization.Charting;

namespace ESSACInspecciones.Controllers
{
    public class AdminController : Controller
    {
        protected Navbar navbar { get; set; }
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
            if (getCurrentUser().IdRolUsuario == 1) return true;
            return false;
        }
        private bool isAdministrator()
        {
            if (getCurrentUser().IdRolUsuario <= 2) return true;
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
                this.navbar = new Navbar();
                ViewBag.Navbar = this.navbar;

                ViewBag.currentUser = user;
                ViewBag.EsAdmin = isAdministrator();
                ViewBag.EsSuperAdmin = isSuperAdministrator();
                ViewBag.IdRol = user.IdRolUsuario;
            }
            else { ViewBag.EsAdmin = false; ViewBag.EsSuperAdmin = false; }
        }

        public ActionResult Index(int? searchResponsable)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }

            MenuNavBarSelected(1);

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
                if (user.IdRolUsuario == CONSTANTES.ROL_RESPONSABLE)
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
            MenuNavBarSelected(5);
            UsuariosBL usuariosBL = new UsuariosBL();
            UsuarioDTO currentUser = getCurrentUser();
            return View(usuariosBL.getUsuariosTodos());//(CONSTANTES.ROL_RESPONSABLE));
        }

        public ActionResult Usuario(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MenuNavBarSelected(5);

            UsuarioDTO currentUser = getCurrentUser();

            UsuariosBL usuariosBL = new UsuariosBL();
            IList<RolDTO> roles = usuariosBL.getRolesCurrent(this.getCurrentUser().IdRolUsuario);
            IList<ClienteDTO> clientes = usuariosBL.getClientesBag(true);
            roles.Insert(0, new RolDTO() { IdRol = 0, Nombre = "Seleccione un Rol" });
            ViewBag.Roles = roles;
            ViewBag.Clientes = clientes;
            var objSent = TempData["Usuario"];
            if (objSent != null) { TempData["Usuario"] = null; return View(objSent); }
            if (id != null)
            {
                UsuarioDTO usuario = usuariosBL.getUsuario((int)id);
                //ViewBag.Roles = usuariosBL.getRolesCurrent(usuario.IdRolUsuario);
                ViewBag.Roles = usuariosBL.getRolesCurrent(currentUser.IdRolUsuario);
                return View(usuario);
            }
            return View();
        }

        public ActionResult AddUser(UsuarioDTO user, string passUser = "", string passChange = "")
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            UsuarioDTO currentUser = getCurrentUser();
            if (!this.isAdministrator() && user.IdUsuario != currentUser.IdUsuario) { createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_ROL_PERMISSION); return RedirectToAction("Usuarios"); }
            if (user.IdRolUsuario == 1 && !this.isSuperAdministrator()) { createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_ROL_PERMISSION); return RedirectToAction("Usuarios"); }
            if (user.IdRolUsuario == 2 && !this.isAdministrator()) { createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_ROL_PERMISSION); return RedirectToAction("Usuarios"); }
            if (currentUser.IdRolUsuario == 2 && user.IdRolUsuario == 2 && user.IdUsuario != currentUser.IdUsuario) { createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_ROL_PERMISSION); return RedirectToAction("Usuarios"); }
            if (currentUser.IdRolUsuario >= 3 && user.IdUsuario != currentUser.IdUsuario) { createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_ROL_PERMISSION); return RedirectToAction("Usuarios"); }
            try
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
                        user.RutaFirma = path;
                    }
                }
                UsuariosBL usuariosBL = new UsuariosBL();
                if (user.IdUsuario == 0 && usuariosBL.validateUsuario(user))
                {
                    usuariosBL.add(user);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Usuarios");
                }
                else if (user.IdUsuario != 0)
                {
                    if (usuariosBL.getUsuario(user.IdUsuario).IdRolUsuario == 1 && !this.isSuperAdministrator()) { createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_ROL_PERMISSION); return RedirectToAction("Usuarios"); }
                    if (usuariosBL.getUsuario(user.IdUsuario).IdRolUsuario == 2 && !this.isAdministrator()) { createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_ROL_PERMISSION); return RedirectToAction("Usuarios"); }

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
                        if (currentUser.IdRolUsuario <= 2)
                            createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE + "<br>Si está intentando actualizar una contraseña, verifique que conozca la <strong>actual contraseña del usuario a modificar</strong>.");
                        else
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
            MenuNavBarSelected(3);
            ClienteBL objBL = new ClienteBL();
            return View(objBL.getClientes());
        }

        public ActionResult Cliente(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MenuNavBarSelected(3);
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
            MenuNavBarSelected(3);

            ClienteBL objBL = new ClienteBL();
            PlantillaBL objPlantillaBL = new PlantillaBL();
            var dataPlantillas = objBL.getPlantillas(true);
            ViewBag.IdCliente = IdCliente;
            var objSent = (InmuebleDTO)TempData["Inmueble"];
            if (objSent != null)
            {
                TempData["Inmueble"] = null;
                ViewBag.Plantillas = removePlantillas(dataPlantillas, objSent.Plantillas);//ViewBag.Responsables = dataResponsables;
                ViewBag.Resources = objSent.Plantillas.AsEnumerable().Select(r => new PlantillaDTO { IdPlantilla = r.IdPlantilla, Nombre = objPlantillaBL.getPlantilla(r.IdPlantilla).Nombre }).ToList();
                return View(objSent);
            }
            if (id != null)
            {
                InmuebleDTO obj = objBL.getInmueble((int)id);
                ViewBag.Plantillas = removePlantillas(dataPlantillas, obj.Plantillas);
                ViewBag.Resources = obj.Plantillas;
                return View(obj);
            }
            else
            {
                ViewBag.Plantillas = dataPlantillas;
            }
            return View();
        }
        public List<PlantillaDTO> removePlantillas(IList<PlantillaDTO> AllPlantillas, List<PlantillaDTO> SeldPlantillas)
        {
            List<PlantillaDTO> plantillasLeft = new List<PlantillaDTO>();//.ToList();
            plantillasLeft = AllPlantillas.ToList();
            plantillasLeft.RemoveAll(a => SeldPlantillas.Exists(w => w.IdPlantilla > 0 && w.IdPlantilla == a.IdPlantilla));
            /*foreach (var plan in AllPlantillas)
                foreach (var planInm in SeldPlantillas)
                    if (plan.IdPlantilla > 0 && plan.IdPlantilla == planInm.IdPlantilla)
                        plantillasLeft.Remove(planInm);*/
            return plantillasLeft;
        }

        public ActionResult AddInmueble(InmuebleDTO dto, int[] plantillas)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MenuNavBarSelected(3);
            try
            {
                if (plantillas != null)
                    dto.Plantillas = plantillas.Select(x => new PlantillaDTO { IdPlantilla = x }).ToList();
                else
                    dto.Plantillas = new List<PlantillaDTO>();

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
            MenuNavBarSelected(3);
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
            MenuNavBarSelected(3);
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
            MenuNavBarSelected(4);
            ServiciosBL objBL = new ServiciosBL();
            return View(objBL.getServicios());
        }

        public ActionResult Servicio(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MenuNavBarSelected(4);
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
            MenuNavBarSelected(2);

            TareaBL objBL = new TareaBL();
            var model = objBL.getTareas();
            var responsables = objBL.getResponsables(true);
            responsables[0].Nombre = "Todos los responsables";
            ViewBag.Responsables = responsables;
            ViewBag.IdResponsable = searchResponsable ?? 0;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (searchResponsable != null && searchResponsable != 0)
                model = model.Where(x => x.Responsables.Any(y => y.IdUsuario == searchResponsable)).ToList();

            return View(model.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Tarea(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MenuNavBarSelected(2);

            TareaBL objBL = new TareaBL();
            var dataResponsables = objBL.getResponsables(true);
            ViewBag.Servicios = objBL.getServicios(true);
            ViewBag.Clientes = objBL.getComboClientes();
            ViewBag.Horas = new TareaDTO().fillHoras();
            ViewBag.Minutos = new TareaDTO().fillMinutos();
            ViewBag.Plantillas = objBL.getPlantillasBag(true);

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
            createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_LOGIN);
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

            MenuNavBarSelected(1);

            UsuariosBL usuariosBL = new UsuariosBL();
            var responsables = usuariosBL.getUsuarios(CONSTANTES.ROL_RESPONSABLE).OrderBy(x => x.IdUsuario).ToList();
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

        public ActionResult Protocolos(int? inmueble, int? periodo)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MenuNavBarSelected(6);

            List<ProtocoloDTO> model = new List<ProtocoloDTO>();
            ProtocoloBL objBL = new ProtocoloBL();
            int idUsuario = getCurrentUser().IdUsuario;
            ViewBag.Clientes = objBL.getClientesTarea(idUsuario);
            ////
            //ViewBag.IdInmueble = inmueble;
            if (inmueble == null)
            {
                ViewBag.IdInmueble = "";
                ViewBag.IdCliente = "";
                ViewBag.IdPeriodo = "";
            }
            else
            {

                ViewBag.IdInmueble = inmueble;
                ClienteBL oClienteBL = new ClienteBL();
                ViewBag.IdCliente = oClienteBL.getInmueble(inmueble.GetValueOrDefault()).IdCliente;
                ViewBag.IdPeriodo = periodo;
            }

            return View();
        }
        public ActionResult Protocolo_server(int idInmueble, int idPeriodo, int? idProtocolo = null, int? idPlantilla = null)
        {
            ViewBag.Horas = new BaseDTO().fillHoras();
            ViewBag.Minutos = new BaseDTO().fillMinutos();
            ViewBag.Items_SelectSINO = new BaseDTO().fillSelectSINO();
            ViewBag.Items_SelectBomba = new BaseDTO().fillSelectBomba();
            ViewBag.Items_SelectNivelTanque = new BaseDTO().fillSelectNivelTanque();
            ViewBag.Items_SelectAccesorios = new BaseDTO().fillSelectAccesorios();
            ViewBag.Items_SelectPresiones = new BaseDTO().fillSelectPresiones();
            ViewBag.Items_SelectControlMonitoreo = new BaseDTO().fillSelectControlMonitoreo();

            ProtocoloBL objBL = new ProtocoloBL();
            ProtocoloDTO obj = objBL.getProtocolo(idInmueble, idPeriodo, idProtocolo, idPlantilla);
            return View(obj);
        }
        public ActionResult Protocolo(int idInmueble, int idPeriodo)
        {
            MenuNavBarSelected(6);

            UsuariosBL oUsuariosBL = new UsuariosBL();
            var listaInspectores = oUsuariosBL.getUsuariosInspectores();
            var usuarioActual = getCurrentUser();
            listaInspectores.Insert(0, new UsuarioDTO { IdUsuario = 0, Nombre = "Seleccione" });
            //listaInspectores = listaInspectores.Where(x=>x.IdUsuario != usuarioActual.IdUsuario).ToList();

            ClienteBL oClienteBL = new ClienteBL();
            ViewBag.IdCliente = oClienteBL.getInmueble(idInmueble).IdCliente;
            ViewBag.IdInmueble = idInmueble;
            ViewBag.IdPeriodo = idPeriodo;

            OpcionRespuestaBL obj = new OpcionRespuestaBL();
            ViewBag.Horas = new BaseDTO().fillHoras().ToJSON();
            ViewBag.Minutos = new BaseDTO().fillMinutos().ToJSON();
            ViewBag.Items_SelectSINO = obj.getOpcionRespuesta(3).ToJSON();
            ViewBag.Items_SelectBomba = obj.getOpcionRespuesta(4).ToJSON();
            ViewBag.Items_SelectNivelTanque = obj.getOpcionRespuesta(5).ToJSON();
            ViewBag.Items_SelectAccesorios = obj.getOpcionRespuesta(6).ToJSON();
            ViewBag.Items_SelectPresiones = obj.getOpcionRespuesta(7).ToJSON();
            ViewBag.Items_SelectSINOonly = obj.getOpcionRespuesta(13).ToJSON();
            ViewBag.Items_SelectInspectores = listaInspectores.ToJSON();
            ViewBag.Items_SelectControlMonitoreo = obj.getOpcionRespuesta(16).ToJSON();

            return View();
        }

        public ActionResult AddProtocolo(ProtocoloDTO dto)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                ProtocoloBL objBL = new ProtocoloBL();
                //IdUsuario
                //dto.IdUsuario = getCurrentUser().IdUsuario;
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

        public ActionResult Periodos()
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            //if (!this.isAdministrator()) { return RedirectToAction("Index"); }
            MenuNavBarSelected(7);
            PeriodoBL periodoBL = new PeriodoBL();
            return View(periodoBL.getPeriodos());
        }

        public ActionResult Periodo(int? id = null)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            MenuNavBarSelected(7);

            PeriodoBL objBL = new PeriodoBL();
            ViewBag.IdPeriodo = id;
            var objSent = TempData["Periodo"];
            if (objSent != null) { TempData["Periodo"] = null; return View(objSent); }
            if (id != null)
            {
                PeriodoDTO obj = objBL.getPeriodo((int)id);
                return View(obj);
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddPeriodo(PeriodoDTO dto)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            try
            {
                PeriodoBL objBL = new PeriodoBL();
                if (dto.IdPeriodo == 0)
                {
                    objBL.add(dto);
                    createResponseMessage(CONSTANTES.SUCCESS);
                    return RedirectToAction("Periodos");
                }
                else if (dto.IdPeriodo != 0)
                {
                    if (objBL.update(dto))
                    {
                        createResponseMessage(CONSTANTES.SUCCESS);
                        return RedirectToAction("Periodos");
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
                if (dto.IdPeriodo != 0)
                    createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_UPDATE_MESSAGE);
                else createResponseMessage(CONSTANTES.ERROR, CONSTANTES.ERROR_INSERT_MESSAGE);
            }
            TempData["Periodo"] = dto;
            return RedirectToAction("Periodo");
        }

        #region APIS
        private MemoryStream CrearPDF(ProtocoloDTO protocolo)
        {
            MemoryStream ms = new MemoryStream();

            //Mis Fonts
            Font myFontTitle18 = FontFactory.GetFont("Open Sans", 16);
            Font myFontTitle18_B = FontFactory.GetFont("Open Sans", 16, Font.BOLD);
            Font myFontTitle15 = FontFactory.GetFont("Open Sans", 14);
            Font myFontTitle15_B = FontFactory.GetFont("Open Sans", 14, Font.BOLD);
            Font myFontTextH12 = FontFactory.GetFont("Open Sans", 12);
            Font myFontTextH12_B = FontFactory.GetFont("Open Sans", 12, Font.BOLD);
            Font myFontText10 = FontFactory.GetFont("Open Sans", 10);
            Font myFontText10_B = FontFactory.GetFont("Open Sans", 10, Font.BOLD);
            Font myFontText8 = FontFactory.GetFont("Open Sans", 8);
            Font myFontText8_B = FontFactory.GetFont("Open Sans", 8, Font.BOLD);
            Font myFontText8_blue = FontFactory.GetFont("Open Sans", 8, new BaseColor(66, 139, 202));
            Font myFontTextH12_blue = FontFactory.GetFont("Open Sans", 12, new BaseColor(66, 139, 202));

            //Numero de columnas
            int numColumns = 12;

            //Imagen
            string imagespath = Server.MapPath("/Content/themes/admin/images");
            string imgPath1 = imagespath + "/logo.png";
            iTextSharp.text.Image pic1 = iTextSharp.text.Image.GetInstance(imgPath1);

            //var grafica = Image.GetInstance(CrearGrafica());

            //Creación del PDF
            Document doc;
            bool protocoloANX = esProtocoloAnexo(protocolo.Plantilla.Nombre) != 0 ? true : false;

            if (protocoloANX)
            {
                doc = new Document(PageSize.A4.Rotate(), 20f, 20f, 20f, 20f);
                numColumns = esProtocoloAnexo(protocolo.Plantilla.Nombre);
            }
            else
            {
                doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
            }
            PdfWriter writer = PdfWriter.GetInstance(doc, ms);
            
            string miCodigo = (protocolo.Codigo != null) ? protocolo.Codigo : "N/A";
            writer.PageEvent = new ITextEvents() { Header = miCodigo };

            pic1.SetAbsolutePosition(doc.PageSize.Width - 155, doc.PageSize.Height - 55);

            //pic1.Alignment = Image.TEXTWRAP | Image.ALIGN_RIGHT;
            if (pic1.Height > pic1.Width)
            {
                //Maximum height is 800 pixels.
                float percentage = 0.0f;
                percentage = 175 / pic1.Height;
                pic1.ScalePercent(percentage * 100);
            }
            else
            {
                //Maximum width is 600 pixels.
                float percentage = 0.0f;
                percentage = 135 / pic1.Width;
                pic1.ScalePercent(percentage * 100);
            }

            doc.Open();
            //Paragraph CodigoProtocolo = new Paragraph(miCodigo, myFontTextH12);
            Paragraph CodigoProtocolo = new Paragraph("", myFontTextH12);
            CodigoProtocolo.Alignment = Element.ALIGN_JUSTIFIED;
            
            CodigoProtocolo.Add(pic1);
            doc.Add(CodigoProtocolo);

            Paragraph Titulo = new Paragraph();
            Titulo.SpacingBefore = 5;
            Titulo.Alignment = Element.ALIGN_JUSTIFIED;
            Titulo.Add(new Phrase(protocolo.Plantilla.Nombre, myFontTitle18_B));
            Titulo.SpacingAfter = 2;

            doc.Add(Titulo);
            
            Paragraph SubTitulo = new Paragraph(protocolo.Plantilla.Nombre2, myFontTitle15);
            SubTitulo.SpacingBefore = 7;
            //SubTitulo.IndentationLeft = 55;
            SubTitulo.Alignment = Element.ALIGN_CENTER;
            SubTitulo.SpacingAfter = 2;

            doc.Add(SubTitulo);

            //Cabecera del protocolo
            PdfPTable tableHeader = new PdfPTable(12);
            tableHeader.WidthPercentage = 100f;
            tableHeader.HorizontalAlignment = Element.ALIGN_JUSTIFIED_ALL;
            PdfPCell cellHeader = new PdfPCell();
            cellHeader.Colspan = 6;
            cellHeader.Phrase = new Phrase("Nombre del Área Protegida:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase("Fecha:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase(protocolo.NombreAreaProtegida, myFontTextH12_blue);
            tableHeader.AddCell(cellHeader);
            var Fecha = protocolo.Fecha != null ? protocolo.Fecha.GetValueOrDefault().ToString("dd/MM/yyyy") : null;
            cellHeader.Phrase = new Phrase(Fecha, myFontTextH12_blue);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase("Dirección:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase("Hora de Inicio:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase(protocolo.Direccion, myFontTextH12_blue);
            tableHeader.AddCell(cellHeader);
            cellHeader.Phrase = new Phrase(protocolo.HoraInicio + ":" + protocolo.MinutoInicio, myFontTextH12_blue);
            tableHeader.AddCell(cellHeader);

            cellHeader.Phrase = new Phrase("Tipo de prueba:", myFontTextH12_B);
            tableHeader.AddCell(cellHeader);
            string miTipoPrueba = "No se asigno el tipo de prueba";
            if(protocolo.TipoPrueba == "1")
            {
                miTipoPrueba = "Recepción del sistema";
            }
            else if(protocolo.TipoPrueba == "2")
            {
                miTipoPrueba = "Pruebas periódicas anuales";
            }

            cellHeader.Phrase = new Phrase(miTipoPrueba, myFontTextH12_blue);
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
            List<OpcionDTO> opciones13 = (List<OpcionDTO>)obj.getOpcionRespuesta(13);
            List<OpcionDTO> opciones16 = (List<OpcionDTO>)obj.getOpcionRespuesta(16);

            //Inicio Tabla Reporte
            foreach (var Seccion in protocolo.Secciones)
            {
                //Validacion del tamaño de tabla de los ANEXOS
                //numColumns = numeroFilasEnSeccion(Seccion.Nombre);

                if (!protocoloANX)
                {
                    int esAnexo = numeroFilasEnSeccion(Seccion.Nombre);
                    numColumns = esAnexo != 0 ? esAnexo : 12;
                    if (esAnexo != 0)
                    {
                        doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        doc.SetMargins(20f, 20f, 20f, 20f);
                        doc.NewPage();
                    }
                    else
                    {
                        doc.SetPageSize(PageSize.A4);
                        doc.SetMargins(20f, 20f, 20f, 20f);
                    }
                }

                PdfPTable tableSeccion = new PdfPTable(numColumns);
                tableSeccion.WidthPercentage = 100f;
                tableSeccion.HorizontalAlignment = Element.ALIGN_JUSTIFIED_ALL;
                //tableSeccion.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                PdfPCell cellSeccion = new PdfPCell();
                cellSeccion.Colspan = numColumns;
                cellSeccion.Phrase = new Phrase(Seccion.Nombre, myFontText10_B);
                tableSeccion.AddCell(cellSeccion);

                //Contenido de secciones
                foreach (var SeccionBody in Seccion.SeccionBodys)
                {
                    PdfPCell cellSeccionBody = new PdfPCell();
                    cellSeccionBody.Rowspan = SeccionBody.Rowspan;
                    cellSeccionBody.Colspan = SeccionBody.Colspan;

                    if (SeccionBody.IdTipoCelda == 1)
                    {
                        cellSeccionBody.Phrase = new Phrase(SeccionBody.Descripcion, myFontText8);
                    }
                    else
                    {
                        string rpta = "";
                        List<OpcionDTO> auxOpc = new List<OpcionDTO>();
                        switch (SeccionBody.IdTipoTag)
                        {
                            case 2: auxOpc = null; break;
                            case 3: auxOpc = opciones3; break;
                            case 4: auxOpc = opciones4; break;
                            case 5: auxOpc = opciones5; break;
                            case 6: auxOpc = opciones6; break;
                            case 7: auxOpc = opciones7; break;
                            case 13: auxOpc = opciones13; break;
                            case 16: auxOpc = opciones16; break;
                            default: auxOpc = null; break;
                        }
                        //NO HAY CASE 2 PORQUE ESTA CONTEMPLADO EN "ELSE" DE ABAJO
                        if (auxOpc != null)
                            foreach (var itemAux in auxOpc)
                            {
                                if (itemAux.IdOpcion == Convert.ToInt32(SeccionBody.Respuesta))
                                    rpta = itemAux.NombreOpcion;
                            }
                        else
                            rpta = SeccionBody.Respuesta;

                        cellSeccionBody.Phrase = new Phrase(rpta, myFontText8_blue);
                    }
                    tableSeccion.AddCell(cellSeccionBody);
                }

                bool ultimaSeccionGrafica = false;
                foreach (var SubSeccion in Seccion.SubSecciones)
                {
                    //Validacion de la plantilla para imprimir la grafica posteriormente
                    if (protocolo.IdPlantilla == 4 || protocolo.IdPlantilla == 5)
                    {
                        if (SubSeccion == Seccion.SubSecciones.Last() && Seccion.Nombre == "RESULTADOS DE LA PRUEBA DE LA BOMBA CONTRA INCENDIO")
                        { 
                            ultimaSeccionGrafica = true;
                        }
                    }
                    PdfPCell cellSubSeccion = new PdfPCell();
                    cellSubSeccion.Colspan = numColumns;
                    cellSubSeccion.Phrase = new Phrase(SubSeccion.Nombre, myFontText10_B);
                    tableSeccion.AddCell(cellSubSeccion);

                    //Contenido de secciones
                    foreach (var SubSeccionBody in SubSeccion.SeccionBodys)
                    {
                        PdfPCell cellSubSeccionBody = new PdfPCell();
                        cellSubSeccionBody.Rowspan = SubSeccionBody.Rowspan;
                        cellSubSeccionBody.Colspan = SubSeccionBody.Colspan;
                        if (SubSeccionBody.IdTipoCelda == 1)
                        {
                            cellSubSeccionBody.Phrase = new Phrase(SubSeccionBody.Descripcion, myFontText8);
                        }
                        else
                        {
                            string rpta = "";
                            List<OpcionDTO> auxOpc = new List<OpcionDTO>();
                            switch (SubSeccionBody.IdTipoTag)
                            {
                                case 2: auxOpc = null; break;
                                case 3: auxOpc = opciones3; break;
                                case 4: auxOpc = opciones4; break;
                                case 5: auxOpc = opciones5; break;
                                case 6: auxOpc = opciones6; break;
                                case 7: auxOpc = opciones7; break;
                                case 13: auxOpc = opciones13; break;
                                case 16: auxOpc = opciones16; break;
                                default: auxOpc = null; break;
                            }
                            //NO HAY CASE 2 PORQUE ESTA CONTEMPLADO EN "ELSE" DE ABAJO
                            if (auxOpc != null)
                                foreach (var itemAux in auxOpc)
                                {
                                    if (itemAux.IdOpcion == Convert.ToInt32(SubSeccionBody.Respuesta))
                                        rpta = itemAux.NombreOpcion;
                                }
                            else
                                rpta = SubSeccionBody.Respuesta;

                            cellSubSeccionBody.Phrase = new Phrase(rpta, myFontText8_blue);
                        }
                        tableSeccion.AddCell(cellSubSeccionBody);
                    }

                    if (ultimaSeccionGrafica)
                    {
                        var image = Image.GetInstance(CrearGrafica(protocolo));
                        //image.Alignment = Element.ALIGN_CENTER;
                        image.ScalePercent(95f);
                        //image.SpacingBefore = 10f;
                        //image.SpacingAfter = 10f;
                        PdfPCell imageCell = new PdfPCell(image);
                        imageCell.Rowspan = 1;
                        imageCell.Colspan = 12;
                        imageCell.Padding = 1f;
                        //imageCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        //imageCell.VerticalAlignment = Element.ALIGN_MIDDLE;


                        //imageCell.Rowspan = 5;
                        //imageCell.Colspan = 12;
                        tableSeccion.AddCell(imageCell);
                    }
                }
                doc.Add(tableSeccion);
                doc.Add(new Paragraph(" "));

                //Grafica
                
            }

            doc.Close();
            return ms;
        }

        private int esProtocoloAnexo(string NombreProtocolo)
        {
            switch (NombreProtocolo)
            {
                //NFPA 10 - Completo
                case "NFPA 10":
                    return 40;
                //NFPA 101 - Completo
                case "NFPA 101":
                    return 12;
                //CCTV - Completo
                case "CCTV":
                    return 32;
                //ANX de Intrusion - Completo
                case "ANEXO DE INTRUSIÓN":
                    return 19;
                //ANX Control de Acceso - Completo
                case "ANEXO DE CONTROL DE ACCESO":
                    return 12;
                //INSPECCIÓN DE LECTORAS - Completo
                case "INSPECCIÓN DE LECTORAS":
                    return 12;
                //NFPA 80 - Completo
                case "NFPA 80":
                    return 12;
                //RNE - Completo
                case "RNE":
                    return 18;
                default:
                    return 0;
            }
        }

        private int numeroFilasEnSeccion(string NombreSeccion)
        {
            switch (NombreSeccion)
            {
                //NFPA 14
                case "ANEXO - HOJA DE INSPECCIÓN DE GABINETES CONTRA INCENDIOS":
                    return 39;
                //NFPA 24
                case "ANEXO - LISTADO DE CASETAS DE ATAQUE RÁPIDO (CAR)":
                    return 36;
                case "ANEXO - INSPECCION DE MONITORES":
                    return 28;
                //NFPA 72
                case "ANEXO - RELACIÓN DE DIPOSITIVOS PROBADOS":
                    return 35;
                //NFPA 11
                case "ANEXO - PRUEBA DEL SISTEMA DE MONITOR AGUA-ESPUMA":
                    return 12;
                //NFPA 2001
                case "ANEXO - LISTADO DE DISPOSITIVOS DE SISTEMA DE DETECCIÓN Y EXTINCIÓN POR AGENTES LIMPIOS":
                    return 12;
                default:
                    return 0;
            }
        }

        private bool siTieneGraficaElProtocoloOSeccion(string nombre)
        {
            switch(nombre)
            {
                case "NFPA 20 - ELECTROBOMBA":
                    return true;
                case "NFPA 20 - MOTOBOMBA":
                    return true;
                case "RESULTADOS DE LA PRUEBA DE LA BOMBA CONTRA INCENDIO":
                    return true;
                default:
                    return false;
            }

        }

        private Byte[] CrearGrafica(ProtocoloDTO protocolo)
        {
            Chart chart = new Chart();

            switch(protocolo.IdPlantilla)
            {
                case 4:
                    chart = CrearGraficaElectrobomba(protocolo);
                    break;
                case 5:
                    chart = CrearGraficaMotobomba(protocolo);
                    break;
            }

            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, System.Web.UI.DataVisualization.Charting.ChartImageFormat.Png);
                return chartimage.GetBuffer();
            }
        }

        private Chart CrearGraficaElectrobomba(ProtocoloDTO protocolo)
        {
            Chart chart = new Chart { Width = 580, Height = 280 };

            var chartArea = new ChartArea();

            chartArea.AxisX.Enabled = AxisEnabled.True;
            chartArea.AxisY.Enabled = AxisEnabled.True;
            chartArea.AxisY2.Enabled = AxisEnabled.True;

            chartArea.AxisX.IsMarginVisible = false;
            chartArea.AxisY.IsMarginVisible = false;
            chartArea.AxisY2.IsMarginVisible = false;

            /*chartArea.AxisX.Minimum = 0;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY2.Minimum = 0;*/

            chartArea.AxisX2.LabelStyle.Enabled = true;

            chartArea.AxisX.Title = "Caudal (GPM)";
            chartArea.AxisY.Title = "Potencia (BHP)";
            chartArea.AxisY2.Title = "Presión (PSI)";

            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY2.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            chartArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
            chartArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
            chartArea.AxisY2.LabelAutoFitStyle = LabelAutoFitStyles.None;

            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8f);
            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8f);
            chartArea.AxisY2.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8f);

            //chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;
            chartArea.BackColor = System.Drawing.Color.White;
            chart.ChartAreas.Add(chartArea);

            //Ubicar seccion
            string busSeccion = "RESULTADOS DE LA PRUEBA DE LA BOMBA CONTRA INCENDIO";
            SeccionDTO miSeccion = protocolo.Secciones.Single(x => x.Nombre == busSeccion);

            Double temp;
            List<Double> datosGrafica = new List<Double>();

            Double numTemp;
            //Obtencion de datos para imprimir la grafica
            foreach(var subSeccion in miSeccion.SubSecciones)
            {
                foreach(var SubSeccionBody in subSeccion.SeccionBodys)
                {
                    if(SubSeccionBody.IdTipoTag == 2)
                    {
                        if (Double.TryParse(SubSeccionBody.Respuesta, out temp))
                        {
                            numTemp = Double.Parse(SubSeccionBody.Respuesta, CultureInfo.InvariantCulture);
                            if (!Double.IsNaN(numTemp))
                            {
                                datosGrafica.Add(numTemp);
                            }
                            else
                            {
                                datosGrafica.Add(0);
                            }
                        }
                        else
                        {
                            datosGrafica.Add(0);
                        }
                    }
                }
            }

            Series line1 = new Series("Curva de Fábrica del Motor");
            Series line2 = new Series("Curva de Prueba del Motor");
            Series line3 = new Series("Curva de Prueba de la Bomba");
            Series line4 = new Series("Curva de Fábrica de la Bomba");

            line1.ChartType = SeriesChartType.Line;
            line2.ChartType = SeriesChartType.Line;
            line3.ChartType = SeriesChartType.Line;
            line4.ChartType = SeriesChartType.Line;

            line1.Color = System.Drawing.Color.FromArgb(153, 0, 255);
            line2.Color = System.Drawing.Color.FromArgb(109, 158, 235);
            line3.Color = System.Drawing.Color.FromArgb(204, 0, 0);
            line4.Color = System.Drawing.Color.FromArgb(147, 196, 125);

            line1.Font = new System.Drawing.Font("Segoe UI", 7f);
            line2.Font = new System.Drawing.Font("Segoe UI", 7f);
            line3.Font = new System.Drawing.Font("Segoe UI", 7f);
            line4.Font = new System.Drawing.Font("Segoe UI", 7f);

            if (datosGrafica[79] != 0 || datosGrafica[89] != 0) line1.Points.Add(new DataPoint(datosGrafica[79], datosGrafica[89]));
            if (datosGrafica[80] != 0 || datosGrafica[90] != 0) line1.Points.Add(new DataPoint(datosGrafica[80], datosGrafica[90]));
            if (datosGrafica[81] != 0 || datosGrafica[91] != 0) line1.Points.Add(new DataPoint(datosGrafica[81], datosGrafica[91]));
            if (datosGrafica[82] != 0 || datosGrafica[92] != 0) line1.Points.Add(new DataPoint(datosGrafica[82], datosGrafica[92]));
            if (datosGrafica[83] != 0 || datosGrafica[93] != 0) line1.Points.Add(new DataPoint(datosGrafica[83], datosGrafica[93]));

            if (datosGrafica[53] != 0 || datosGrafica[8] != 0) line2.Points.Add(new DataPoint(datosGrafica[53], datosGrafica[8]));
            if (datosGrafica[61] != 0 || datosGrafica[16] != 0) line2.Points.Add(new DataPoint(datosGrafica[61], datosGrafica[16]));
            if (datosGrafica[69] != 0 || datosGrafica[24] != 0) line2.Points.Add(new DataPoint(datosGrafica[69], datosGrafica[24]));
            if (datosGrafica[77] != 0 || datosGrafica[32] != 0) line2.Points.Add(new DataPoint(datosGrafica[77], datosGrafica[32]));

            if (datosGrafica[53] != 0 || datosGrafica[54] != 0) line3.Points.Add(new DataPoint(datosGrafica[53], datosGrafica[54]));
            if (datosGrafica[61] != 0 || datosGrafica[62] != 0) line3.Points.Add(new DataPoint(datosGrafica[61], datosGrafica[62]));
            if (datosGrafica[69] != 0 || datosGrafica[70] != 0) line3.Points.Add(new DataPoint(datosGrafica[69], datosGrafica[70]));
            if (datosGrafica[77] != 0 || datosGrafica[78] != 0) line3.Points.Add(new DataPoint(datosGrafica[77], datosGrafica[78]));

            if (datosGrafica[79] != 0 || datosGrafica[84] != 0) line4.Points.Add(new DataPoint(datosGrafica[79], datosGrafica[84]));
            if (datosGrafica[80] != 0 || datosGrafica[85] != 0) line4.Points.Add(new DataPoint(datosGrafica[80], datosGrafica[85]));
            if (datosGrafica[81] != 0 || datosGrafica[86] != 0) line4.Points.Add(new DataPoint(datosGrafica[81], datosGrafica[86]));
            if (datosGrafica[82] != 0 || datosGrafica[87] != 0) line4.Points.Add(new DataPoint(datosGrafica[82], datosGrafica[87]));
            if (datosGrafica[83] != 0 || datosGrafica[88] != 0) line4.Points.Add(new DataPoint(datosGrafica[83], datosGrafica[88]));

            foreach (var point in line1.Points)
            {
                point.Label = "(" + point.XValue.ToString() + ", " + point.YValues[0].ToString() + ")";
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 7;
            }
            foreach (var point in line2.Points)
            {
                point.Label = "(" + point.XValue.ToString() + ", " + point.YValues[0].ToString() + ")";
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 7;
            }
            foreach (var point in line3.Points)
            {
                point.Label = "(" + point.XValue.ToString() + ", " + point.YValues[0].ToString() + ")";
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 7;
            }
            foreach (var point in line4.Points)
            {
                point.Label = "(" + point.XValue.ToString() + ", " + point.YValues[0].ToString() + ")";
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 7;
            }

            line3.YAxisType = AxisType.Secondary;
            line4.YAxisType = AxisType.Secondary;

            chart.Series.Add(line1);
            chart.Series.Add(line2);
            chart.Series.Add(line3);
            chart.Series.Add(line4);

            //Leyendas
            Legend legend = new Legend();
            legend.Name = "Leyenda";
            legend.Alignment = System.Drawing.StringAlignment.Center;
            legend.Docking = Docking.Bottom;

            chart.Legends.Add(legend);

            return chart;
        }
        private Chart CrearGraficaMotobomba(ProtocoloDTO protocolo)
        {
            Chart chart = new Chart { Width = 580, Height = 280 };

            var chartArea = new ChartArea();

            chartArea.AxisX.Enabled = AxisEnabled.True;
            chartArea.AxisY.Enabled = AxisEnabled.True;

            chartArea.AxisX.IsMarginVisible = false;
            chartArea.AxisY.IsMarginVisible = false;

            chartArea.AxisX.Title = "Caudal (GPM)";
            chartArea.AxisY.Title = "Presión (PSI)";

            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            chartArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
            chartArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;

            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8f);
            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Segoe UI", 8f);

            chartArea.BackColor = System.Drawing.Color.White;
            chart.ChartAreas.Add(chartArea);

            //Ubicar seccion
            string busSeccion = "RESULTADOS DE LA PRUEBA DE LA BOMBA CONTRA INCENDIO";
            SeccionDTO miSeccion = protocolo.Secciones.Single(x => x.Nombre == busSeccion);

            Double temp;
            List<Double> datosGrafica = new List<Double>();

            Double numTemp;
            //Obtencion de datos para imprimir la grafica
            foreach (var subSeccion in miSeccion.SubSecciones)
            {
                foreach (var SubSeccionBody in subSeccion.SeccionBodys)
                {
                    if (SubSeccionBody.IdTipoTag == 2)
                    {
                        if (Double.TryParse(SubSeccionBody.Respuesta, out temp))
                        {
                            numTemp = Double.Parse(SubSeccionBody.Respuesta, CultureInfo.InvariantCulture);
                            if(!Double.IsNaN(numTemp))
                            { 
                                datosGrafica.Add(numTemp);
                            }
                            else
                            {
                                datosGrafica.Add(0);
                            }
                        }
                        else
                        {
                            datosGrafica.Add(0);
                        }
                    }
                }
            }

            Series line1 = new Series("Curva de Prueba de la Bomba");
            Series line2 = new Series("Curva de Fábrica de la Bomba");
            Series line3 = new Series("Curva de Prueba de la Bomba 2");

            line1.ChartType = SeriesChartType.Line;
            line2.ChartType = SeriesChartType.Line;
            line3.ChartType = SeriesChartType.Line;
            //Color de Lineas
            line1.Color = System.Drawing.Color.FromArgb(254, 46, 46);
            line2.Color = System.Drawing.Color.FromArgb(103, 131, 183);
            line3.Color = System.Drawing.Color.FromArgb(130, 88, 250);
            //Tipo de fuente de los puntos
            line1.Font = new System.Drawing.Font("Segoe UI", 7f);
            line2.Font = new System.Drawing.Font("Segoe UI", 7f);
            line3.Font = new System.Drawing.Font("Segoe UI", 7f);

            if (datosGrafica[22] != 0 || datosGrafica[23] != 0) line1.Points.Add(new DataPoint(datosGrafica[22], datosGrafica[23]));
            if (datosGrafica[30] != 0 || datosGrafica[31] != 0) line1.Points.Add(new DataPoint(datosGrafica[30], datosGrafica[31]));
            if (datosGrafica[38] != 0 || datosGrafica[39] != 0) line1.Points.Add(new DataPoint(datosGrafica[38], datosGrafica[39]));
            if (datosGrafica[46] != 0 || datosGrafica[47] != 0) line1.Points.Add(new DataPoint(datosGrafica[46], datosGrafica[47]));

            if (datosGrafica[6] != 0 || datosGrafica[11] != 0) line2.Points.Add(new DataPoint(datosGrafica[6], datosGrafica[11]));
            if (datosGrafica[7] != 0 || datosGrafica[12] != 0) line2.Points.Add(new DataPoint(datosGrafica[7], datosGrafica[12]));
            if (datosGrafica[8] != 0 || datosGrafica[13] != 0) line2.Points.Add(new DataPoint(datosGrafica[8], datosGrafica[13]));
            if (datosGrafica[9] != 0 || datosGrafica[14] != 0) line2.Points.Add(new DataPoint(datosGrafica[9], datosGrafica[14]));
            if (datosGrafica[10] != 0 || datosGrafica[15] != 0) line2.Points.Add(new DataPoint(datosGrafica[10], datosGrafica[15]));

            if (datosGrafica[57] != 0 || datosGrafica[58] != 0) line3.Points.Add(new DataPoint(datosGrafica[57], datosGrafica[58]));
            if (datosGrafica[65] != 0 || datosGrafica[66] != 0) line3.Points.Add(new DataPoint(datosGrafica[65], datosGrafica[66]));
            if (datosGrafica[73] != 0 || datosGrafica[74] != 0) line3.Points.Add(new DataPoint(datosGrafica[73], datosGrafica[74]));
            if (datosGrafica[81] != 0 || datosGrafica[82] != 0) line3.Points.Add(new DataPoint(datosGrafica[81], datosGrafica[82]));

            foreach (var point in line1.Points)
            {
                point.Label = "(" + point.XValue.ToString() + ", " + point.YValues[0].ToString() + ")";
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 7;
            }
            foreach (var point in line2.Points)
            {
                point.Label = "(" + point.XValue.ToString() + ", " + point.YValues[0].ToString() + ")";
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 7;
            }
            foreach (var point in line3.Points)
            {
                point.Label = "(" + point.XValue.ToString() + ", " + point.YValues[0].ToString() + ")";
                point.MarkerStyle = MarkerStyle.Circle;
                point.MarkerSize = 7;
            }

            chart.Series.Add(line1);
            chart.Series.Add(line2);
            chart.Series.Add(line3);

            //Leyendas
            Legend legend = new Legend();
            legend.Name = "Leyenda";
            legend.Alignment = System.Drawing.StringAlignment.Center;
            legend.Docking = Docking.Bottom;

            chart.Legends.Add(legend);

            return chart;
        }

        [HttpGet]
        public ActionResult GetUsuario(int id)
        {
            UsuariosBL objBL = new UsuariosBL();
            var lista = objBL.getUsuario(id);
            return Json(lista, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveProtocolo(string protocolo)
        {
            if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            var objProtocolo = new JavaScriptSerializer().Deserialize<ProtocoloDTO>(protocolo);
            //IdUsuario
            //objProtocolo.IdUsuario = getCurrentUser().IdUsuario;
            if (!string.IsNullOrEmpty(objProtocolo.StrFecha)) objProtocolo.Fecha = Convert.ToDateTime(objProtocolo.StrFecha);
            ProtocoloBL objBL = new ProtocoloBL();
            bool response = false;
            if (objProtocolo.IdProtocolo == 0) response = objBL.add(objProtocolo);
            else response = objBL.update(objProtocolo);
            return Json(response, JsonRequestBehavior.AllowGet);
            //return Json(new { Response = response }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetProtocolos(int inmueble, int periodo)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ProtocoloBL objBL = new ProtocoloBL();
            int idUsuario = getCurrentUser().IdUsuario;
            var model = objBL.getProtocolos(idUsuario, inmueble, periodo);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetProtocolo(int idInmueble, int idPeriodo, int? idProtocolo = null, int? idPlantilla = null)
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            ProtocoloBL objBL = new ProtocoloBL();
            //int idUsuario = getCurrentUser().IdUsuario;
            var model = objBL.getProtocolo(idInmueble, idPeriodo, idProtocolo, idPlantilla);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPeriodos()
        {
            PeriodoBL perioBL = new PeriodoBL();

            var periodos = perioBL.getPeriodos();
            return Json(periodos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerarPdfProtocolo(int idInmueble, int idPeriodo, int? idProtocolo = null, int? idPlantilla = null)
        {
            ProtocoloBL objBL = new ProtocoloBL();
            //int idUsuario = getCurrentUser().IdUsuario;
            var model = objBL.getProtocolo(idInmueble, idPeriodo, idProtocolo, idPlantilla);
            var ms = CrearPDF(model);
            byte[] file = ms.ToArray();
            MemoryStream output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;
            HttpContext.Response.AppendHeader("Content-Disposition", "inline; filename=" + model.Plantilla.Nombre + ".pdf");//HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + model.Plantilla.Nombre);
            return File(output, "application/pdf"); //new FileStreamResult(output, "application/pdf");
        }

        //[HttpGet]
        //public ActionResult GenerarPdfProtocolo(int idInmueble, int? idProtocolo = null, int? idPlantilla = null)
        //{
        //    ProtocoloBL objBL = new ProtocoloBL();
        //    //int idUsuario = getCurrentUser().IdUsuario;
        //    var model = objBL.getProtocolo(idInmueble, idProtocolo, idPlantilla);
        //    CrearPDF(model);
        //    HttpContext.Response.AppendHeader("Content-Disposition", "inline; filename=" + "Doc1.pdf");
        //    return Json(true, JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet]
        //public ActionResult GetProtocoloReporte(int idInmueble, int? idProtocolo = null, int? idPlantilla = null)
        //{
        //    ProtocoloBL objBL = new ProtocoloBL();
        //    //int idUsuario = getCurrentUser().IdUsuario;
        //    var model = objBL.getProtocolo(idInmueble, idProtocolo, idPlantilla);
        //    CrearPDF((ProtocoloDTO)model);

        //    return Json(model, JsonRequestBehavior.AllowGet);
        //}

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
        public ActionResult GetPlantillas()
        {
            //if (!this.currentUser()) { return RedirectToAction("Ingresar"); }
            TareaBL objBL = new TareaBL();
            return Json(objBL.getPlantillasBag(true), JsonRequestBehavior.AllowGet);
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

        public void MenuNavBarSelected(int num)
        {
            navbar.clearAll();
            switch (num)
            {
                case 1:
                    navbar.dangerActive = "active";
                    break;
                case 2:
                    navbar.warningActive = "active";
                    break;
                case 3:
                    navbar.infoActive = "active";
                    break;
                case 4:
                    navbar.successActive = "active";
                    break;
                case 5:
                    navbar.warningActive2 = "active";
                    break;
                case 6:
                    navbar.primaryActive = "active";
                    break;
                case 7:
                    navbar.successActive2 = "active";
                    break;
            }

            ViewBag.navbar = navbar;
        }
    }

    public class ITextEvents : PdfPageEventHelper
    {

        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;


        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        #endregion


        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                cb = writer.DirectContent;
                headerTemplate = cb.CreateTemplate(100, 100);
                footerTemplate = cb.CreateTemplate(50, 50);
            }
            catch (DocumentException de)
            {

            }
            catch (System.IO.IOException ioe)
            {

            }
        }

        public override void OnEndPage(iTextSharp.text.pdf.PdfWriter writer, iTextSharp.text.Document document)
        {
            base.OnEndPage(writer, document);

            iTextSharp.text.Font baseFontNormal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);

            iTextSharp.text.Font baseFontBig = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.BLACK);

            //Phrase p1Header = new Phrase("Sample Header Here", baseFontNormal);
            Phrase p1Header = new Phrase(Header, baseFontNormal);

            //Create PdfTable object
            PdfPTable pdfTab = new PdfPTable(3);

            //We will have to create separate cells to include image logo and 2 separate strings
            //Row 1
            PdfPCell pdfCell1 = new PdfPCell(p1Header);
            PdfPCell pdfCell2 = new PdfPCell();
            PdfPCell pdfCell3 = new PdfPCell();
            //String text = "Page " + writer.PageNumber + " of ";
            String text = writer.PageNumber.ToString();
            //String text = "";

            //Add paging to header
            {
                cb.BeginText();
                cb.SetFontAndSize(bf, 10);
                //cb.SetTextMatrix(document.PageSize.GetRight(20), document.PageSize.GetTop(11));
                //cb.ShowText(text);
                cb.EndText();
                //float len = bf.GetWidthPoint(text, 10);
                //Adds "12" in Page 1 of 12
                //cb.AddTemplate(headerTemplate, document.PageSize.GetRight(100) + len, document.PageSize.GetTop(11));
            }
            //Add paging to footer
            {
                cb.BeginText();
                cb.SetFontAndSize(bf, 10);
                cb.SetTextMatrix(document.PageSize.GetRight(30), document.PageSize.GetBottom(11));
                cb.ShowText(text);
                cb.EndText();
                //float len = bf.GetWidthPoint(text, 10);
                //cb.AddTemplate(footerTemplate, document.PageSize.GetRight(180) + len, document.PageSize.GetBottom(11));
            }
            //Row 2
            //PdfPCell pdfCell4 = new PdfPCell(new Phrase("Sub Header Description", baseFontNormal));
            //Row 3


            //PdfPCell pdfCell5 = new PdfPCell(new Phrase("Date:" + PrintTime.ToShortDateString(), baseFontBig));
            //PdfPCell pdfCell6 = new PdfPCell();
            //PdfPCell pdfCell7 = new PdfPCell(new Phrase("TIME:" + string.Format("{0:t}", DateTime.Now), baseFontBig));


            //set the alignment of all three cells and set border to 0
            pdfCell1.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell2.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfCell3.HorizontalAlignment = Element.ALIGN_LEFT;
            /*pdfCell4.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell5.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell6.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell7.HorizontalAlignment = Element.ALIGN_CENTER;*/


            //pdfCell2.VerticalAlignment = Element.ALIGN_BOTTOM;
            pdfCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
            /*pdfCell4.VerticalAlignment = Element.ALIGN_TOP;
            pdfCell5.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell6.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell7.VerticalAlignment = Element.ALIGN_MIDDLE;*/


            //pdfCell4.Colspan = 3;



            pdfCell1.Border = 0;
            pdfCell2.Border = 0;
            pdfCell3.Border = 0;
            /*pdfCell4.Border = 0;
            pdfCell5.Border = 0;
            pdfCell6.Border = 0;
            pdfCell7.Border = 0;*/

            pdfCell1.Colspan = 1;
            pdfCell2.Colspan = 1;
            pdfCell3.Colspan = 1;


            //add all three cells into PdfTable
            pdfTab.AddCell(pdfCell1);
            pdfTab.AddCell(pdfCell2);
            pdfTab.AddCell(pdfCell3);
            /*pdfTab.AddCell(pdfCell4);
            pdfTab.AddCell(pdfCell5);
            pdfTab.AddCell(pdfCell6);
            pdfTab.AddCell(pdfCell7);*/

            pdfTab.TotalWidth = document.PageSize.Width - 10;
            pdfTab.WidthPercentage = 90f;
            pdfTab.HorizontalAlignment = Element.ALIGN_JUSTIFIED_ALL;
            //pdfTab.HorizontalAlignment = Element.ALIGN_CENTER;


            //call WriteSelectedRows of PdfTable. This writes rows from PdfWriter in PdfTable
            //first param is start row. -1 indicates there is no end row and all the rows to be included to write
            //Third and fourth param is x and y position to start writing
            pdfTab.WriteSelectedRows(0, -1, 20, document.PageSize.Height - 4, writer.DirectContent);
            //set pdfContent value

            //Move the pointer and draw line to separate header section from rest of page
            /*cb.MoveTo(40, document.PageSize.Height - 100);
            cb.LineTo(document.PageSize.Width - 40, document.PageSize.Height - 100);
            cb.Stroke();*/

            //Move the pointer and draw line to separate footer section from rest of page
            /*cb.MoveTo(40, document.PageSize.GetBottom(50));
            cb.LineTo(document.PageSize.Width - 40, document.PageSize.GetBottom(50));
            cb.Stroke();*/
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            headerTemplate.BeginText();
            headerTemplate.SetFontAndSize(bf, 10);
            headerTemplate.SetTextMatrix(0, 0);
            headerTemplate.ShowText((writer.PageNumber - 1).ToString());
            headerTemplate.EndText();

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, 10);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText((writer.PageNumber - 1).ToString());
            footerTemplate.EndText();


        }
    }
}
