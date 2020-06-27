using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

namespace FinalProject.Controllers
{
    public class RegisterController : Controller
    {
        // GET: Register
        
        public ActionResult Index()
        {
            string param = Request.QueryString["register"];
            if (param == "failed")
            {
                ViewBag.Message = "Failed";
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
            string exc = "", user_name = "", user_fullname = "",user_email="";
            int user_role = 2;
            int level = 0;
            MySqlConnection conn = HomeController.conn();
            MySqlCommand query = conn.CreateCommand();
            query.CommandText = "SELECT * FROM `user` WHERE user_name=@user_name OR user_email=@user_email";
            user_name = Convert.ToString(fc["user_name"]);
            user_email = Convert.ToString(fc["user_email"]);
            query.Parameters.AddWithValue("@user_name", user_name);
            query.Parameters.AddWithValue("@user_email", user_email);
            MySqlDataReader fetchQuery = null;
            try
            {
                conn.Open();
                fetchQuery = query.ExecuteReader();
                if (!fetchQuery.HasRows)
                {
                    exc = "";
                    user_role = 2;
                    level = 0;
                    conn = HomeController.conn();
                    query = conn.CreateCommand();
                    query.CommandText = "INSERT INTO `user`(`user_name`, `user_fullname`, `user_email`, `user_password`, `user_role`) VALUES (@user_name, @user_fullname, @user_email, @user_password, 2)";
                    user_name = Convert.ToString(fc["user_name"]);
                    user_fullname = Convert.ToString(fc["user_fullname"]);
                    user_email = Convert.ToString(fc["user_email"]);
                    string user_password = Convert.ToString(fc["user_password"]);

                    query.Parameters.AddWithValue("@user_name", user_name);
                    query.Parameters.AddWithValue("@user_fullname", user_fullname);
                    query.Parameters.AddWithValue("@user_email", user_email);
                    query.Parameters.AddWithValue("@user_password", user_password);
                    user_id = -1;
                    try
                    {
                        conn.Open();
                        query.ExecuteNonQuery();
                        user_id = (int)query.LastInsertedId;
                        level++;
                       
                    }
                    catch (MySql.Data.MySqlClient.MySqlException exception)
                    {
                        level = -5;
                        exc = exception.ToString();
                        Debug.WriteLine(exc + " exception caught");
                    }
                }
                else
                {
                    level = -2;
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException exception)
            {
                level = -5;
                exc = exception.ToString();
                Debug.WriteLine("Exception outer" + exc);
            }

            conn.Close();

            if (level == 1)
            {
                Session["set"] = true;
                Session["user_id"] = user_id;
                Session["email"] = user_email;
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
                user_role = 0;
            }
            if (level == -2)
            {
                return Redirect("/Register?register=failed");
            }
            Debug.WriteLine(user_role);
            if (user_role == 2)
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