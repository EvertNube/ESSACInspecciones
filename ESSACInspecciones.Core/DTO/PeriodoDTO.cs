﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.DTO
{
    public class PeriodoDTO
    {
        public int IdPeriodo { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Active { get; set; }
    }
}
