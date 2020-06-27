using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Areas.Admin.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string BoardName { get; set; }
        public string StdName { get; set; }
        public string SubjectName { get; set; }

        public Subject(int id, string boardName, string stdName, string subjectName)
        {
            this.Id = id;
            this.BoardName = boardName;
            this.StdName = stdName;
            this.SubjectName = subjectName;
        }
    }
}