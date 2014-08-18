using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanvasCourseCreator.Models
{
    public class CanvasEnrollmentModel
    {
        public string id { get; set; }
        public string course_id { get; set; }
        public string sis_course_id { get; set; }
        public string sis_section_id { get; set; }
        public string user_id { get; set; }
        public string role { get; set; }

    }
}