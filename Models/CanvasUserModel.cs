using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CanvasCourseCreator.Models
{
    public class CanvasUserModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public string sis_user_id { get; set; }
        public string login_id { get; set; }
        public string email { get; set; }
        public string last_login { get; set; }
    }
}