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
    public class EntryStocksController : Controller
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
                        return View(db.EntryStock.Where(s=>s.Status != (StatusStockSaleProduct) 3).Include(c => c.Company).Include(e => e.Product).ToList());
                    }
                    else if (Check.IsStockist(permission))
                    {
                        ViewBag.permission = true;
                        List<EntryStock> entryStocks = db.EntryStock.Where(o => o.CompanyId == idCompany && o.Status != (StatusStockSaleProduct)3).Include(c => c.Company).Include(e => e.Product).ToList();
                        return View(entryStocks);
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
                    EntryStock entryStock = db.EntryStock.Find(id);

                    if (entryStock == null)
                        return RedirectToAction("Error404", "Error");

                    else if (Check.IsSuperAdmin(permission) || (Check.IsStockist(permission) && idCompany == entryStock.CompanyId))
                        return View(entryStock);
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

        public JsonResult GetProducts(int? id)
        {
            var products = db.Product.Where(x => x.CompanyId == id).Select(x => new { id = x.IdProduct, name = x.NameProduct }).ToList(); ;

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
                    if (Check.IsSuperAdmin(permission))
                    {
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                        ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct");
                    }
                    else if (Check.IsStockist(permission))
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                        ViewBag.ProductId = new SelectList(db.Product.Where(p => p.CompanyId == idCompany), "IdProduct", "NameProduct");
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                    return View();
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
        public ActionResult Create([Bind(Include = "FabicationDate,ExpirationDate,ProductBatch,Amount,ProductId,CompanyId")] EntryStock entryStock)
        {
            if (ModelState.IsValid)
            {
                EntryStock entryStockDB = db.EntryStock.Where(p => p.CompanyId == entryStock.CompanyId && p.ProductId == p.ProductId
                && p.ProductBatch.Equals(entryStock.ProductBatch) && p.FabicationDate.Equals(entryStock.FabicationDate)
                && p.ExpirationDate.Equals(entryStock.ExpirationDate)).FirstOrDefault();

                if (entryStockDB == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = (int)Session["idUser"]; //who is login

                            db.EntryStock.Add(entryStock);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + entryStock.FabicationDate.ToString("dd/MM/yyyy") + " " + entryStock.ExpirationDate.ToString("dd/MM/yyyy") + " " + entryStock.ProductBatch
                                + " " + entryStock.Amount,
                                Who = idUser,
                                EntryStockId = entryStock.IdStock,
                                CompanyId = entryStock.CompanyId,
                                ProductId = entryStock.ProductId
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "NEW STOCK INSERTED";
                        }
                        catch
                        {
                            trans.Rollback();
                            ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                            goto ReturnIfError;
                        }
                    }

                }
                else
                    ViewBag.error = "THIS STOCK ALREADY INSERTED, TRY EDIT IT";

                return RedirectToAction("Index");
            }

        ReturnIfError:
            int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
            int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;

            if(permission > 0)
            {
                if (Check.IsSuperAdmin(permission))
                {
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", entryStock.CompanyId);
                    ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct", entryStock.ProductId);
                }
                else if (Check.IsSupervisor(permission))
                {
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);

                    ViewBag.ProductId = new SelectList(db.Product.Where(p => p.CompanyId == idCompany), "IdProduct", "NameProduct", entryStock.ProductId);
                }
                return View(entryStock);
            }
            else
            {
                TempData["error"] = "YOU ARE NOT LOGGED IN";
                return RedirectToAction("LogIn", "Employees");
            }

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
                    EntryStock stock = db.EntryStock.Find(id);

                    if (stock == null)
                        return RedirectToAction("Error404", "Error");

                    if (stock.Status != (StatusStockSaleProduct)1)
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }

                    if (Check.IsSuperAdmin(permission))
                    {
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", stock.CompanyId);
                        ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct", stock.ProductId);
                    }
                    else if (Check.IsStockist(permission) && idCompany == stock.CompanyId)
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", stock.CompanyId);
                        ViewBag.ProductId = new SelectList(db.Product.Where(p => p.CompanyId == idCompany), "IdProduct", "NameProduct", stock.ProductId);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                    return View(stock);
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
        public ActionResult Edit([Bind(Include = "IdStock, FabicationDate, ExpirationDate, ProductBatch, Amount, ProductId, CompanyId")] EntryStock stock)
        {
            if (ModelState.IsValid)
            {
                EntryStock oldStock = db.EntryStock.Find(stock.IdStock);

                if (oldStock == null)
                {
                    ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                    return RedirectToAction("Index");
                }
                else if (!oldStock.Equals(stock))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = (int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[C]" + stock.FabicationDate.ToString("dd/MM/yyyy") + " " + stock.ExpirationDate.ToString("dd/MM/yyyy") + " " + stock.ProductBatch
                                + " " + stock.Amount,
                                Old = oldStock.FabicationDate.ToString("dd/MM/yyyy") + " " + oldStock.ExpirationDate.ToString("dd/MM/yyyy") + " " + oldStock.ProductBatch
                                + " " + oldStock.Amount,
                                Who = idUser,
                                EntryStockId = oldStock.IdStock,
                                CompanyId = oldStock.CompanyId,
                                ProductId = stock.ProductId
                            };

                            oldStock.FabicationDate = stock.FabicationDate;
                            oldStock.ExpirationDate = stock.ExpirationDate;
                            oldStock.ProductBatch = stock.ProductBatch;
                            oldStock.Amount = stock.Amount;
                            //Can exchange the product
                            oldStock.ProductId = stock.ProductId;

                            db.SaveChanges();

                            db.Log.Add(log);
                            db.SaveChanges();

                            trans.Commit();
                            TempData["confirm"] = "SUCCESSFUL CHANGES";
                        }
                        catch
                        {
                            trans.Rollback();
                            ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                            goto ReturnIfError;
                        }
                    }
                }
                else
                    TempData["message"] = "NO CHANGES WERE RECORDED";

                return RedirectToAction("Index");
            }

        ReturnIfError:
            int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
            int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;

            if (permission > 0)
            {
                if (Check.IsSuperAdmin(permission))
                {
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", stock.CompanyId);
                    ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct", stock.ProductId);
                }
                else if (Check.IsSupervisor(permission))
                {
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);

                    ViewBag.ProductId = new SelectList(db.Product.Where(p => p.CompanyId == idCompany), "IdProduct", "NameProduct", stock.ProductId);
                }
                return View(stock);
            }
            else
            {
                TempData["error"] = "YOU ARE NOT LOGGED IN";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("Error500", "Error");

            try
            {
                int idUser = (int)Session["idUser"];
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];
                string userName = (string)Session["username"];

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    EntryStock entryStock = db.EntryStock.Find(id);

                    if (entryStock == null)
                        return RedirectToAction("Error404", "Error");


                    if (entryStock.Status != (StatusStockSaleProduct)1)
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }

                    else if (Check.IsSuperAdmin(permission) || (Check.IsStockist(permission) && idCompany == entryStock.CompanyId))
                        return View(entryStock);
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EntryStock stock = db.EntryStock.Find(id);
            using (var trans = db.Database.BeginTransaction())
            {
                int idUser = (int)Session["idUser"]; //who is login
                try
                {
                    stock.DeactivateStock();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        EntryStockId = id,
                        ProductId = stock.ProductId,
                        CompanyId = stock.CompanyId,
                        New = "DISABLED",
                        Old = "ACTIVATED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "SUCCESSFUL DELETE";
                }
                catch
                {
                    trans.Rollback();
                    ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                }
            }
            return RedirectToAction("Index");
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
