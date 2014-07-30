using ESSACInspecciones.Core.DTO;
using ESSACInspecciones.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.BL
{
    public class ClienteBL: Base
    {

        #region CRUD Cliente
        public IList<ClienteDTO> getClientes(bool activeOnly = false)
        {
            using (var context = getContext())
            {
                var result = !activeOnly ? from r in context.Cliente
                                           select new ClienteDTO
                                           {
                                               IdCliente = r.IdCliente,
                                               NombreEmpresa = r.Nombre,
                                               Telefono1 = r.Telefono_1,
                                               Telefono2 = r.Telefono_2,
                                               contacto = context.ContactoCliente.Where(x => x.IdCliente == r.IdCliente && x.Default == true).Select(y => new ContactoDTO{ IdContacto = y.IdContactoCliente, Nombre = y.Nombre, Anexo = y.Anexo, Telefono = y.Telefono, Email = y.Email, Celular = y.Celular }).FirstOrDefault(),
                                               Active = r.Active
                                           } : from r in context.Cliente
                                               where r.Active == true
                                               select new ClienteDTO
                                               {
                                                   IdCliente = r.IdCliente,
                                                   NombreEmpresa = r.Nombre,
                                                   Telefono1 = r.Telefono_1,
                                                   Telefono2 = r.Telefono_2,
                                                   contacto = context.ContactoCliente.Where(x => x.IdCliente == r.IdCliente && x.Default == true).Select(y => new ContactoDTO { IdContacto = y.IdContactoCliente, Nombre = y.Nombre, Anexo = y.Anexo, Telefono = y.Telefono, Email = y.Email, Celular = y.Celular }).FirstOrDefault(),
                                                   Active = r.Active
                                               };

                return result.ToList<ClienteDTO>();
            }
        }

        public bool add(ClienteDTO ClienteDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    Cliente Cliente = new Cliente();
                    Cliente.Nombre = ClienteDTO.NombreEmpresa;
                    Cliente.Telefono_1 = ClienteDTO.Telefono1;
                    Cliente.Telefono_2 = ClienteDTO.Telefono2;
                    Cliente.Active = true;
                    context.Cliente.Add(Cliente);
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

        public bool update(ClienteDTO ClienteDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    var Cliente = context.Cliente.Where(x => x.IdCliente == ClienteDTO.IdCliente).SingleOrDefault();
                    Cliente.Nombre = ClienteDTO.NombreEmpresa;
                    Cliente.Telefono_1 = ClienteDTO.Telefono1;
                    Cliente.Telefono_2 = ClienteDTO.Telefono2;
                    Cliente.Active = ClienteDTO.Active;
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

        public ClienteDTO getCliente(int id)
        {
            using (var context = getContext())
            {
                var result = from r in context.Cliente
                             where r.IdCliente == id
                             select new ClienteDTO
                             {
                                 NombreEmpresa = r.Nombre,
                                 Telefono1 = r.Telefono_1,
                                 Telefono2 = r.Telefono_2,
                                 IdCliente = r.IdCliente,
                                 Active = r.Active
                             };
                return result.SingleOrDefault();
            }
        }
        public IList<ClienteDTO> getComboClientes()
        {
            using (var context = getContext())
            {
                var result = context.Cliente.Join(context.Inmueble, x => x.IdCliente, y => y.IdCliente, (x, y) => new { x, y }).Where(z => z.x.Active == true).Select(r => new ClienteDTO { IdCliente = r.x.IdCliente, NombreEmpresa = r.x.Nombre }).Distinct().ToList();
                return result;
            }
        }
        #endregion

        #region CRUD Contacto
        public IList<ContactoDTO> getContactos(int IdCliente, bool activeOnly = false)
        {
            using (var context = getContext())
            {
                var result = !activeOnly ? from r in context.ContactoCliente
                                           where r.IdCliente == IdCliente
                                           select new ContactoDTO
                                           {
                                               IdContacto = r.IdContactoCliente,
                                               Nombre =r.Nombre,
                                               Telefono =r.Telefono,
                                               Email = r.Email,
                                               Cargo = r.Cargo,
                                               Celular = r.Celular,
                                               Area = r.Area,
                                               Anexo = r.Anexo,
                                               IdCliente = r.IdCliente,
                                               Default = r.Default,
                                               Active = r.Active
                                           } : from r in context.ContactoCliente
                                               where r.IdCliente == IdCliente & r.Active == true
                                               select new ContactoDTO
                                               {
                                                   IdContacto = r.IdContactoCliente,
                                                   Nombre =r.Nombre,
                                                   Telefono =r.Telefono,
                                                   Email = r.Email,
                                                   Cargo = r.Cargo,
                                                   Celular = r.Celular,
                                                   Area = r.Area,
                                                   Anexo = r.Anexo,
                                                   Default = r.Default,
                                                   IdCliente = r.IdCliente,
                                                   Active = r.Active
                                               };

                return result.ToList<ContactoDTO>();
            }
        }

        public bool add(ContactoDTO ContactoDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    ContactoCliente Contacto = new ContactoCliente();
                    Contacto.Nombre = ContactoDTO.Nombre;
                    Contacto.Telefono = ContactoDTO.Telefono;
                    Contacto.Email = ContactoDTO.Email;
                    Contacto.Cargo = ContactoDTO.Cargo;
                    Contacto.Celular = ContactoDTO.Celular;
                    Contacto.Area = ContactoDTO.Area;
                    Contacto.Anexo = ContactoDTO.Anexo;
                    Contacto.IdCliente = ContactoDTO.IdCliente;
                    Contacto.Default = ContactoDTO.Default;
                    Contacto.Active = true;
                    context.ContactoCliente.Add(Contacto);
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

        public bool update(ContactoDTO ContactoDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    var Contacto = context.ContactoCliente.Where(x => x.IdContactoCliente == ContactoDTO.IdContacto).SingleOrDefault();
                    Contacto.Nombre = ContactoDTO.Nombre;
                    Contacto.Telefono = ContactoDTO.Telefono;
                    Contacto.Email = ContactoDTO.Email;
                    Contacto.Cargo = ContactoDTO.Cargo;
                    Contacto.Celular = ContactoDTO.Celular;
                    Contacto.Area = ContactoDTO.Area;
                    Contacto.Anexo = ContactoDTO.Anexo;
                    Contacto.IdCliente = ContactoDTO.IdCliente;
                    Contacto.Active = ContactoDTO.Active;
                    Contacto.Default = ContactoDTO.Default;
                    Contacto.Active = ContactoDTO.Active;
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

        public ContactoDTO getContacto(int id)
        {
            using (var context = getContext())
            {
                var result = from r in context.ContactoCliente
                             where r.IdContactoCliente == id
                             select new ContactoDTO
                             {
                                 IdContacto = r.IdContactoCliente,
                                 Nombre = r.Nombre,
                                 Telefono = r.Telefono,
                                 Email = r.Email,
                                 Cargo = r.Cargo,
                                 Celular = r.Celular,
                                 Default = r.Default,
                                 Area = r.Area,
                                 Anexo = r.Anexo,
                                 IdCliente = r.IdCliente,
                                 Active = r.Active
                             };
                return result.SingleOrDefault();
            }
        }
        #endregion

        #region CRUD Inmueble
        public IList<InmuebleDTO> getInmuebles(int IdCliente, bool activeOnly = false)
        {
            using (var context = getContext())
            {
                var result = !activeOnly ? from r in context.Inmueble
                                           where r.IdCliente == IdCliente
                                           select new InmuebleDTO
                                           {
                                               IdInmueble = r.IdInmueble,
                                               NombreInmueble = r.Nombre,
                                               Direccion = r.Direccion,
                                               IdCliente = r.IdCliente,
                                               Active = r.Active
                                           } : from r in context.Inmueble
                                               where r.IdCliente == IdCliente & r.Active == true
                                               select new InmuebleDTO
                                               {
                                                   IdInmueble = r.IdInmueble,
                                                   NombreInmueble = r.Nombre,
                                                   Direccion = r.Direccion,
                                                   IdCliente = r.IdCliente,
                                                   Active = r.Active
                                               };

                return result.ToList<InmuebleDTO>();
            }
        }

        public bool add(InmuebleDTO InmuebleDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    Inmueble Inmueble = new Inmueble();
                    Inmueble.Nombre = InmuebleDTO.NombreInmueble;
                    Inmueble.Direccion = InmuebleDTO.Direccion;
                    Inmueble.IdCliente = InmuebleDTO.IdCliente;
                    Inmueble.Active = true;
                    context.Inmueble.Add(Inmueble);
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

        public bool update(InmuebleDTO InmuebleDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    var Inmueble = context.Inmueble.Where(x => x.IdInmueble == InmuebleDTO.IdInmueble).SingleOrDefault();
                    Inmueble.Nombre = InmuebleDTO.NombreInmueble;
                    Inmueble.Direccion = InmuebleDTO.Direccion;
                    Inmueble.IdCliente = InmuebleDTO.IdCliente;
                    Inmueble.Active = InmuebleDTO.Active;
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

        public InmuebleDTO getInmueble(int id)
        {
            using (var context = getContext())
            {
                var result = from r in context.Inmueble
                             where r.IdInmueble == id
                             select new InmuebleDTO
                             {
                                 IdInmueble = r.IdInmueble,
                                 NombreInmueble = r.Nombre,
                                 Direccion = r.Direccion,
                                 IdCliente = r.IdCliente,
                                 Active = r.Active
                             };
                return result.SingleOrDefault();
            }
        }
        #endregion

        #region Combos
        public IList<EstadoDTO> getEstados(int IdServicio, bool activeOnly = false)
        {
            using (var context = getContext())
            {
                var result = !activeOnly ? from r in context.ServicioEstado
                                           join x in context.Estado on r.IdEstado equals x.IdEstado
                                           where r.IdServicio == IdServicio
                                           select new EstadoDTO
                                           {
                                               IdEstado = r.IdEstado,
                                               NombreEstado = x.NombreEstado,
                                               Active = r.Active
                                           } : from r in context.ServicioEstado
                                               join x in context.Estado on r.IdEstado equals x.IdEstado
                                               where r.IdServicio == IdServicio & r.Active == true
                                               select new EstadoDTO
                                               {
                                                   IdEstado = r.IdEstado,
                                                   NombreEstado = x.NombreEstado,
                                                   Active = r.Active
                                               };

                return result.ToList<EstadoDTO>();
            }
        }
        #endregion

        #region Busqueda
        public IList<ClienteDTO> searchClientes(string busqueda)
        {
            using (var context = getContext())
            {
                return (from r in context.Cliente
                        //join x in context.Inmueble on r.IdCliente equals x.IdCliente
                        where (r.Nombre.Contains(busqueda) | r.Telefono_1.Contains(busqueda) | r.Telefono_2.Contains(busqueda))
                        select new ClienteDTO
                        {
                            IdCliente = r.IdCliente,
                            NombreEmpresa = r.Nombre,
                            Telefono1 = r.Telefono_1,
                            Telefono2 = r.Telefono_2
                            //Inmuebles = context.Inmueble.Where(x => x.IdCliente == r.IdCliente).Select(y => new InmuebleDTO{ NombreInmueble = y.Nombre, Direccion = y.Direccion })
                        }).ToList();
            }
        }
        public IList<InmuebleDTO> searchInmuebles(string busqueda)
        {
            using (var context = getContext())
            {
                return (from r in context.Inmueble
                        where (r.Nombre.Contains(busqueda) | r.Direccion.Contains(busqueda))
                        select new InmuebleDTO
                        {
                            IdInmueble = r.IdInmueble,
                            IdCliente = r.IdCliente,
                            NombreInmueble = r.Nombre,
                            Direccion = r.Direccion
                        }).ToList();
            }
        }
        #endregion
    }
}
