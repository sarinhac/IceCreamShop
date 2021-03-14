using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Services;

namespace IceCreamSystem.Controllers
{
    public class ProductsController : Controller
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
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSuperAdmin(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.Product.Include(c => c.Company).Include(p => p.Category).Include(p => p.UnitMeasure).ToList());
                    }
                    else if (Check.IsSupervisor(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.Product.Where(c => c.CompanyId == idCompany).Include(c => c.Company).Include(p => p.Category).Include(p => p.UnitMeasure).ToList());
                    }
                    else if (Check.IsStockist(permission))
                        return View(db.Product.Where(c => c.CompanyId == idCompany).Include(c => c.Company).Include(p => p.Category).Include(p => p.UnitMeasure).ToList());
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
                    Product product = db.Product.Find(id);

                    if (product == null)
                        return RedirectToAction("Error404", "Error");

                    else if (Check.IsSuperAdmin(permission) || (Check.IsStockist(permission) && idCompany == product.CompanyId))
                        return View(product);
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

        public JsonResult GetCategory(int? id)
        {
            var categories = db.Category.Where(x => x.CompanyId == id).Select(x => new { id = x.IdCategory, name = x.NameCategory }).ToList(); ;

            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnitMeasure(int? id)
        {
            var unitMeasures = db.UnitMeasure.Where(x => x.CompanyId == id).Select(x => new { id = x.IdUnitMeasure, name = x.NameUnitMeasure }).ToList(); ;

            return Json(unitMeasures, JsonRequestBehavior.AllowGet);
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
                        ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory");
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                        ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure");
                    }
                    else if (Check.IsSupervisor(permission))
                    {
                        #region Company
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                        #endregion

                        ViewBag.CategoryId = new SelectList(db.Category.Where(c => c.CompanyId == idCompany).ToList(), "IdCategory", "NameCategory");
                        ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure.Where(c => c.CompanyId == idCompany).ToList(), "IdUnitMeasure", "NameUnitMeasure");

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
        public ActionResult Create([Bind(Include = "NameProduct,DescriptionProduct,CostPrice,SalePrice,MinStock,SellNegative,AmountStock,CategoryId,UnitMeasureId,CompanyId")] Product product)
        {
            int idUser = (int)Session["idUser"]; //who is login
            int idCompany = (int)Session["idCompany"];

            if (ModelState.IsValid)
            {
                Product productDB = db.Product.Where(p => p.CompanyId == product.CompanyId && p.NameProduct.Equals(product.NameProduct)).FirstOrDefault();

                if (productDB == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.Product.Add(product);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + product.NameProduct + " " + product.DescriptionProduct + " " + product.CostPrice + " " + product.SalePrice +
                                 " " + product.MinStock + " " + product.SellNegative,
                                Who = idUser,
                                ProductId = product.IdProduct,
                                CompanyId = product.CompanyId,
                                CategoryId = product.CategoryId,
                                UnitMeasureId = product.UnitMeasureId
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "NEW PRODUCT CREATED";
                        }
                        catch
                        {
                            trans.Rollback();
                            ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                            goto ReturnIfError;
                        }
                    }
                    return RedirectToAction("Index");

                }
                else
                    ViewBag.error = "PRODUCT ALREADY REGISTERED, TRY ANOTHER NAME";

            }

        ReturnIfError:
            int permission = Session["permission"] != null ? (int)Session["permission"] : 0;

            if (permission > 0)
            {
                if (Check.IsSuperAdmin(permission))
                {
                    ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory", product.CategoryId);
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", product.CompanyId);
                    ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
                }
                else if (Check.IsSupervisor(permission) && idCompany == product.CompanyId)
                {
                    #region Category
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", product.CompanyId);
                    #endregion

                    #region Category and unit measure
                    ViewBag.CategoryId = new SelectList(db.Category.Where(c => c.CompanyId == idCompany).ToList(), "IdCategory", "NameCategory", product.CategoryId);
                    ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure.Where(c => c.CompanyId == idCompany).ToList(), "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
                    #endregion
                }
                return View(product);
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
                    Product product = db.Product.Find(id);

                    if (product == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission))
                    {
                        ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory", product.CategoryId);
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", product.CompanyId);
                        ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
                    }
                    else if (Check.IsSupervisor(permission) && idCompany == product.CompanyId)
                    {
                        #region Category
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", product.CompanyId);
                        #endregion

                        #region Category and unit measure
                        ViewBag.CategoryId = new SelectList(db.Category.Where(c => c.CompanyId == idCompany).ToList(), "IdCategory", "NameCategory", product.CategoryId);
                        ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure.Where(c => c.CompanyId == idCompany).ToList(), "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
                        #endregion
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                    return View(product);
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
        public ActionResult Edit([Bind(Include = "IdProduct,NameProduct,DescriptionProduct,CostPrice,SalePrice,MinStock,SellNegative,AmountStock,CategoryId,UnitMeasureId,CompanyId")] Product product)
        {
            int idUser = (int)Session["idUser"]; //who is login
            int idCompany = (int)Session["idCompany"];

            if (ModelState.IsValid)
            {
                Product oldProduct = db.Product.Find(product.IdProduct);

                if (oldProduct == null)
                {
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }
                else if (!oldProduct.Equals(product))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + oldProduct.NameProduct + " " + oldProduct.DescriptionProduct + " " + oldProduct.CostPrice + " " + oldProduct.SalePrice +
                                 " " + oldProduct.MinStock + " " + oldProduct.SellNegative,
                                Old = product.NameProduct + " " + product.DescriptionProduct + " " + product.CostPrice + " " + product.SalePrice +
                                 " " + product.MinStock + " " + product.SellNegative,
                                Who = idUser,
                                ProductId = oldProduct.IdProduct,
                                CompanyId = oldProduct.CompanyId,
                                CategoryId = oldProduct.CategoryId,
                                UnitMeasureId = oldProduct.UnitMeasureId
                            };

                            oldProduct.NameProduct = product.NameProduct;
                            oldProduct.DescriptionProduct = product.DescriptionProduct;
                            oldProduct.CostPrice = product.CostPrice;
                            oldProduct.SalePrice = product.SalePrice;
                            oldProduct.MinStock = product.MinStock;
                            oldProduct.SellNegative = product.SellNegative;
                            oldProduct.CategoryId = product.CategoryId;
                            oldProduct.UnitMeasureId = product.UnitMeasureId;

                            db.SaveChanges();

                            int productDB = db.Product.Where(u => u.CompanyId == idCompany && u.NameProduct.Equals(oldProduct.NameProduct)).Count();
                            if (productDB > 1)
                            {
                                trans.Rollback();
                                ViewBag.error = "PRODUCT ALREADY REGISTERED, TRY ANOTHER NAME";
                                goto ReturnIfError;
                            }

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

            if (permission > 0)
            {
                if (Check.IsSuperAdmin(permission))
                {
                    ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory", product.CategoryId);
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", product.CompanyId);
                    ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
                }
                else if (Check.IsSupervisor(permission) && idCompany == product.CompanyId)
                {
                    #region Category
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", product.CompanyId);
                    #endregion

                    #region Category and unit measure
                    ViewBag.CategoryId = new SelectList(db.Category.Where(c => c.CompanyId == idCompany).ToList(), "IdCategory", "NameCategory", product.CategoryId);
                    ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure.Where(c => c.CompanyId == idCompany).ToList(), "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
                    #endregion
                }
                return View(product);
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
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Product product = db.Product.Find(id);

                    if (product == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && idCompany == product.CompanyId))
                        return View(product);
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
            int idUser = (int)Session["idUser"];
            Product product = db.Product.Find(id);

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    product.DeactivateProduct();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        ProductId = id,
                        CompanyId = product.CompanyId,
                        CategoryId = product.CategoryId,
                        UnitMeasureId = product.UnitMeasureId,
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

        public ActionResult Active(int? id)
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
                    Product product = db.Product.Find(id);

                    if (product == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && idCompany == product.CompanyId))
                        return View(product);
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

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult ActiveConfirmed(int id)
        {
            int idUser = (int)Session["idUser"];
            Product product = db.Product.Find(id);

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    product.ReactivateProduct();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        ProductId = id,
                        CompanyId = product.CompanyId,
                        CategoryId = product.CategoryId,
                        UnitMeasureId = product.UnitMeasureId,
                        New = "ACTIVATED",
                        Old = "DISABLED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "SUCCESSFUL REACTIVATION";

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
