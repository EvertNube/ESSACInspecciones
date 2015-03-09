using ESSACInspecciones.Core.DTO;
using ESSACInspecciones.Data;
using ESSACInspecciones.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.BL
{
    public class UsuariosBL : Base
    {
        public IList<UsuarioDTO> getUsuariosInspectores()
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.IdRol == 3
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Pass = r.Pass,
                                 Active = r.Estado,
                                 IdRolUsuario = r.IdRol,
                                 RutaFirma = r.RutaFirma
                             };
                return result.AsEnumerable<UsuarioDTO>().OrderByDescending(x => x.IdUsuario).ToList<UsuarioDTO>();
            }
        }
        public IList<UsuarioDTO> getUsuarios()
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.IdRol != CONSTANTES.SUPER_ADMIN_ROL
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Pass = r.Pass,
                                 Active = r.Estado,
                                 IdRolUsuario = r.IdRol //?? 0
                             };
                return result.AsEnumerable<UsuarioDTO>().OrderByDescending(x => x.IdUsuario).ToList<UsuarioDTO>();
            }
        }
        public IList<UsuarioDTO> getUsuarios(int IdRol)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario.AsEnumerable()
                             where getRoleKeys(IdRol).Contains(r.IdRol)//& r.IdRol != CONSTANTES.SUPER_ADMIN_ROL & r.Estado == true
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Active = r.Estado,
                                 IdRolUsuario = r.IdRol //?? 0
                             };
                return result.ToList<UsuarioDTO>();//.AsEnumerable<UsuarioDTO>().OrderByDescending(x => x.Nombre).ToList<UsuarioDTO>();
            }
        }

        public IList<UsuarioDTO> getUsuariosTodos()
        {
            using(var context = getContext())
            {
                var result = from r in context.Usuario.AsEnumerable()
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Active = r.Estado,
                                 IdRolUsuario = r.IdRol //?? 0
                             };

                return result.ToList<UsuarioDTO>();
            }
        }

        public int[] getRoleKeys(int IdRol)
        {
            var roles = new int[1];
            if (IdRol == CONSTANTES.SUPER_ADMIN_ROL) roles = new int[] { CONSTANTES.ROL_ADMIN, CONSTANTES.ROL_RESPONSABLE };
            if (IdRol == CONSTANTES.ROL_ADMIN) roles = new int[] { CONSTANTES.ROL_RESPONSABLE };
            if (IdRol == CONSTANTES.ROL_RESPONSABLE) roles = new int[] { CONSTANTES.ROL_RESPONSABLE };
            return roles;
        }

        public IList<UsuarioDTO> getUsuarios(int IdRol, int[] usuariosIds) {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.IdRol != CONSTANTES.SUPER_ADMIN_ROL & r.IdRol == IdRol & usuariosIds.Contains(r.IdUsuario)
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 Email = r.Email,
                                 Cuenta = r.Cuenta,
                                 Active = r.Estado,
                                 IdRolUsuario = r.IdRol //?? 0
                             };
                return result.AsEnumerable<UsuarioDTO>().OrderByDescending(x => x.Nombre).ToList<UsuarioDTO>();
            }
        }

        public System.Collections.IList getUsuarios2(int IdRol)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.IdRol != CONSTANTES.SUPER_ADMIN_ROL & r.IdRol == IdRol
                             select new
                             {
                                 name = r.Nombre,
                                 id = r.IdUsuario,
                                 //tareas = context.Tarea.Where(x => x.IdResponsable == r.IdUsuario).Select(y => new { IdTarea = y.IdTarea, Nombre = y.Nombre, FechaInicio = y.FechaInicio, FechaFin = y.FechaFin })
                             };
                return result.ToList();//.AsEnumerable().OrderByDescending(x => x.name).ToList();
            }
        }

        public bool add(UsuarioDTO user)
        {
            using (var context = getContext())
            {
                try
                {
                    Usuario usuario = new Usuario();
                    usuario.Nombre = user.Nombre;
                    usuario.InicialesNombre = user.InicialesNombre;
                    usuario.Email = user.Email;
                    usuario.Cuenta = user.Cuenta;
                    usuario.Pass = Encrypt.GetCrypt(user.Pass);
                    usuario.IdRol = user.IdRolUsuario;//>= 2 ? user.IdRol : 3;
                    //usuario.IdCargo = user.IdCargo;
                    usuario.RutaFirma = user.RutaFirma;
                    usuario.Estado = true;
                    usuario.FechaRegistro = DateTime.Now;
                    context.Usuario.Add(usuario);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public bool validateUsuario(UsuarioDTO user)
        {
            using (var context = getContext())
            {
                if (user.Cuenta != null && user.Email != null)
                {
                    var result = from r in context.Usuario
                                 where r.Cuenta == user.Cuenta || r.Email == user.Email
                                 select r;
                    if (result.FirstOrDefault<Usuario>() == null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        //public IEnumerable<RolDTO> getRoles()
        //{
        //    using (var context = getContext())
        //    {
        //        var result = from r in context.Rol
        //                     where r.IdRol != CONSTANTES.SUPER_ADMIN_ROL
        //                     select new RolDTO
        //                     {
        //                         IdRol = r.IdRol,
        //                         Nombre = r.Nombre
        //                     };
        //        return result.ToList<RolDTO>();
        //    }
        //}
        public IList<RolDTO> getRoles()
        {
            using (var context = getContext())
            {
                var result = from r in context.Rol
                             //where r.Activo == true//r.IdRol != CONSTANTES.SUPER_ADMIN_ROL
                             select new RolDTO
                             {
                                 IdRol = r.IdRol,
                                 Nombre = r.Nombre
                             };
                return result.ToList<RolDTO>();
            }
        }

        public IList<RolDTO> getRolesCurrent(int idCurrentRol)
        {
            using (var context = getContext())
            {
                var result = from r in context.Rol
                             where r.IdRol >= idCurrentRol
                             select new RolDTO
                             {
                                 IdRol = r.IdRol,
                                 Nombre = r.Nombre
                             };
                return result.ToList<RolDTO>();
            }
        }

        public UsuarioDTO getUsuarioByCuenta(UsuarioDTO user)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.Cuenta == user.Cuenta
                             select new UsuarioDTO
                             {
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 IdRolUsuario = r.IdRol,// ?? 0,
                                 Active = r.Estado,
                                 Email = r.Email,
                                 Pass = r.Pass,
                                 Cuenta = r.Cuenta
                             };
                return result.SingleOrDefault<UsuarioDTO>();
            }
        }

        public bool isValidUser(UsuarioDTO user)
        {
            if (user.Cuenta == null || user.Pass == null)
                return false;

            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.Estado == true & r.Cuenta == user.Cuenta
                             select r;
                Usuario usuario = result.SingleOrDefault<Usuario>();
                if (usuario != null)
                {
                    if (Encrypt.comparetoCrypt(user.Pass, usuario.Pass))
                        return true;
                }
            }
            return false;
        }

        //public UsuarioDTO getUsuario(int id)
        //{
        //    using (var context = getContext())
        //    {
        //        var result = from r in context.Usuario
        //                     where r.IdUsuario == id
        //                     select new UsuarioDTO
        //                     {
        //                         Cuenta = r.Cuenta,
        //                         Email = r.Email,
        //                         Active = r.Estado,
        //                         IdRol = r.IdRol,// ?? 0,
        //                         IdCargo = r.IdCargo ?? 0,
        //                         IdUsuario = r.IdUsuario,
        //                         Nombre = r.Nombre,
        //                         InicialesNombre = r.InicialesNombre,
        //                         Pass = r.Pass
        //                     };
        //        return result.SingleOrDefault();
        //    }
        //}
        public UsuarioDTO getUsuario(int id)
        {
            using (var context = getContext())
            {
                var result = from r in context.Usuario
                             where r.IdUsuario == id
                             select new UsuarioDTO
                             {
                                 Cuenta = r.Cuenta,
                                 Email = r.Email,
                                 Active = r.Estado,
                                 //IdRol = r.IdRol,// ?? 0,
                                 IdRolUsuario = r.IdRol,
                                 IdUsuario = r.IdUsuario,
                                 Nombre = r.Nombre,
                                 InicialesNombre = r.InicialesNombre,
                                 Pass = r.Pass,
                                 RutaFirma = r.RutaFirma
                             };
                return result.SingleOrDefault();
            }
        }

        public bool update(UsuarioDTO user, string passUser, string passChange, UsuarioDTO currentUser)
        {
            using (var context = getContext())
            {
                try
                {
                    Usuario usuario = context.Usuario.Where(x => x.IdUsuario == user.IdUsuario).SingleOrDefault();
                    if (usuario != null)
                    {
                        usuario.Nombre = user.Nombre;
                        usuario.InicialesNombre = user.InicialesNombre;
                        usuario.Email = user.Email;
                        usuario.IdRol = user.IdRolUsuario;// >= 2 ? user.IdRol : 3;
                        //usuario.IdCargo = user.IdCargo;
                        usuario.Cuenta = user.Cuenta;
                        usuario.Estado = user.Active;
                        usuario.RutaFirma = user.RutaFirma;
                        if (!String.IsNullOrWhiteSpace(passUser) && !String.IsNullOrWhiteSpace(passChange))
                        {
                            if ((currentUser.IdRolUsuario <= 2 || currentUser.IdUsuario == user.IdUsuario)
                                && Encrypt.comparetoCrypt(passUser, currentUser.Pass))
                            {
                                usuario.Pass = Encrypt.GetCrypt(passChange);
                            }
                            else return false;
                        }
                        context.SaveChanges();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    //throw e;
                    return false;
                }
                return false;
            }
        }

        public bool recoverPassword(string CuentaEmail)
        {
            using (var context = getContext())
            {
                var result = (from r in context.Usuario
                              where r.Cuenta.Equals(CuentaEmail) || r.Email.Equals(CuentaEmail) && r.Estado == true
                              select new UsuarioDTO
                              {
                                  IdUsuario = r.IdUsuario,
                                  Nombre = r.Nombre,
                                  Email = r.Email
                              }).FirstOrDefault();
                if (result != null)
                {
                    string newPassword = Functions.GeneratePassword();
                    updatePassword(result, newPassword);
                    SendMailPassRecovery(result, newPassword);
                    return true;
                }
                else
                    return false;
            }
        }
        public bool updatePassword(UsuarioDTO user, string passChange)
        {
            using (var context = getContext())
            {
                try
                {
                    Usuario usuario = context.Usuario.Where(x => x.IdUsuario == user.IdUsuario).SingleOrDefault();
                    usuario.Pass = Encrypt.GetCrypt(passChange);
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
        public void SendMailPassRecovery(UsuarioDTO user, string passChange)
        {
            string to = user.Email;
            string subject = "Recuperación de Contraseña";
            string body = "Sr(a). " + user.Nombre + " su contraseña es : " + passChange;
            MailHandler.Send(to, "", subject, body);
        }
        #region
        public IList<UsuarioDTO> searchResponsables(string busqueda)
        {
            using (var context = getContext())
            {
                return (from r in context.Usuario
                        where r.IdRol != CONSTANTES.SUPER_ADMIN_ROL
                        & r.Nombre.Contains(busqueda)
                        & r.Estado == true
                        select new UsuarioDTO
                        {
                            IdUsuario = r.IdUsuario,
                            Nombre = r.Nombre,
                            InicialesNombre = r.InicialesNombre,
                            Email = r.Email
                        }).ToList();
            }
        }
        #endregion
    }
}
