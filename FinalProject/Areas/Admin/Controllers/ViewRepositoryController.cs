using FinalProject.Controllers;
using FinalProject.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinalProject.Areas.Admin.Controllers
{
    public class ViewRepositoryController : Controller
    {
        // GET: Admin/ViewRepository
        public ActionResult Index()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    return Redirect("/Admin/DashBoard");
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
                    var tupleModel = new Tuple<Dictionary<int, string>, List<Commit>>(GetBranchesOwnerOfDoc(doc_id), GetVersionHistoryOfBranch(branch_id));
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

        [HttpPost]
        public ActionResult isBranchUser(int user_id)
        {
            if ((int)Session["user_id"] == user_id)
            {
                return Json(new { result = "true" });
            }
            return Json(new { result = "false" });
        }

        public ActionResult DownloadFileVersion(int doc_ver_id)
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
                commits.Add(new Commit(Convert.ToInt32(fetchQuery["doc_version_id"].ToString()), fetchQuery["doc_version"].ToString() + "." + fetchQuery["doc_subversion"].ToString() + "." + fetchQuery["doc_revision_number"].ToString(), fetchQuery["doc_comment"].ToString(), DateTime.Parse(fetchQuery["doc_revision_date"].ToString())));
            }
            conn.Close();
            return commits;
        }

        public Dictionary<int, String> GetBranchesOwnerOfDoc(int doc_id)
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

    }
}