using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Models
{
    public class Commit
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public string Comment { get; set; }
        public DateTime RevDate { get; set; }

        public Commit(int id, string ver, string comment, DateTime revDate)
        {
            this.Id = id;
            this.Version = ver;
            this.Comment = comment;
            this.RevDate = revDate;
        }
    }
}