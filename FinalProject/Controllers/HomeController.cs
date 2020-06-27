using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        public static MySqlConnection conn()
        {
            string connString = "server=localhost;port=3306;database=wordrev;username=root;password=";
            MySqlConnection conn = new MySqlConnection(connString);
            return conn;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}