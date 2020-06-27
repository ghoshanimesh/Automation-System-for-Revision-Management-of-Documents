using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinalProject.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FinalProject.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    return View(GetDocuments((int)Session["user_id"]));
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

        public List<CollaboratorDocument> GetDocuments(int user_id)
        {
            List<CollaboratorDocument> documents = new List<CollaboratorDocument>();
            ArrayList docIds = new ArrayList();
            MySqlConnection conn = HomeController.conn();
            MySqlConnection conn_inner = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            MySqlCommand inner_query_1 = conn.CreateCommand();
            MySqlCommand inner_query_2 = conn_inner.CreateCommand();
            query.CommandText = "SELECT branch_owner_user_id,doc_id FROM `branch` WHERE branch_owner_user_id = @user_id";
            query.Parameters.AddWithValue("@user_id", user_id);
            try
            {
                conn.Open();
                Debug.WriteLine("query");
                MySqlDataReader fetchQuery = query.ExecuteReader();
                while (fetchQuery.Read())
                {
                    int doc_id = Convert.ToInt32(fetchQuery["doc_id"].ToString());
                    docIds.Add(doc_id);
                }
                conn.Close();
                foreach (var doc_id in docIds)
                {
                    conn.Open();
                    inner_query_1.CommandText = "SELECT document.doc_id, board.board_name, standard.standard, subject.subject, level.level, language.language_name, document.doc_title FROM ((((((`document` INNER JOIN subj_lang_level ON subj_lang_level.subj_lang_level_id = document.subj_lang_level_id) INNER JOIN language On language.language_id = subj_lang_level.language_id) INNER JOIN level On subj_lang_level.level_id = level.level_id) INNER JOIN subject ON subj_lang_level.subject_id = subject.subject_id) INNER JOIN standard ON standard.standard_id = subject.standard_id) INNER JOIN board ON board.board_id = standard.board_id) WHERE doc_id = @doc_id";
                    inner_query_1.Parameters.Clear();
                    inner_query_1.Parameters.AddWithValue("@doc_id", doc_id);
                    Debug.WriteLine("inner query 1");
                    MySqlDataReader innerFQ_1 = inner_query_1.ExecuteReader();

                    conn_inner.Open();
                    inner_query_2.CommandText = "SELECT document_versions.updated_at, user.user_fullname, document_versions.doc_version, document_versions.doc_subversion, document_versions.doc_revision_number FROM ((`branch` INNER JOIN document_versions ON branch.branch_id = document_versions.branch_id) INNER JOIN user ON user.user_id = branch.branch_owner_user_id)  WHERE doc_id=@doc_id ORDER BY document_versions.updated_at DESC LIMIT 1";
                    inner_query_2.Parameters.Clear();
                    inner_query_2.Parameters.AddWithValue("@doc_id", doc_id);
                    Debug.WriteLine("inner query 2");
                    MySqlDataReader innerFQ_2 = inner_query_2.ExecuteReader();

                    Debug.WriteLine("Finished all");
                    while (innerFQ_1.Read() && innerFQ_2.Read())
                    {
                        documents.Add(new CollaboratorDocument(
                            Convert.ToInt32(innerFQ_1["doc_id"].ToString()),
                            innerFQ_1["doc_title"].ToString(),
                            innerFQ_1["board_name"].ToString(),
                            innerFQ_1["standard"].ToString(),
                            innerFQ_1["subject"].ToString(),
                            innerFQ_1["level"].ToString(),
                            innerFQ_1["language_name"].ToString(),
                            innerFQ_2["doc_version"].ToString() + "." + innerFQ_2["doc_subversion"].ToString() + "." + innerFQ_2["doc_revision_number"].ToString(),
                            innerFQ_2["updated_at"].ToString(),
                            innerFQ_2["user_fullname"].ToString()
                            )
                        );
                    }
                    conn.Close();
                    conn_inner.Close();
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException exception)
            {
                Debug.WriteLine(exception.ToString());
            }
            return documents;
        }

        public ActionResult Logout()
        {
            Session["set"] = false;
            Session["email"] = null;
            Session["level"] = -1;
            Session["user_name"] = null;
            Session["user_fullname"] = null;
            Session["user_role"] = 0;
            return Redirect("/Login?login=loggedout");
        }

    }
}