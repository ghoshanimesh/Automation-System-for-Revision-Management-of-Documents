using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Areas.Admin.Models
{
    public class Language
    {
        public int language_id { get; set; }
        public String language_name { get; set; }

        public Language(int language_id,String language_name)
        {
            this.language_id = language_id;
            this.language_name = language_name;
        }
    }
}