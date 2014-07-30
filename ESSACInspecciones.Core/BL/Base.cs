using ESSACInspecciones.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using ESSACInspecciones.Helpers;

namespace ESSACInspecciones.Core
{
    public class Base
    {
        protected ESSACInspeccionesEntities getContext()
        {
            //return new ESSACInspeccionesEntities(ConnectionStringProvider.getEntityBuilder().ToString());
            return new ESSACInspeccionesEntities();
        }
      
    }
}
