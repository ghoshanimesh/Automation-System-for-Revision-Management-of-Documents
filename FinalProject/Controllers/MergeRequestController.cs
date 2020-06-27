using FinalProject.Models;
using MySql.Data.MySqlClient;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinalProject.Controllers
{
    public class MergeRequestController : Controller
    {
        // GET: MergeRequest
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

        public ActionResult RecievedMergeRequest()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "SELECT merge_request_id, user.user_fullname as un,comment, status,action_performed FROM merge_request INNER JOIN user where requested_to = @user_id and user.user_id = requested_from";
                    query.Parameters.AddWithValue("@user_id", Session["user_id"]);
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
                    List<MergeRequest> res = new List<MergeRequest>();
                    while (fetchQuery.Read())
                    {
                        //ap - 0 : pending     ap=1,status=0 : n    ap1s1=yes
                        if (Convert.ToInt32(fetchQuery["status"]) == 0 && Convert.ToInt32(fetchQuery["action_performed"]) == 1)
                        {
                            res.Add(new MergeRequest(Convert.ToInt32(fetchQuery["merge_request_id"]), (string)fetchQuery["un"], "Rejected", (string)fetchQuery["comment"]));
                        }
                        else if (Convert.ToInt32(fetchQuery["action_performed"]) == 0)
                        {
                            res.Add(new MergeRequest(Convert.ToInt32(fetchQuery["merge_request_id"]), (string)fetchQuery["un"], "Pending", (string)fetchQuery["comment"]));
                            //pending
                        }                        
                        else if (Convert.ToInt32(fetchQuery["status"]) == 1 && Convert.ToInt32(fetchQuery["action_performed"]) == 1)
                        {
                            res.Add(new MergeRequest(Convert.ToInt32(fetchQuery["merge_request_id"]), (string)fetchQuery["un"], "Accepted", (string)fetchQuery["comment"]));
                        }
                    }
                    conn.Close();
                    return View(res);
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

        public ActionResult SentMergeRequest()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "SELECT merge_request_id, user.user_fullname as un,comment, status,action_performed FROM merge_request INNER JOIN user where requested_from = @user_id and user.user_id = requested_to";
                    query.Parameters.AddWithValue("@user_id", Session["user_id"]);
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
                    List<MergeRequest> res = new List<MergeRequest>();
                    while (fetchQuery.Read())
                    {
                        //ap - 0 : pending     ap=1,status=0 : n    ap1s1=yes
                        if (Convert.ToInt32(fetchQuery["status"]) == 0 && Convert.ToInt32(fetchQuery["action_performed"]) == 1)
                        {
                            res.Add(new MergeRequest(Convert.ToInt32(fetchQuery["merge_request_id"]), (string)fetchQuery["un"], "Rejected", (string)fetchQuery["comment"]));
                        }
                        else if (Convert.ToInt32(fetchQuery["action_performed"]) == 0)
                        {
                            res.Add(new MergeRequest(Convert.ToInt32(fetchQuery["merge_request_id"]), (string)fetchQuery["un"], "Pending", (string)fetchQuery["comment"]));
                            //pending
                        }
                        else if (Convert.ToInt32(fetchQuery["status"]) == 1 && Convert.ToInt32(fetchQuery["action_performed"]) == 1)
                        {
                            res.Add(new MergeRequest(Convert.ToInt32(fetchQuery["merge_request_id"]), (string)fetchQuery["un"], "Accepted", (string)fetchQuery["comment"]));
                        }
                    }
                    conn.Close();
                    return View(res);
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
        public ActionResult RejectMergeRequest(int merge_Request_id, string comment)
        {
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "UPDATE `merge_request` SET `status`= 0,`action_performed`=1, comment=@comment WHERE merge_request_id = @merge_request_id";
            query.Parameters.AddWithValue("@comment", comment);
            query.Parameters.AddWithValue("@merge_request_id", merge_Request_id);
            int RowsAffected = -1;
            try
            {
                conn.Open();
                RowsAffected = query.ExecuteNonQuery();
                conn.Close();
                return Json(new { result = "success" });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return Json(new { result = "error" + e.ToString() });
            }
        }

        public void DownloadLatestFileOfBranchWithName(int branch_id, string title)
        {
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM `document_versions` WHERE branch_id = @branch_id ORDER BY doc_revision_date DESC LIMIT 1";
            query.Parameters.AddWithValue("@branch_id", branch_id);
            MySqlDataReader fetchQuery;
            byte[] doc_file = null;
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
                doc_file = (byte[])fetchQuery["doc"];
            }
            conn.Close();
            System.IO.File.WriteAllBytes(Path.Combine(Server.MapPath("~/UploadedFiles/"), title + ".docx"), doc_file);
        }

        public void MergeFiles(string DirPath)
        {
            string path1 = DirPath + "requested_to.docx";
            string path2 = DirPath + "requested_from.docx";
            string op = DirPath + "merged.docx";

            byte[] arr1 = System.IO.File.ReadAllBytes(path1);
            byte[] arr2 = System.IO.File.ReadAllBytes(path2);

            var sources = new List<OpenXmlPowerTools.Source>();
            sources.Add(new OpenXmlPowerTools.Source(new WmlDocument("requested_to.docx", arr1)));
            sources.Add(new OpenXmlPowerTools.Source(new WmlDocument("requested_from.docx", arr2)));

            var mergedDoc = DocumentBuilder.BuildDocument(sources);
            mergedDoc.SaveAs(op);
        }

        public ActionResult AcceptMergeRequest(int merge_Request_id, string comment)
        {
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT requested_from_branch_id, requested_to_branch_id FROM `merge_request` WHERE merge_request_id = @merge_request_id";
            query.Parameters.AddWithValue("@merge_request_id", merge_Request_id);
            MySqlDataReader fetchQuery;
            int requested_from_bid = 0, requested_to_bid = 0;
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
                requested_from_bid = Convert.ToInt32(fetchQuery["requested_from_branch_id"]);
                requested_to_bid = Convert.ToInt32(fetchQuery["requested_to_branch_id"]);
            }
            conn.Close();
            Debug.WriteLine(requested_from_bid);
            Debug.WriteLine(requested_to_bid);

            DownloadLatestFileOfBranchWithName(requested_from_bid, "requested_from");
            DownloadLatestFileOfBranchWithName(requested_to_bid, "requested_to");
            if (Directory.GetFiles(Server.MapPath("~/UploadedFiles/")).Length > 1)
            {
                MergeFiles(Server.MapPath("~/UploadedFiles/"));
            }
            //query.CommandText = "UPDATE `branch` SET `is_merged`= 1, `updated_at`=@updated_at,`updated_by`=@updated_by WHERE `branch_id`=@branch_id";
            //query.Parameters.AddWithValue("@updated_at", DateTime.Now);
            //query.Parameters.AddWithValue("@updated_by", Session["user_id"]);
            //query.Parameters.AddWithValue("@branch_id", requested_from_bid);
            //int RowsAffected = -1;
            //try
            //{
            //    conn.Open();
            //    RowsAffected = query.ExecuteNonQuery();
            //}
            //catch (Exception e)
            //{
            //    fetchQuery = null;
            //    Debug.WriteLine("Exception branchowners" + e);
            //}
            //conn.Close();

            int doc_version = -1;
            string branch_name_to = "";
            query.CommandText = "SELECT document_versions.doc_version, branch.branch_name FROM (`document_versions` INNER JOIN branch ON document_versions.branch_id = branch.branch_id) WHERE branch.branch_id = @branch_id_to ORDER BY document_versions.doc_revision_date DESC LIMIT 1";
            query.Parameters.AddWithValue("@branch_id_to", requested_to_bid);
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
                doc_version = Convert.ToInt32(fetchQuery["doc_version"]);
                branch_name_to = (string)fetchQuery["branch_name"];
            }
            conn.Close();

            string p1 = Path.Combine(Server.MapPath("~/UploadedFiles/"), "merged.docx");
            System.IO.FileStream _FileStream = new System.IO.FileStream(p1, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);
            long _TotalBytes = new System.IO.FileInfo(p1).Length;
            byte[] _Buffer = _BinaryReader.ReadBytes((Int32)_TotalBytes);
            _FileStream.Close();
            _FileStream.Dispose();
            _BinaryReader.Close();

            query.CommandText = "INSERT INTO `document_versions`(`branch_id`, `doc`, `doc_version`, `doc_subversion`, `doc_revision_number`, `doc_comment`, `doc_revision_date`, `created_at`, `created_by`) VALUES (@branch_id_to_final, @doc, @doc_version, @doc_subversion, @doc_revision_number, @doc_comment, @time_revision, @time, @user_id)";
            query.Parameters.AddWithValue("@branch_id_to_final", requested_to_bid);
            query.Parameters.AddWithValue("@doc", _Buffer);
            query.Parameters.AddWithValue("@doc_version", doc_version + 1);
            query.Parameters.AddWithValue("@doc_subversion", 0);
            query.Parameters.AddWithValue("@doc_revision_number", 0);
            query.Parameters.AddWithValue("@doc_comment", "Merged with active branch " + branch_name_to);
            query.Parameters.AddWithValue("@time_revision", DateTime.Now);
            query.Parameters.AddWithValue("@time", DateTime.Now);
            query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
            conn.Open();
            int RowsAffected = query.ExecuteNonQuery();
            long commit_id = query.LastInsertedId;
            if (Directory.GetFiles(Server.MapPath("~/UploadedFiles/")).Length > 0)
            {
                Array.ForEach(Directory.GetFiles(Server.MapPath("~/UploadedFiles/")), System.IO.File.Delete);
            }
            conn.Close();
            query.CommandText = "UPDATE `merge_request` SET `status`= 1,`action_performed`=1, comment = @comment WHERE merge_request_id = @merge_request_id_new";
            query.Parameters.AddWithValue("@comment", comment);
            query.Parameters.AddWithValue("@merge_request_id_new", merge_Request_id);
            RowsAffected = -1;
            try
            {
                conn.Open();
                RowsAffected = query.ExecuteNonQuery();
                conn.Close();
                return Json(new { result = "success" });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return Json(new { result = "error" + e.ToString() });
            }
        }
    }
}