using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ESSACInspecciones.Models
{
    public class Navbar
    {
        public string primaryActive { get; set; }
        public string successActive { get; set; }
        public string successActive2 { get; set; }
        public string infoActive { get; set; }
        public string warningActive { get; set; }
        public string warningActive2 { get; set; }
        public string dangerActive { get; set; }
        public string defaultActive { get; set; }
        public string darkActive { get; set; }
        public string disabledActive { get; set; }

        public Navbar()
        {
            this.primaryActive = "";
            this.successActive = "";
            this.successActive2 = "";
            this.infoActive = "";
            this.warningActive = "";
            this.warningActive2 = "";
            this.dangerActive = "";
            this.defaultActive = "";
            this.darkActive = "";
            this.disabledActive = "";
        }

        public void clearAll()
        {
            this.primaryActive = "";
            this.successActive = "";
            this.successActive2 = "";
            this.infoActive = "";
            this.warningActive = "";
            this.warningActive2 = "";
            this.dangerActive = "";
            this.defaultActive = "";
            this.darkActive = "";
            this.disabledActive = "";
        }

        public void activeAll()
        {
            this.primaryActive = "active";
            this.successActive = "active";
            this.infoActive = "active";
            this.warningActive = "active";
            this.dangerActive = "active";
            this.defaultActive = "active";
            this.darkActive = "active";
            this.disabledActive = "active";
        }
    }
}
