using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Models.Enum;

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

            var entryStock = db.EntryStock.Include(e => e.Company).Include(e => e.Product);
            return View(entryStock.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EntryStock entryStock = db.EntryStock.Find(id);
            if (entryStock == null)
            {
                return HttpNotFound();
            }
            return View(entryStock);
        }

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FabicationDate,ExpirationDate,ProductBatch,Amount,ProductId,CompanyId")] EntryStock entryStock)
        {
            if (ModelState.IsValid)
            {
                EntryStock entryStockDB = db.EntryStock.Where(p => p.CompanyId == entryStock.CompanyId && p.ProductId == p.ProductId && p.Status == (StatusGeneral)1
                && p.ProductBatch.Equals(entryStock.ProductBatch) && p.FabicationDate.Equals(entryStock.FabicationDate)
                && p.ExpirationDate.Equals(entryStock.ExpirationDate)).FirstOrDefault();

                if(entryStockDB == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = 1;// (int)Session["idUser"]; //who is login

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
                            TempData["confirm"] = "Updated stock";
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
                    TempData["message"] = "This Product Batch already registred, try another name";

                return RedirectToAction("Index");
            }

            ReturnIfError:
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", entryStock.CompanyId);
            ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct", entryStock.ProductId);
            return View(entryStock);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EntryStock stock = db.EntryStock.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", stock.CompanyId);
            ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct", stock.ProductId);
            return View(stock);
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
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }
                else if (!oldStock.Equals(stock))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = 1;// (int)Session["idUser"]; //who is login
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
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", stock.CompanyId);
            ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct", stock.ProductId);
            return View(stock);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EntryStock stock = db.EntryStock.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }
            return View(stock);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EntryStock stock = db.EntryStock.Find(id);
            using (var trans = db.Database.BeginTransaction())
            {
                int idUser = 1;// (int)Session["idUser"]; //who is login
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
