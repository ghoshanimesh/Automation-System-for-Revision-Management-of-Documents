using FinalProject.Controllers;
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
    public class DashBoardController : Controller
    {
        // GET: Admin/DashBoard
        public ActionResult Index()
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

        public ActionResult Read()
        {
            if (Session["set"] != null)
            {
                if ((bool)Session["set"] == true)
                {
                    MySqlConnection conn = HomeController.conn();
                    MySqlCommand query = conn.CreateCommand();
                    query.CommandText = "Select count(doc_version_id) as no_of_rec, MONTH(doc_revision_date) as mn from document_versions where branch_id IN (SELECT branch_id from branch where branch_owner_user_id=(select user_id from user where user_id = @user_id)) GROUP BY mn";
                    query.Parameters.AddWithValue("@user_id", (int)Session["user_id"]);
                    MySqlDataReader result = null;
                    try
                    {
                        conn.Open();
                        result = query.ExecuteReader();

                        Dictionary<string, int> res = new Dictionary<string, int>();
                        while (result.Read())
                        {
                            res.Add(result["mn"].ToString(), Convert.ToInt32(result["no_of_rec"]));
                        }
                        conn.Close();
                        return Json(JsonConvert.SerializeObject(res));
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