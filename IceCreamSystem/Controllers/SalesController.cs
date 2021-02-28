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

            var sale = db.Sale.Include(s => s.Company).Include(s => s.Employee);
            return View(sale.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sale.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            return View(sale);
        }

        public JsonResult GetProducts(string search)
        {
            int companyId = 1;// (int)Session["companyId"];

            var products = db.Product.Where(x => x.CompanyId == companyId && x.NameProduct.StartsWith(search)).Select(x => new { IdProduct = x.IdProduct, NameProduct = x.NameProduct, SalePrice = x.SalePrice, UnitMeasure = x.UnitMeasure.NameUnitMeasure }).ToList();
            return Json(products, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Create()
        {
            int companyId = 1;// (int)Session["companyId"];
            int employeeId = 1; //(int)Session["idUser"];
            Sale sale = new Sale() { CompanyId = companyId, EmployeeId = employeeId };
            db.Sale.Add(sale);
            db.SaveChanges();

            return View(sale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Sale sale)
        {
            if (sale.IdSale > 0)
            {
                Sale currentSale = db.Sale.Find(sale.IdSale);
                int companyId = 1;// (int)Session["companyId"];
                string[] productsInCookie = Request.Cookies["products"].Value.Split('/');
                
                if (productsInCookie == null || productsInCookie.Length == 0)
                {
                    //this sale has no product, so it will be deleted
                    Sale saleNoProducts = db.Sale.Find(sale.IdSale);
                    db.Sale.Remove(saleNoProducts);
                    db.SaveChanges();
                    TempData["message"] = "Unsaved Sale";
                }
                else
                {
                    List<SaleProduct> saleProducts = SalesProductsService.ReturnSaleProducts(productsInCookie, companyId, sale.IdSale);
                    decimal TotalPrice = SalesProductsService.GetTotalPrice(saleProducts);

                    db.SaleProduct.AddRange(saleProducts);
                    db.SaveChanges();
                    currentSale.TotalPrice = TotalPrice;
                    db.SaveChanges();
                    TempData["message"] = "Sale Saved With Pending Status";
                }

                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", sale.CompanyId);
            ViewBag.EmployeeId = new SelectList(db.Employee, "IdEmployee", "NameEmployee", sale.EmployeeId);
            return View(sale);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sale.Find(id);

            if (sale == null)
            {
                return HttpNotFound();
            }

            List<SaleProduct> saleProducts = db.SaleProduct.Where(s => s.SaleId == id).ToList();
            ViewBag.Products = saleProducts;
           
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", sale.CompanyId);
            ViewBag.EmployeeId = new SelectList(db.Employee, "IdEmployee", "NameEmployee", sale.EmployeeId);
            return View(sale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdSale,CompanyId,EmployeeId,Status,Created")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sale).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", sale.CompanyId);
            ViewBag.EmployeeId = new SelectList(db.Employee, "IdEmployee", "NameEmployee", sale.EmployeeId);
            return View(sale);
        }

        public ActionResult Cancel(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sale sale = db.Sale.Find(id);
            if (sale == null)
            {
                return HttpNotFound();
            }
            return View(sale);
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int id)
        {
            #region CANCEL
            Sale sale = db.Sale.Find(id);
            int idUser = 1;//(int)Session["idUser"];
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
                    TempData["confirm"] = "Cancellation completed";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    TempData["error"] = "An error happened. Please try again";
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
