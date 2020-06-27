using FinalProject.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Mammoth;
using System.Text.RegularExpressions;

namespace FinalProject.Controllers
{
    public class DocumentStreamController : Controller
    {
        // GET: DocumentStream
        public ActionResult Index()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    return Redirect("/Dashboard");
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        //Index is via post only since doc_id is required and if other doc_id which is not assigned to an user 
        //is acquired by the user then the user can see the details of the doc not assigned to him
        [HttpPost]
        public ActionResult Index(FormCollection fc)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    int doc_id = Convert.ToInt32(fc["doc_id"]);
                    Debug.WriteLine(doc_id);
                    int branch_id = GetMasterBranchId(doc_id);
                    ViewBag.DocId = doc_id;
                    ViewBag.DocTitle = GetDocumentName(doc_id);
                    ViewBag.CommitCount = GetDocCommitCount(doc_id);
                    ViewBag.IsMergedRequest = IfMergeRequestCloseBranch(doc_id);
                    var tupleModel = new Tuple<Dictionary<int,string>, List<Commit>>(GetBranchesOwnerOfDoc(doc_id), GetVersionHistoryOfBranch(branch_id));
                    return View(tupleModel);
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        [HttpPost]
        public ActionResult UpdateCommits(int doc_id, int branch_owner_user_id)
        {
            int branch_id = 0;
            Debug.WriteLine(doc_id + " " + branch_owner_user_id);
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT branch_id FROM `branch` WHERE doc_id = @doc_id AND branch_owner_user_id = @user_id";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            query.Parameters.AddWithValue("@user_id", branch_owner_user_id);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
                Debug.WriteLine("Fetched");
                while (fetchQuery.Read())
                {
                    branch_id = Convert.ToInt32(fetchQuery["branch_id"]);
                }
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception branchowners" + e);
            }
            Debug.WriteLine(branch_id);
            conn.Close();
            List<Commit> commits = GetVersionHistoryOfBranch(branch_id);
            return Json(JsonConvert.SerializeObject(commits));
        }

        public int GetMasterBranchId(int doc_id)
        {
            int branch_id = 0;
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT branch_id FROM `branch` WHERE doc_id = @doc_id AND is_deleted = 0 AND is_master_branch = 1";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception branchowners" + e);
            }
            while (fetchQuery.Read())
            {
                branch_id = Convert.ToInt32(fetchQuery["branch_id"]);
            }
            conn.Close();
            return branch_id;
        }

        public string GetDocumentName(int doc_id)
        {
            string name = "";
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT document.doc_title FROM document WHERE document.doc_id = @doc_id AND is_deleted = 0";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception branchowners" + e);
            }
            while (fetchQuery.Read())
            {
                name = fetchQuery["doc_title"].ToString();
            }
            conn.Close();
            return name;
        }

        public int GetDocCommitCount(int doc_id)
        {
            int count = 0;
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT COUNT(*) AS commit_count FROM document_versions WHERE branch_id IN (SELECT branch_id FROM branch WHERE branch.doc_id = @doc_id AND is_deleted = 0)";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception branchowners" + e);
            }
            while (fetchQuery.Read())
            {
                count = Convert.ToInt32(fetchQuery["commit_count"]);
            }
            conn.Close();
            return count;            
        }

        public List<Commit> GetVersionHistoryOfBranch(int branch_id)
        {
            List<Commit> commits = new List<Commit>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM `document_versions` WHERE branch_id = @branch_id AND is_deleted = 0";
            query.Parameters.AddWithValue("@branch_id", branch_id);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception branchowners" + e);
            }
            while (fetchQuery.Read())
            {
                commits.Add(new Commit(Convert.ToInt32(fetchQuery["doc_version_id"].ToString()), fetchQuery["doc_version"].ToString() + "."+ fetchQuery["doc_subversion"].ToString() + "."+ fetchQuery["doc_revision_number"].ToString(), fetchQuery["doc_comment"].ToString(), DateTime.Parse(fetchQuery["doc_revision_date"].ToString())));
            }
            conn.Close();
            return commits;
        }

        public Dictionary<int,String> GetBranchesOwnerOfDoc(int doc_id)
        {
            Dictionary<int, String> branchOwners = new Dictionary<int, String>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT branch_owner_user_id,branch_name FROM `branch` WHERE is_deleted = 0 AND doc_id = @doc_id";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception branchowners" + e);
            }
            while (fetchQuery.Read())
            {
                branchOwners.Add(Convert.ToInt32(fetchQuery["branch_owner_user_id"].ToString()), fetchQuery["branch_name"].ToString());
            }
            conn.Close();
            return branchOwners;
        }

        [HttpPost]
        public ActionResult isBranchUser(int user_id)
        {
            if ((int)Session["user_id"] == user_id)
            {
                return Json(new { result = "true"});
            }
            return Json(new { result = "false" });
        }

        public ActionResult DownloadFile(int doc_ver_id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    Debug.WriteLine("I am here");
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "SELECT document_versions.doc,document.doc_title FROM ((document INNER JOIN branch ON document.doc_id = branch.doc_id) INNER JOIN document_versions ON document_versions.branch_id = branch.branch_id) WHERE document_versions.doc_version_id = @doc_ver_id";
                    query.Parameters.AddWithValue("@doc_ver_id", doc_ver_id);
                    MySqlDataReader fetchQuery;
                    try
                    {
                        conn.Open();
                        fetchQuery = query.ExecuteReader();
                        Debug.WriteLine("Done");
                    }
                    catch (Exception e)
                    {
                        fetchQuery = null;
                        Debug.WriteLine("Exception " + e);
                    }
                    Debug.WriteLine("Query done");
                    byte[] data = null;
                    String title = "";
                    while (fetchQuery.Read())
                    {
                        title = fetchQuery["doc_title"].ToString();
                        data = (byte[])fetchQuery["doc"];
                    }
                    conn.Close();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.ContentType = "application/ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + title + ".docx");
                    Response.BinaryWrite(data);
                    Response.Flush();
                    Response.End();
                    Debug.WriteLine("All done");
                    return new EmptyResult();
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        public ActionResult DownloadLatestFile(int doc_id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    Debug.WriteLine("I am here");
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "SELECT * FROM ((`document_versions` INNER JOIN branch ON branch.branch_id = document_versions.branch_id) INNER JOIN document ON document.doc_id = branch.doc_id) WHERE document.doc_id = @doc_id AND document.is_deleted = 0 ORDER BY document_versions.updated_at DESC LIMIT 1";
                    query.Parameters.AddWithValue("@doc_id", doc_id);
                    MySqlDataReader fetchQuery;
                    try
                    {
                        conn.Open();
                        fetchQuery = query.ExecuteReader();
                        Debug.WriteLine("Done");
                    }
                    catch (Exception e)
                    {
                        fetchQuery = null;
                        Debug.WriteLine("Exception " + e);
                    }
                    Debug.WriteLine("Query done");
                    byte[] data = null;
                    String title = "";
                    while (fetchQuery.Read())
                    {
                        title = fetchQuery["doc_title"].ToString();
                        data = (byte[])fetchQuery["doc"];
                    }
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.ContentType = "application/ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + title + ".docx");
                    Response.BinaryWrite(data);
                    Response.Flush();
                    Response.End();
                    Debug.WriteLine("All done");
                    return new EmptyResult();
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        public ActionResult UploadNewDocument(string route, int doc_id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    ViewBag.DocId = doc_id;
                    if (route == "initial")
                    {
                        ClearAllFiles(Server.MapPath("~/UploadedFiles/"));
                        ViewBag.Route = "initial";
                    }else if (route == "updoc")
                    {
                        ViewBag.Route = "updoc";
                    }else if (route == "error")
                    {
                        ClearAllFiles(Server.MapPath("~/UploadedFiles/"));
                        ViewBag.Route = "error";
                    }
                    return View();
                }
                else
                {
                    return Redirect("/Login?login=failed");
                }
            }
            else
            {
                return Redirect("/Login?login=loggedout");
            }
        }

        [HttpPost]
        public ActionResult DiffDocument(FormCollection fc)
        {
            ClearAllFiles(Server.MapPath("~/UploadedFiles/"));
            //Uploaded file
            HttpFileCollectionBase fileUploaded = Request.Files;
            var InputFileName = Path.GetFileName(fileUploaded[0].FileName);
            var ServerSavePath = Path.Combine(Server.MapPath("~/UploadedFiles/"), InputFileName);
            //Save file to server folder
            Debug.WriteLine(ServerSavePath);
            fileUploaded[0].SaveAs(ServerSavePath);
            Session["filename"] = InputFileName;

            //Last Commit File
            int doc_id = Convert.ToInt32(fc["doc_id"]);
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT doc FROM `document_versions` WHERE branch_id IN (SELECT branch_id FROM branch WHERE branch.branch_owner_user_id = @user_id AND branch.doc_id = @doc_id) ORDER BY doc_revision_date DESC LIMIT 1";
            query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
            query.Parameters.AddWithValue("@doc_id", doc_id);
            MySqlDataReader fetchQuery = null;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
                Console.WriteLine("Mila");
            }
            catch (Exception ep)
            {
                Console.WriteLine("Exception " + ep);
                Console.WriteLine("NULL");
            }
            while (fetchQuery.Read())
            {
                Console.WriteLine("Inner");
                var temp = (byte[])fetchQuery["doc"];
                System.IO.File.WriteAllBytes(Path.Combine(Server.MapPath("~/UploadedFiles/"), "document_db.docx"), temp);
            }
            conn.Close();
            CalculateDiff(Path.Combine(Server.MapPath("~/UploadedFiles/"), "document_db.docx"), Path.Combine(Server.MapPath("~/UploadedFiles/"), InputFileName));
            return RedirectToAction("UploadNewDocument", new { route = "updoc", doc_id = doc_id});
        }

        public void CalculateDiff(string s1, string s2)
        {
            var converter = new DocumentConverter().AddStyleMap("u => u"); ;
            var res1 = converter.ConvertToHtml(s1);
            var html1 = res1.Value;
            var warnings1 = res1.Warnings;
            var res2 = converter.ConvertToHtml(s2);
            var html2 = res2.Value;
            var warnings2 = res2.Warnings;

            Debug.WriteLine(html1 + "\n");
            Debug.WriteLine(html2);
            CompareFiles(html1, html2);
        }

        public void CompareFiles(string s1, string s2)
        {
            string text = "";
            if (GetHash(s1).Equals(GetHash(s2)))
            {
                Console.WriteLine("Same");
            }
            else
            {
                HtmlDiff.HtmlDiff diffHelper = new HtmlDiff.HtmlDiff(s1, s2);
                diffHelper.AddBlockExpression(new Regex(@"(<sup>[0-9]*</sup>|<sub>[0-9]*</sub>)", RegexOptions.IgnoreCase));
                text = diffHelper.Build();
            }
            string textstart = "<html><head><style> ins {background-color: #cfc;text-decoration:inherit;} del {color: #999;background-color:#FEC8C8;}ins.mod {background-color: #FFE1AC;}</style></head><body>";
            string textend = "</body></html>";
            string htmlFinal = String.Concat(textstart, text, textend);
            Console.WriteLine(htmlFinal);
            IronPdf.HtmlToPdf Renderer = new IronPdf.HtmlToPdf();
            Renderer.RenderHtmlAsPdf(htmlFinal).SaveAs(Path.Combine(Server.MapPath("~/UploadedFiles/"), "diff_op.pdf"));
        }

        public string GetHash(String input)
        {
            string hashResult = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                hashResult = builder.ToString();
            }
            return hashResult;
        }

        public ActionResult ViewDiff()
        {
            string path = Path.Combine(Server.MapPath("~/UploadedFiles/"), "diff_op.pdf");
            if (System.IO.File.Exists(path))
            {
                return File(path, "Application/pdf");
            }
            return RedirectToAction("UploadNewDocument");
        }

        public void ClearAllFiles(string DirPath)
        {
            Session["filename"] = "";
            if (Directory.GetFiles(DirPath).Length > 0)
            {
                Array.ForEach(Directory.GetFiles(DirPath), System.IO.File.Delete);
            }
        }

        public ActionResult CommitFile(int doc_id, string comment, int version_value)
        {
            string path = Path.Combine(Server.MapPath("~/UploadedFiles/"), "diff_op.pdf");
            if (System.IO.File.Exists(path))
            {
                int branch_id = 0, doc_subversion=0, doc_revisionnumber=0;
                byte[] _Buffer = null;
                MySqlConnection conn = HomeController.conn();
                MySqlCommand query = conn.CreateCommand();
                query.CommandText = "SELECT * FROM document_versions INNER JOIN branch ON branch.branch_id = document_versions.branch_id WHERE branch.branch_id = (SELECT branch_id FROM `branch` WHERE doc_id = @doc_id AND branch_owner_user_id = @user_id AND is_merged = 0 AND is_deleted = 0) ORDER BY document_versions.doc_revision_date DESC LIMIT 1";
                query.Parameters.AddWithValue("@doc_id", doc_id);
                query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
                MySqlDataReader fetchQuery;
                try
                {
                    conn.Open();
                    fetchQuery = query.ExecuteReader();
                }
                catch (Exception e)
                {
                    fetchQuery = null;
                    Debug.WriteLine("Exception branch fetch" + e);
                }
                while (fetchQuery.Read())
                {
                    branch_id = Convert.ToInt32(fetchQuery["branch_id"]);
                    doc_subversion = Convert.ToInt32(fetchQuery["doc_subversion"]);
                    doc_revisionnumber = Convert.ToInt32(fetchQuery["doc_revision_number"]);
                    Debug.WriteLine(doc_subversion + " " + doc_revisionnumber + " Numbersssss innereeer");
                }
                switch (version_value)
                {
                    case 1:
                        doc_subversion++;
                        doc_revisionnumber = 0;
                        break;
                    case 2:
                        doc_revisionnumber++;
                        break;
                }
                conn.Close();
                try
                {
                    if ((string)Session["filename"] != "")
                    {
                        string p1 = Path.Combine(Server.MapPath("~/UploadedFiles/"), (string)Session["filename"]);

                        System.IO.FileStream _FileStream = new System.IO.FileStream(p1, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                        System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);
                        long _TotalBytes = new System.IO.FileInfo(p1).Length;
                        _Buffer = _BinaryReader.ReadBytes((Int32)_TotalBytes);
                        _FileStream.Close();
                        _FileStream.Dispose();
                        _BinaryReader.Close();
                        MySqlCommand query_commit = conn.CreateCommand();
                        query_commit.CommandText = "INSERT INTO `document_versions`(`branch_id`, `doc`, `doc_version`, `doc_subversion`, `doc_revision_number`, `doc_comment`, `doc_revision_date`, `created_at`, `created_by`) VALUES (@branch_id, @doc, @doc_version, @doc_subversion, @doc_revision_number, @doc_comment, @time_revision, @time, @user_id)";
                        query_commit.Parameters.AddWithValue("@branch_id", branch_id);
                        query_commit.Parameters.AddWithValue("@doc", _Buffer);
                        query_commit.Parameters.AddWithValue("@doc_version", 0);
                        query_commit.Parameters.AddWithValue("@doc_subversion", doc_subversion);
                        query_commit.Parameters.AddWithValue("@doc_revision_number", doc_revisionnumber);
                        query_commit.Parameters.AddWithValue("@doc_comment", comment);
                        query_commit.Parameters.AddWithValue("@time_revision", DateTime.Now);
                        query_commit.Parameters.AddWithValue("@time", DateTime.Now);
                        query_commit.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
                        conn.Open();
                        int RowsAffected = query_commit.ExecuteNonQuery();
                        long commit_id = query_commit.LastInsertedId;
                        ClearAllFiles(Server.MapPath("~/UploadedFiles/"));
                        conn.Close();
                        return Json(new { result = "success" });
                    }
                    else
                    {
                        return Json(new { result = "error" });
                    }
                }
                catch (Exception _Exception)
                {
                    Debug.WriteLine("Exception caught in process: {0}", _Exception.ToString());
                    return Json(new { result = "error" });
                }
            }
            else
            {
                return Json(new { result = "error" });
            }
        }

        public bool IfMergeRequestCloseBranch(int doc_id)
        {
            bool result = false;
            int action_perf = -1;
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM `merge_request` WHERE requested_from = @requested_from AND requested_from_branch_id = (SELECT branch_id FROM `branch` WHERE doc_id = @doc_id AND branch_owner_user_id = @branch_owner_user_id AND is_merged = 0)";
            query.Parameters.AddWithValue("@requested_from", Session["user_id"]);
            query.Parameters.AddWithValue("@doc_id", doc_id);
            query.Parameters.AddWithValue("@branch_owner_user_id", Session["user_id"]);
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception branchowners" + e);
            }
            while (fetchQuery.Read())
            {
                action_perf = Convert.ToInt32(fetchQuery["action_performed"]);
            }
            conn.Close();
            if (action_perf == 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}