using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalProject.Models
{
    public class CollaboratorDocument
    {
        public int DocId { get; set; }
        public string DocumentName { get; set; }
        public string BoardName { get; set; }
        public string StandardName { get; set; }
        public string SubjectName { get; set; }
        public string Level { get; set; }
        public string Language { get; set; }
        public string CurrentVersion { get; set; }
        public string LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }

        public CollaboratorDocument(int DocId, string DocumentName, string BoardName, string StandardName, string SubjectName, string Level, string Language, string CurrentVersion, string LastModifiedOn, string LastModifiedBy)
        {
            this.DocId = DocId;
            this.DocumentName = DocumentName;
            this.BoardName = BoardName;
            this.StandardName = StandardName;
            this.SubjectName = SubjectName;
            this.Level = Level;
            this.Language = Language;
            this.CurrentVersion = CurrentVersion;
            this.LastModifiedOn = LastModifiedOn;
            this.LastModifiedBy = LastModifiedBy;
        }

    }
}