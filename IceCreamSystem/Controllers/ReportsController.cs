using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Models.Enum;
using IceCreamSystem.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsStockist(permission))
                    {
                        int countStock = db.EntryStock.Count();
                        ViewBag.CountStock = countStock;
                    }

                    if (Check.IsSeller(permission))
                    {
                        int countSales = db.Sale.Count();
                        ViewBag.CountSales = countSales;

                        int countPayments = db.Payment.Count();
                        ViewBag.CountPayments = countPayments;
                    }

                    if (Check.IsAdmin(permission))
                    {
                        int countEmployees = db.Employee.Count();
                        ViewBag.CountEmployees = countEmployees;

                    }

                    return View();
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        #region EMPLOYEE'S REPORTS
        public ActionResult ReportsEmployees()
        {
            return View();
        }

        public ActionResult ActiveEmployeesReport()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsAdmin(permission))
                    {
                        List<Employee> activeEmployee = db.Employee.Where(e => e.CompanyId == idCompany && e.Status == (StatusGeneral)1).ToList();

                        double count = db.Employee.Where(e => e.CompanyId == idCompany).Count();
                        double count2 = activeEmployee.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        return View(activeEmployee);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult InactiveEmployeesReport()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsAdmin(permission))
                    {
                        List<Employee> inactiveEmployee = db.Employee.Where(e => e.CompanyId == idCompany && e.Status == 0).ToList();

                        double count = db.Employee.Where(e => e.CompanyId == idCompany).Count();
                        double count2 = inactiveEmployee.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        return View(inactiveEmployee);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }
        #endregion,

        #region STOCK'S REPORTS
        public ActionResult ReportsStock()
        {
            return View();
        }
        public ActionResult LowStock()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsStockist(permission))
                    {
                        List<Product> lowStock = db.Product.Where(e => e.CompanyId == idCompany && e.Status == (StatusGeneral)1 && e.AmountStock < e.MinStock).ToList();

                        double count = db.Product.Where(e => e.CompanyId == idCompany && e.Status == (StatusGeneral)1).Count();
                        double count2 = lowStock.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        return View(lowStock);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult RecentEntryStock()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsStockist(permission))
                    {
                        DateTime date = DateTime.Now.AddDays(-7);
                        DateTime dateNow = DateTime.Now;

                        List<EntryStock> recentEntryStock = db.EntryStock.Where(e => e.CompanyId == idCompany && e.Created >= date && e.Created <= dateNow).ToList();

                        double count = db.EntryStock.Where(e => e.CompanyId == idCompany && e.Created >= date && e.Created <= dateNow).Count();
                        double count2 = recentEntryStock.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        return View(recentEntryStock);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult CurrentStock()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsStockist(permission))
                    {
                        List<Product> currentStock = db.Product.Where(e => e.CompanyId == idCompany && e.Status == (StatusGeneral)1).Include(p => p.Category).Include(p => p.UnitMeasure).ToList();

                        return View(currentStock);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult CloseExpiration()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsStockist(permission))
                    {
                        DateTime date = DateTime.Now.AddDays(30);
                        DateTime dateNow = DateTime.Now;

                        List<EntryStock> closeExpiration = db.EntryStock.Where(e => e.CompanyId == idCompany && e.ExpirationDate > dateNow && e.ExpirationDate < date).ToList();

                        double count = db.EntryStock.Where(e => e.CompanyId == idCompany && e.ExpirationDate > dateNow).Count();
                        double count2 = closeExpiration.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        return View(closeExpiration);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }
        #endregion

        #region SALE'S REPORTS
        public ActionResult ReportsSales()
        {
            return View();
        }

        public ActionResult UnfinishedSales()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSeller(permission))
                    {
                        string[] period = Request.Cookies["period"] == null ? null : Request.Cookies["period"].Value.Split('-');
                        DateTime initialDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now.AddDays(-30) : Convert.ToDateTime(period[0]).AddHours(1);
                        DateTime finalDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now : Convert.ToDateTime(period[1]);

                        List<Sale> unfinishedSales = db.Sale.Where(e => e.CompanyId == idCompany && e.Created >= initialDate && e.Created <= finalDate && e.Status != (SaleStatus)2).ToList();

                        double count = db.Sale.Where(e => e.CompanyId == idCompany && e.Created >= initialDate && e.Created <= finalDate).Count();
                        double count2 = unfinishedSales.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        ViewBag.Total = unfinishedSales.Sum(x => x.TotalPrice);

                        return View(unfinishedSales);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult SalesFinished()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSeller(permission))
                    {
                        string[] period = Request.Cookies["period"] == null ? null : Request.Cookies["period"].Value.Split('-');
                        DateTime initialDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now.AddDays(-30) : Convert.ToDateTime(period[0]).AddHours(1);
                        DateTime finalDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now : Convert.ToDateTime(period[1]);

                        List<Sale> salesFinished = db.Sale.Where(e => e.CompanyId == idCompany && e.Created >= initialDate && e.Created <= finalDate && e.Status == (SaleStatus)2).ToList();

                        double count = db.Sale.Where(e => e.CompanyId == idCompany && e.Created >= initialDate && e.Created <= finalDate).Count();
                        double count2 = salesFinished.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        ViewBag.Total = salesFinished.Sum(x => x.TotalPrice);

                        return View(salesFinished);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult CanceledSales()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSeller(permission))
                    {
                        string[] period = Request.Cookies["period"] == null ? null : Request.Cookies["period"].Value.Split('-');
                        DateTime initialDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now.AddDays(-30) : Convert.ToDateTime(period[0]).AddHours(1);
                        DateTime finalDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now : Convert.ToDateTime(period[1]);

                        List<Sale> canceledSales = db.Sale.Where(e => e.CompanyId == idCompany && e.Created >= initialDate && e.Created <= finalDate && e.Status == (SaleStatus)3).ToList();

                        double count = db.Sale.Where(e => e.CompanyId == idCompany && e.Created >= initialDate && e.Created <= finalDate).Count();
                        double count2 = canceledSales.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        ViewBag.Total = canceledSales.Sum(x => x.TotalPrice);

                        return View(canceledSales);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }
        #endregion

        #region PAYMENT'S REPORTS
        public ActionResult ReportsPayments()
        {
            return View();
        }

        public ActionResult LatePayments()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSupervisor(permission))
                    {
                        string[] period = Request.Cookies["period"] == null ? null : Request.Cookies["period"].Value.Split('-');
                        DateTime initialDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now.AddDays(-30) : Convert.ToDateTime(period[0]).AddHours(1);
                        DateTime finalDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now : Convert.ToDateTime(period[1]);

                        List<Payment> latePayments = db.Payment.Where(e => e.CompanyId == idCompany && e.Status == (StatusPayment)3 && e.forecastDatePayment >= initialDate && e.forecastDatePayment <= finalDate).ToList();

                        double count = db.Payment.Where(e => e.CompanyId == idCompany && e.forecastDatePayment >= initialDate && e.forecastDatePayment <= finalDate).Count();
                        double count2 = latePayments.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        ViewBag.Total = latePayments.Sum(x => x.InstallmentPrice);

                        return View(latePayments);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }

        }

        public ActionResult PaidPayments()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSupervisor(permission))
                    {
                        string[] period = Request.Cookies["period"] == null ? null : Request.Cookies["period"].Value.Split('-');
                        DateTime initialDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now.AddDays(-30) : Convert.ToDateTime(period[0]).AddHours(1);
                        DateTime finalDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now : Convert.ToDateTime(period[1]);

                        List<Payment> paidPayments = db.Payment.Where(e => e.CompanyId == idCompany && e.Status == (StatusPayment)1 && e.forecastDatePayment >= initialDate && e.forecastDatePayment <= finalDate).ToList();

                        double count = db.Payment.Where(e => e.CompanyId == idCompany && e.forecastDatePayment >= initialDate && e.forecastDatePayment <= finalDate).Count();
                        double count2 = paidPayments.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        ViewBag.Total = paidPayments.Sum(x => x.InstallmentPrice);

                        return View(paidPayments);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult UpcomingPayments()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSupervisor(permission))
                    {
                        string[] period = Request.Cookies["period"] == null ? null : Request.Cookies["period"].Value.Split('-');
                        DateTime initialDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now.AddDays(-30) : Convert.ToDateTime(period[0]).AddHours(1);
                        DateTime finalDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now : Convert.ToDateTime(period[1]);

                        List<Payment> upcomingPayments = db.Payment.Where(e => e.CompanyId == idCompany && e.Status == (StatusPayment)2 && e.forecastDatePayment >= initialDate && e.forecastDatePayment <= finalDate).ToList();

                        double count = db.Payment.Where(e => e.CompanyId == idCompany && e.forecastDatePayment >= initialDate && e.forecastDatePayment <= finalDate).Count();
                        double count2 = upcomingPayments.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        ViewBag.Total = upcomingPayments.Sum(x => x.InstallmentPrice);

                        return View(upcomingPayments);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult RefundedPayments()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSupervisor(permission))
                    {
                        string[] period = Request.Cookies["period"] == null ? null : Request.Cookies["period"].Value.Split('-');
                        DateTime initialDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now.AddDays(-30) : Convert.ToDateTime(period[0]).AddHours(1);
                        DateTime finalDate = String.IsNullOrEmpty(period[0]) ? DateTime.Now : Convert.ToDateTime(period[1]);

                        List<Payment> refundedPayments = db.Payment.Where(e => e.CompanyId == idCompany && e.Status == (StatusPayment)4 && e.Created >= initialDate && e.Created <= finalDate).ToList();

                        double count = db.Payment.Where(e => e.CompanyId == idCompany && e.Created >= initialDate && e.Created <= finalDate).Count();
                        double count2 = refundedPayments.Count;
                        double percent = count2 == 0 ? 0 : Math.Round((count2 / count * 100), 2);

                        ViewBag.Count = count2;
                        ViewBag.TotalCount = count;
                        ViewBag.Percent = percent;

                        ViewBag.Total = refundedPayments.Sum(x => x.InstallmentPrice);

                        return View(refundedPayments);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }
        #endregion
    }
}