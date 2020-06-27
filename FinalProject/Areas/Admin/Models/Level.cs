using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Areas.Admin.Models
{
    public class Level
    {
        public int level_id { get; set; }
        public String level { get; set; }

        public Level(int level_id,String level)
        {
            this.level_id = level_id;
            this.level = level;
        }
    }
}