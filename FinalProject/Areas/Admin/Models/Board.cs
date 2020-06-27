using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Areas.Admin.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string BoardName { get; set; }

        public Board(int id, string boardName)
        {
            this.Id = id;
            this.BoardName = boardName;
        }
    }
}