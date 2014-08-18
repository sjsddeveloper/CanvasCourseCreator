using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanvasCourseCreator.Models
{
    public class CanvasSectionModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string sis_section_id { get; set; }
        public string course_id { get; set; }

    }
}