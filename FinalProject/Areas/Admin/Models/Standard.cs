using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Areas.Admin.Models
{
    public class Standard
    {
        public int Id { get; set; }
        public string BoardName { get; set; }
        public string StandardName { get; set; }

        public Standard(int id, string boardName, string stdName) {
            this.Id = id;
            this.BoardName = boardName;
            this.StandardName = stdName;
        }
    }
}