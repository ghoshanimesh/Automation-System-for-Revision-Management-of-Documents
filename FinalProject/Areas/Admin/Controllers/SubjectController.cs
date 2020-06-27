using FinalProject.Areas.Admin.Models;
using FinalProject.Controllers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinalProject.Areas.Admin.Controllers
{
    public class SubjectController : Controller
    {
        // GET: Admin/Subject
        public ActionResult Index() { 
            List<Subject> subjList = new List<Subject>();
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "SELECT subject.subject_id, subject.subject, board.board_name, standard.standard FROM (`subject` INNER JOIN (standard INNER Join board On standard.board_id = board.board_id) ON standard.standard_id = subject.standard_id) WHERE subject.is_deleted = 0";
                    MySqlDataReader fetchQuery;
                    try
                    {
                        conn.Open();
                        fetchQuery = query.ExecuteReader();
                    }
                    catch (Exception e)
                    {
                        fetchQuery = null;
                        Debug.WriteLine("Exception " + e);
                    }
                    while (fetchQuery.Read())
                    {
                        subjList.Add(new Subject(Convert.ToInt32(fetchQuery["subject_id"].ToString()), fetchQuery["board_name"].ToString(), fetchQuery["standard"].ToString(), fetchQuery["subject"].ToString()));
                    }
                    return View(subjList);
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

        public List<Standard> GetStandards()
        {
            List<Standard> stdList = new List<Standard>();
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT standard.standard_id, standard.standard, board.board_name FROM (`standard` INNER JOIN board ON standard.board_id = board.board_id) WHERE standard.is_deleted = 0";
            MySqlDataReader fetchQuery;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                fetchQuery = null;
                Debug.WriteLine("Exception " + e);
            }
            while (fetchQuery.Read())
            {
                stdList.Add(new Standard(Convert.ToInt32(fetchQuery["standard_id"].ToString()), fetchQuery["board_name"].ToString(), fetchQuery["standard"].ToString()));
            }
            conn.Close();
            return (stdList);
        }

        public ActionResult Add()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    return View(GetStandards());
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
        [ValidateAntiForgeryToken]
        public ActionResult Add(FormCollection fc)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    int std_id = Convert.ToInt32(fc["std_id"]);
                    string subjName = Convert.ToString(fc["subject_name"]);
                    int user_id = (int)Session["user_id"];
                    DateTime time = DateTime.Now;

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "INSERT INTO `subject`(`standard_id`, `subject`, `created_by`, `created_at`) VALUES (@std_id, @subjName, @created_by, @created_at)";
                    query.Parameters.AddWithValue("@std_id", std_id);
                    query.Parameters.AddWithValue("@subjName", subjName);
                    query.Parameters.AddWithValue("@created_by", user_id);
                    query.Parameters.AddWithValue("@created_at", time);
                    int RowsAffected = -1;
                    try
                    {
                        conn.Open();
                        RowsAffected = query.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception " + e);
                    }
                    conn.Close();
                    return RedirectToAction("Index", "Subject");
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

        public Subject GetSubjectOnId(int id)
        {
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT subject.subject_id, subject.subject, board.board_name, standard.standard FROM (`subject` INNER JOIN (standard INNER Join board On standard.board_id = board.board_id) ON standard.standard_id = subject.standard_id) WHERE subject.subject_id = @id";
            query.Parameters.AddWithValue("@id", id);
            MySqlDataReader fetchQuery = null;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception " + e);
            }
            Subject subj = null;
            if (fetchQuery != null)
            {
                while (fetchQuery.Read())
                {
                    subj = new Subject(Convert.ToInt32(fetchQuery["subject_id"].ToString()), fetchQuery["board_name"].ToString(), fetchQuery["standard"].ToString(), fetchQuery["subject"].ToString());
                }
            }
            conn.Close();
            return subj;
        }

        public ActionResult Edit(int id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    Subject subj = GetSubjectOnId(id);
                    ViewBag.Id = subj.Id;
                    ViewBag.StandardName = subj.StdName;
                    ViewBag.BoardName = subj.BoardName;
                    ViewBag.SubjectName = subj.SubjectName;
                    return View(GetStandards());
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection fc)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    int subjId = Convert.ToInt32(fc["subject_id"].ToString());
                    string subjName = Convert.ToString(fc["subject_name"]);
                    int stdId = Convert.ToInt32(fc["std_id"].ToString());
                    int user_id = (int)Session["user_id"];
                    DateTime time = DateTime.Now;

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "UPDATE `subject` SET `standard_id`=@std_id,`subject`=@subjName,`updated_by`=@updated_by,`updated_at`=@updated_at WHERE subject_id = @subj_id";
                    query.Parameters.AddWithValue("@std_id", stdId);
                    query.Parameters.AddWithValue("@subjName", subjName);
                    query.Parameters.AddWithValue("@updated_by", user_id);
                    query.Parameters.AddWithValue("@updated_at", time);
                    query.Parameters.AddWithValue("@subj_id", subjId);

                    int RowsAffected = -1;
                    try
                    {
                        conn.Open();
                        RowsAffected = query.ExecuteNonQuery();
                        Debug.WriteLine(RowsAffected.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception " + e);
                    }
                    conn.Close();
                    return RedirectToAction("Index", "Subject");
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
        public ActionResult Delete(int id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    int user_id = (int)Session["user_id"];
                    DateTime time = DateTime.Now;
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "UPDATE `subject` SET `is_deleted`=1,`deleted_by`=@user_id,`deleted_at`=@time WHERE subject_id = @subj_id";
                    query.Parameters.AddWithValue("@user_id", user_id);
                    query.Parameters.AddWithValue("@time", time);
                    query.Parameters.AddWithValue("@subj_id", id);
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

    }
}