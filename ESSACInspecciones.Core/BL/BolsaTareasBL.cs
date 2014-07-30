using ESSACInspecciones.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.BL
{//ELIMINAR LA CLASE
    public class BolsaTareasBL : Base
    {
        //public bool add(int idTarea)
        //{
        //    bool retorno = false;
        //    using (var context = getContext())
        //    {
        //        try
        //        {
        //            BolsaTareas changeBolsaTareas = new BolsaTareas();
        //            var originalBolsaTareas = context.BolsaTareas.Where(x => x.IdTarea == idTarea).FirstOrDefault();
        //            if (originalBolsaTareas != null)
        //            {
        //                changeBolsaTareas = originalBolsaTareas;
        //                changeBolsaTareas.FechaCreacion = DateTime.Now;//DateTime.UtcNow
        //                changeBolsaTareas.Estado = true;
        //                retorno = false;
        //            }
        //            else
        //            {
        //                changeBolsaTareas = new BolsaTareas();
        //                changeBolsaTareas.IdTarea = idTarea;
        //                changeBolsaTareas.FechaCreacion = DateTime.Now;
        //                changeBolsaTareas.Estado = true;
        //                retorno = true;
        //            }
        //            if (context.Entry(changeBolsaTareas).State == System.Data.EntityState.Detached)
        //                context.Set<BolsaTareas>().Add(changeBolsaTareas);                    
        //            context.SaveChanges();                    
        //        }
        //        catch (Exception e)
        //        {
        //            //throw e;
        //            retorno = true;
        //        }
        //        return retorno;
        //    }
        //}
        //public bool delete(int idTarea)
        //{
        //    using (var context = getContext())
        //    {
        //        try
        //        {
        //            var bolsaTareas = context.BolsaTareas.Where(x => x.IdTarea == idTarea).SingleOrDefault();
        //            bolsaTareas.Estado = false;
        //            context.SaveChanges();
        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            //throw e;
        //            return false;
        //        }
        //    }
        //}
    }
}
