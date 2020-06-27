using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

namespace FinalProject.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            string param = Request.QueryString["login"];
            if (param == "failed")
            {
                ViewBag.Message = "Failed";
                return View();
            }else if (param == "loggedout")
            {
                ViewBag.Message = "LoggedOut";
                return View();
            }
            else
            {
                ViewBag.Message = "Initial";
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FormCollection fc)
        {
            int user_id = -1;
            string exc = "", user_name = "", user_fullname = "";
            int user_role = 0;
            int level = 0;
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM `user` WHERE user_email=@useremail AND user_password=@userpassword";
            string useremail = Convert.ToString(fc["email"]);
            string userpassword = Convert.ToString(fc["password"]);

            query.Parameters.AddWithValue("@useremail", useremail);
            query.Parameters.AddWithValue("@userpassword", userpassword);
            try
            {
                conn.Open();
                MySqlDataReader fetchQuery = query.ExecuteReader();
                while (fetchQuery.Read())
                {
                    user_id = Convert.ToInt32(fetchQuery["user_id"].ToString());
                    user_name = fetchQuery["user_name"].ToString();
                    user_fullname = fetchQuery["user_fullname"].ToString();
                    user_role = Convert.ToInt32(fetchQuery["user_role"].ToString());
                    level++;
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException exception)
            {
                level = -5;
                exc = exception.ToString();
            }
            conn.Close();
            if (level == 1)
            {
                Session["set"] = true;
                Session["user_id"] = user_id;
                Session["email"] = useremail;
                Session["level"] = level;
                Session["user_name"] = user_name;
                Session["user_fullname"] = user_fullname;
                Session["user_role"] = user_role;
            }
            else
            {
                Session["user_id"] = user_id;
                Session["set"] = false;
                Session["level"] = level;
                Session["exc"] = exc;
                Session["user_name"] = null;
                Session["user_fullname"] = null;
                Session["user_role"] = 0;
            }
            if (user_role == 1)
            {
                //Head Editor or Admin
                return Redirect("/Admin/DashBoard/");
            }else if(user_role == 2)
            {
                return Redirect("/Dashboard/");
            }
            else
            {
                return Redirect("/Login?login=failed");
            }
        }

    }
}