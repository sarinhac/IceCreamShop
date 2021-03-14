using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Models.Enum;
using IceCreamSystem.Services;

namespace IceCreamSystem.Controllers
{
    public class SalesController : Controller
    {
        private Context db = new Context();

        public ActionResult Index()
        {
            ViewBag.message = TempData["message"] != null ? TempData["message"].ToString() : null;
            ViewBag.confirm = TempData["confirm"] != null ? TempData["confirm"].ToString() : null;
            ViewBag.error = TempData["error"] != null ? TempData["error"].ToString() : null;

            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSuperAdmin(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.Sale.Include(s => s.Company).Include(s => s.Employee).ToList());
                    }
                    else if (Check.IsSeller(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.Sale.Where(s => s.CompanyId == idCompany).Include(s => s.Company).Include(s => s.Employee).ToList());
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
                    }
                }
                else
                {
                    TempData["error"] = "YOU ARE NOT LOGGED IN";
                    return RedirectToAction("LogIn", "Employees");
                }
            }
            catch
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("Error500", "Error");

            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Sale sale = db.Sale.Find(id);

                    if (sale == null)
                        return RedirectToAction("Error404", "Error");

                    else if (Check.IsSuperAdmin(permission) || (Check.IsSeller(permission) && idCompany == sale.CompanyId))
                    {
                        return View(sale);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["error"] = "YOU ARE NOT LOGGED IN";
                    return RedirectToAction("LogIn", "Employees");
                }
            }
            catch
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        public JsonResult GetProducts(string search)
        {
            int companyId = (int)Session["idCompany"];

            var products = db.Product.Where(x => x.CompanyId == companyId && x.NameProduct.StartsWith(search) && (x.AmountStock > x.MinStock || x.SellNegative == true)).Select(x => new { IdProduct = x.IdProduct, NameProduct = x.NameProduct, SalePrice = x.SalePrice, UnitMeasure = x.UnitMeasure.NameUnitMeasure }).ToList();
            return Json(products, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Create()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSeller(permission))
                    {
                        Sale sale = new Sale() { CompanyId = idCompany, EmployeeId = idUser };
                        db.Sale.Add(sale);
                        db.SaveChanges();
                        return View(sale);

                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["error"] = "YOU ARE NOT LOGGED IN";
                    return RedirectToAction("LogIn", "Employees");
                }
            }
            catch
            {
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Sale sale)
        {
            if (sale.IdSale > 0)
            {
                Sale currentSale = db.Sale.Find(sale.IdSale);
                int companyId = (int)Session["idCompany"];
                string[] productsInCookie = Request.Cookies["products"].Value.Split('/');

                if (productsInCookie == null || (productsInCookie.Length == 1 && string.IsNullOrEmpty(productsInCookie[0])) || productsInCookie.Length == 0)
                {
                    //this sale has no product, so it will be deleted
                    Sale saleNoProducts = db.Sale.Find(sale.IdSale);
                    db.Sale.Remove(saleNoProducts);
                    db.SaveChanges();
                    TempData["message"] = "UNSAVED SALE";
                }
                else
                {
                    List<SaleProduct> saleProducts = SalesProductsService.ReturnSaleProducts(productsInCookie, companyId, sale.IdSale);
                    decimal TotalPrice = SalesProductsService.GetTotalPrice(saleProducts);

                    db.SaleProduct.AddRange(saleProducts);
                    db.SaveChanges();
                    currentSale.TotalPrice = TotalPrice;
                    db.SaveChanges();
                    TempData["message"] = "SALE SAVED WITH PENDING STATUS";
                }

                return RedirectToAction("Index");
            }

            return View(sale);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("Error500", "Error");
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Sale sale = db.Sale.Find(id);
                    if (Check.IsSeller(permission))
                    {
                        List<SaleProduct> saleProducts = db.SaleProduct.Where(s => s.SaleId == id).ToList();
                        ViewBag.Products = saleProducts;

                        return View(sale);

                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["error"] = "YOU ARE NOT LOGGED IN";
                    return RedirectToAction("LogIn", "Employees");
                }
            }
            catch
            {
                return RedirectToAction("Error500", "Error");
            }
        }

        public ActionResult Cancel(int? id)
        {
            if (id == null)
                return RedirectToAction("Error500", "Error");
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Sale sale = db.Sale.Find(id);

                    if (sale == null)
                        return RedirectToAction("Error404", "Error");

                    else if (Check.IsSuperAdmin(permission) || (Check.IsSeller(permission) && idCompany == sale.CompanyId))
                    {
                        return View(sale);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["error"] = "YOU ARE NOT LOGGED IN";
                    return RedirectToAction("LogIn", "Employees");
                }
            }
            catch
            {
                return RedirectToAction("Error500", "Error");

            }
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int id)
        {
            #region CANCEL
            Sale sale = db.Sale.Find(id);
            int idUser = (int)Session["idUser"];
            int companyId = sale.CompanyId;

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    sale.CancelSale();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        CompanyId = companyId,
                        SaleId = sale.IdSale,
                        New = "Canceled",
                        Old = sale.Status.ToString().ToUpper()
                    };

                    db.Log.Add(log);
                    db.SaveChanges();

                    List<Payment> payments = db.Payment.Where(p => p.SaleId == sale.IdSale).ToList();
                    List<Log> logs = new List<Log>();

                    if (payments != null && payments.Count > 0)
                    {
                        for (int i = 0; i < payments.Count; i++)
                        {
                            payments[i].Status = (StatusPayment)4;
                            int idPayment = payments[i].IdPayment;

                            Log log1 = new Log
                            {
                                Who = idUser,
                                PaymentId = idPayment,
                                CompanyId = companyId,
                                SaleId = id,
                                New = "CANCELED",
                                Old = payments[i].Status.ToString().ToUpper()
                            };

                            logs.Add(log1);
                        }
                        db.SaveChanges();
                        db.Log.AddRange(logs);
                        db.SaveChanges();
                    }
                    else
                        throw new Exception();

                    trans.Commit();
                    TempData["confirm"] = "CANCELLATION COMPLETED";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    TempData["error"] = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                    return RedirectToAction("Index");
                }

            }
            #endregion
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
