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
    public class BoardController : Controller
    {
        // GET: Admin/Board
        public ActionResult Index()
        {
            List<Board> boardList = new List<Board>();
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "SELECT * FROM `board` WHERE is_deleted = 0";
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
                        boardList.Add(new Board(Convert.ToInt32(fetchQuery["board_id"].ToString()), fetchQuery["board_name"].ToString()));
                    }
                    return View(boardList);
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

        public ActionResult Add()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
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
        [ValidateAntiForgeryToken]
        public ActionResult Add(FormCollection fc)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    string boardName = Convert.ToString(fc["board_name"]);
                    int user_id = (int)Session["user_id"];
                    DateTime time = DateTime.Now;

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "INSERT INTO `board`(`board_name`, `created_by`, `created_at`) VALUES (@name,@created_by,@created_at)";
                    query.Parameters.AddWithValue("@name", boardName);
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
                    return RedirectToAction("Index","Board");
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

        public string GetBoardNameOnId(int id)
        {
            string name = "";
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM `board` WHERE board_id = @id";
            query.Parameters.AddWithValue("@id",id);
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
            if (fetchQuery != null)
            {
                while (fetchQuery.Read())
                {
                    name = fetchQuery["board_name"].ToString();
                }
            }
            conn.Close();
            return name;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FormCollection fc)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    string boardName = Convert.ToString(fc["board_name"]);
                    int id = Convert.ToInt32(fc["board_id"].ToString());
                    int user_id = (int)Session["user_id"];
                    DateTime time = DateTime.Now;

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "UPDATE `board` SET `board_name`=@name,`updated_by`=@updated_by,`updated_at`=@updated_at WHERE board_id = @id";
                    query.Parameters.AddWithValue("@name", boardName);
                    query.Parameters.AddWithValue("@updated_by", user_id);
                    query.Parameters.AddWithValue("@updated_at", time);
                    query.Parameters.AddWithValue("@id", id);

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
                    return RedirectToAction("Index", "Board");
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

        public ActionResult Edit(int id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    ViewBag.Id = id;
                    ViewBag.BoardName = GetBoardNameOnId(id);
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
                    query.CommandText = "UPDATE `board` SET `is_deleted`=1,`deleted_by`=@user_id,`deleted_at`=@time WHERE board_id = @board_id";
                    query.Parameters.AddWithValue("@user_id", user_id);
                    query.Parameters.AddWithValue("@time", time);
                    query.Parameters.AddWithValue("@board_id", id);
                    int RowsAffected = -1;
                    try
                    {
                        conn.Open();
                        RowsAffected = query.ExecuteNonQuery();
                        conn.Close();
                        return Json(new { result = "success"});
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                        return Json(new { result = "error" + e.ToString()});
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