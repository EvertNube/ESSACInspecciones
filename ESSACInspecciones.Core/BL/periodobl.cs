using ESSACInspecciones.Core.DTO;
using ESSACInspecciones.Data;
using ESSACInspecciones.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.BL
{
    public class PeriodoBL : Base
    {
        public IList<PeriodoDTO> getPeriodos()
        {
            using (var context = getContext())
            {
                var result = from r in context.Periodo.AsEnumerable()
                             select new PeriodoDTO
                             {
                                 IdPeriodo = r.IdPeriodo,
                                 Nombre = r.Nombre,
                                 FechaInicio = r.FechaInicio,
                                 FechaFin = r.FechaFin,
                                 Active = r.Active
                             };
                return result.ToList<PeriodoDTO>();
            }
        }

        public PeriodoDTO getPeriodo(int id)
        {
            using (var context = getContext())
            {
                var result = from r in context.Periodo.AsEnumerable()
                             where r.IdPeriodo == id
                             select new PeriodoDTO
                             {
                                 IdPeriodo = r.IdPeriodo,
                                 Nombre = r.Nombre,
                                 FechaInicio = r.FechaInicio,
                                 FechaFin = r.FechaFin,
                                 Active = r.Active
                             };
                return result.SingleOrDefault();
            }
        }

        public bool add(PeriodoDTO periodoDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    Periodo periodo = new Periodo();
                    periodo.Nombre = periodoDTO.Nombre;
                    periodo.FechaInicio = periodoDTO.FechaInicio;
                    periodo.FechaFin = periodoDTO.FechaFin;
                    periodo.Active = true;
                    context.Periodo.Add(periodo);
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

        public bool update(PeriodoDTO periodoDTO)
        {
            using (var context = getContext())
            {
                try
                {
                    var periodo = context.Periodo.Where(x => x.IdPeriodo == periodoDTO.IdPeriodo).SingleOrDefault();
                    periodo.Nombre = periodoDTO.Nombre;
                    periodo.FechaInicio = periodoDTO.FechaInicio;
                    periodo.FechaFin = periodoDTO.FechaFin;
                    periodo.Active = periodoDTO.Active;
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

    }
}
