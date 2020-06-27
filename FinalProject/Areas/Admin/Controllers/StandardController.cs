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
    public class StandardController : Controller
    {
        // GET: Admin/Standard
        public ActionResult Index()
        {
            List<Standard> stdList = new List<Standard>();
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
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
                    return View(stdList);
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

        public List<Board> GetBoards()
        {
            List<Board> boardList = new List<Board>();
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
            conn.Close();
            return boardList;
        }

        public ActionResult Add()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    return View(GetBoards());
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
                    int boardId = Convert.ToInt32(fc["board_id"]);
                    string standardName = Convert.ToString(fc["standard_name"]);
                    int user_id = (int)Session["user_id"];
                    DateTime time = DateTime.Now;

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "INSERT INTO `standard`(`board_id`, `standard`, `created_by`, `created_at`) VALUES (@boardId, @standardName, @created_by, @created_at)";
                    query.Parameters.AddWithValue("@boardId", boardId);
                    query.Parameters.AddWithValue("@standardName", standardName);
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
                    return RedirectToAction("Index", "Standard");
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

        public Standard GetStandardOnId(int id)
        {
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM (`standard` INNER JOIN board ON standard.board_id = board.board_id) WHERE standard_id = @id";
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
            Standard std = null;
            if (fetchQuery != null)
            {
                while (fetchQuery.Read())
                {
                    std = new Standard(Convert.ToInt32(fetchQuery["standard_id"].ToString()), fetchQuery["board_name"].ToString(), fetchQuery["standard"].ToString());
                }
            }
            conn.Close();
            return std;
        }

        public ActionResult Edit(int id)
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    Standard std = GetStandardOnId(id);
                    ViewBag.Id = std.Id;
                    ViewBag.StandardName = std.StandardName;
                    ViewBag.BoardName = std.BoardName;
                    return View(GetBoards());
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
                    int stdId = Convert.ToInt32(fc["standard_id"].ToString());
                    string stdName = Convert.ToString(fc["standard_name"]);
                    int boardId = Convert.ToInt32(fc["board_id"].ToString());
                    int user_id = (int)Session["user_id"];
                    DateTime time = DateTime.Now;

                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "UPDATE `standard` SET `board_id`=@boardId,`standard`=@stdName,`updated_by`=@updated_by,`updated_at`=@updated_at WHERE standard_id = @stdId";
                    query.Parameters.AddWithValue("@boardId", boardId);
                    query.Parameters.AddWithValue("@stdName", stdName);
                    query.Parameters.AddWithValue("@updated_by", user_id);
                    query.Parameters.AddWithValue("@updated_at", time);
                    query.Parameters.AddWithValue("@stdId", stdId);

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
                    return RedirectToAction("Index", "Standard");
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
                    query.CommandText = "UPDATE `standard` SET `is_deleted`=1,`deleted_by`=@user_id,`deleted_at`=@time WHERE standard_id = @std_id";
                    query.Parameters.AddWithValue("@user_id", user_id);
                    query.Parameters.AddWithValue("@time", time);
                    query.Parameters.AddWithValue("@std_id", id);
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