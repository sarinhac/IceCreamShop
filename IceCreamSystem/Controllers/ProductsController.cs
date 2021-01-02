using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;

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

            var product = db.Product.Include(p => p.Category).Include(p => p.Company).Include(p => p.UnitMeasure);
            return View(product.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory");
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NameProduct,DescriptionProduct,CostPrice,SalePrice,MinStock,SellNegative,AmountStock,CategoryId,UnitMeasureId,CompanyId")] Product product)
        {
            if (ModelState.IsValid)
            {
                Product productDB = db.Product.Where(p => p.NameProduct.Equals(product.NameProduct) && p.CompanyId == product.CompanyId).FirstOrDefault();

                if (productDB == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = (int)Session["idUser"]; //who is login

                            db.Product.Add(product);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + product.NameProduct + " " + product.DescriptionProduct + " " + product.CostPrice + " " + product.SalePrice +
                                 " " + product.MinStock + " " + product.SellNegative + " " + product.CategoryId + " " + product.UnitMeasureId,
                                Who = idUser,
                                ProductId = product.IdProduct,
                                CompanyId = product.CompanyId
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "New Product Created";
                        }
                        catch
                        {
                            trans.Rollback();
                            ViewBag.error = "Sorry, but an error happened, try again, if the error continues please contact your system supplier";
                            goto ReturnIfError;
                        }
                    }
                }
                else
                    TempData["message"] = "This Product already exists, try another name";

                return RedirectToAction("Index");

            }

        ReturnIfError:
            ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory", product.CategoryId);
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", product.CompanyId);
            ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
            return View(product);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory", product.CategoryId);
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", product.CompanyId);
            ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdProduct,NameProduct,DescriptionProduct,CostPrice,SalePrice,MinStock,SellNegative,AmountStock,CategoryId,UnitMeasureId,CompanyId")] Product product)
        {
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
                        int idUser = (int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + oldProduct.NameProduct + " " + oldProduct.DescriptionProduct + " " + oldProduct.CostPrice + " " + oldProduct.SalePrice +
                                 " " + oldProduct.MinStock + " " + oldProduct.SellNegative + " " + oldProduct.CategoryId + " " + oldProduct.UnitMeasureId,
                                Old = product.NameProduct + " " + product.DescriptionProduct + " " + product.CostPrice + " " + product.SalePrice +
                                 " " + product.MinStock + " " + product.SellNegative + " " + product.CategoryId + " " + product.UnitMeasureId,
                                Who = idUser,
                                ProductId = oldProduct.IdProduct,
                                CompanyId = oldProduct.CompanyId
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

                            db.Log.Add(log);
                            db.SaveChanges();

                            trans.Commit();
                            TempData["confirm"] = "Successful Changes";
                        }
                        catch
                        {
                            trans.Rollback();
                            ViewBag.error = "Sorry, but an error happened, try again, if the error continues please contact your system supplier";
                            goto ReturnIfError;
                        }
                    }
                }
                else
                    TempData["message"] = "No changes were recorded";

                return RedirectToAction("Index");
            }

        ReturnIfError:
            ViewBag.CategoryId = new SelectList(db.Category, "IdCategory", "NameCategory", product.CategoryId);
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", product.CompanyId);
            ViewBag.UnitMeasureId = new SelectList(db.UnitMeasure, "IdUnitMeasure", "NameUnitMeasure", product.UnitMeasureId);
            return View(product);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
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
                        New = "DISABLED",
                        Old = "ACTIVATED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Successful Delete";
                }
                catch
                {
                    trans.Rollback();
                    TempData["error"] = "An error happened. Please try again";
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Active(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
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
                        New = "ACTIVATED",
                        Old = "DISABLED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Successful Delete";
                }
                catch
                {
                    trans.Rollback();
                    TempData["error"] = "An error happened. Please try again";
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
