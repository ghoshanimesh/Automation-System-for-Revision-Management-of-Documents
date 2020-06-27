using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Models
{
    public class MergeRequest
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string status { get; set; }
        public string comment { get; set; }

        public MergeRequest(int Id, string UserName, string status, string comment)
        {
            this.Id = Id;
            this.UserName = UserName;
            this.status = status;
            this.comment = comment;
        }
    }
}