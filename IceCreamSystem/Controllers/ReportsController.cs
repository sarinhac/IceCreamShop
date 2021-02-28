using IceCreamSystem.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IceCreamSystem.Controllers
{
    public class ReportsController : Controller
    {
        private Context db = new Context();
        public ActionResult Index()
        {
            int countSales = db.Sale.Count();
            ViewBag.CountSales = countSales;

            int countEmployees = db.Employee.Count();
            ViewBag.CountEmployees = countEmployees;
            
            int countStock = db.EntryStock.Count();
            ViewBag.CountStock = countStock;

            int countPayments = db.Payment.Count();
            ViewBag.CountPayments = countPayments;
            return View();
        }
    }
}