using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanvasCourseCreator.Models
{
    public class SISPeriodModel
    {
        public int mstuniq { get; set; }
        public int period { get; set; }
        public string termc { get; set; }
        public int funiq { get; set; }
        public CanvasSectionModel section { get; set; }
    }
}