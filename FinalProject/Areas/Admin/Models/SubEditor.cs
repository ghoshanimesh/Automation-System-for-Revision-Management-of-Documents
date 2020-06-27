using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Areas.Admin.Models
{
    public class SubEditor
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string UserFullname { get; set; }

        public SubEditor(int Id, string username, string userFullname)
        {
            this.UserId = Id;
            this.Username = username;
            this.UserFullname = userFullname;
        }
    }
}