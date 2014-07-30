using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSACInspecciones.Core.DTO
{
    public class EventDTO
    {
        public string title { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public bool allDay { get; set; }
        public List<int> resource { get; set; }
        public int idTarea { get; set; }
        public string color { get; set; }
        public string strResource { get; set; }
        public EventDTO()
        {
            allDay = true;
        }
    }
}
