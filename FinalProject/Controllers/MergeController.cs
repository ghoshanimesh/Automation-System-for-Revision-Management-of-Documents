using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinalProject.Controllers
{
    public class MergeController : Controller
    {
        // GET: Merge
        public ActionResult Index(int doc_id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    ViewBag.DocId = doc_id;
                    return View(GetBranchNamesOnDocId(doc_id));
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

        public Dictionary<int,string> GetBranchNamesOnDocId(int doc_id)
        {
            Dictionary<int,string> branchNames = new Dictionary<int, string>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT branch_owner_user_id,branch_name FROM `branch` WHERE doc_id = @doc_id AND branch_owner_user_id != @user_id";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
            try
            {
                conn.Open();
                MySqlDataReader fetchQuery = query.ExecuteReader();
                while (fetchQuery.Read())
                {
                    branchNames.Add(Convert.ToInt32(fetchQuery["branch_owner_user_id"]), fetchQuery["branch_name"].ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Errorrrr " + ex);
            }
            conn.Close();
            return branchNames;
        }

        public int GetBranchId(int doc_id, int branch_owner_user_id)
        {
            int branch_id = 0;
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM `branch` WHERE doc_id=@doc_id AND branch_owner_user_id = @branch_owner_user_id";
            query.Parameters.AddWithValue("@doc_id", doc_id);
            query.Parameters.AddWithValue("@branch_owner_user_id", branch_owner_user_id);
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

        [HttpPost]
        public ActionResult MergeRequest(FormCollection fc)
        {
            Debug.WriteLine(fc["doc_id"]);
            Debug.WriteLine(fc["merge_branch_id"]);
            int requested_from_branch_id = GetBranchId(Convert.ToInt32(fc["doc_id"]), (int)Session["user_id"]);
            Debug.WriteLine(requested_from_branch_id);
            int requested_to_branch_id = GetBranchId(Convert.ToInt32(fc["doc_id"]), Convert.ToInt32(fc["merge_branch_id"]));
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "INSERT INTO `merge_request`(`requested_from`, `requested_to`, `requested_from_branch_id`, `requested_to_branch_id`, `status`, `action_performed`, `created_at`, `updated_at`) VALUES (@requested_from, @requested_to, @requested_from_branch_id, @requested_to_branch_id, @status, @action_performed, @created_at, @updated_at)";
            query.Parameters.AddWithValue("@requested_from", Session["user_id"]);
            query.Parameters.AddWithValue("@requested_to", fc["merge_branch_id"]);
            query.Parameters.AddWithValue("@requested_from_branch_id", requested_from_branch_id);
            query.Parameters.AddWithValue("@requested_to_branch_id", requested_to_branch_id);
            query.Parameters.AddWithValue("@status", 0);
            query.Parameters.AddWithValue("@action_performed", 0);
            query.Parameters.AddWithValue("@created_at", DateTime.Now);
            query.Parameters.AddWithValue("@updated_at", DateTime.Now);
            int rowsAffected = 0;
            try
            {
                conn.Open();
                rowsAffected = query.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception branch fetch" + e);
            }
            conn.Close();
            return Redirect("/MergeRequest/SentMergeRequest");
        }

    }
}