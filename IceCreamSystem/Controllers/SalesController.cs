using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;

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

            var products = db.Product.Where(x => x.CompanyId == companyId && x.NameProduct.StartsWith(search)).Select(x => new { IdProduct = x.IdProduct, NameProduct = x.NameProduct, SalePrice = x.SalePrice }).ToList();
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
                string[] productsInCookie = Request.Cookies.AllKeys[0].Split('/');
                List<SaleProduct> saleProducts = new List<SaleProduct>();
                decimal TotalPrice = 0M;
                if(productsInCookie.Length == 0 || (productsInCookie.Length == 1 && productsInCookie[0].Equals("__RequestVerificationToken")))
                {
                    //this sale has no product, so it will be deleted
                    Sale saleNoProducts = db.Sale.Find(sale.IdSale);
                    db.Sale.Remove(saleNoProducts);
                    db.SaveChanges();
                    TempData["message"] = "Unsaved Sale";
                }
                else
                {
                    for (int i = 0; i < productsInCookie.Length; i++)
                    {
                        if (!String.IsNullOrEmpty(productsInCookie[i]) && !productsInCookie[i].Equals("__RequestVerificationToken"))
                        {
                            string[] products = productsInCookie[i].Split(',');
                            int idProd = Convert.ToInt32(products[0]);
                            Product prod = db.Product.Where(x => x.CompanyId == companyId && x.IdProduct == idProd).FirstOrDefault();

                            SaleProduct saleProduct = new SaleProduct { ProductId = prod.IdProduct, SaleId = sale.IdSale, Amount = Convert.ToInt32(products[3]) };
                            decimal price = prod.SalePrice * saleProduct.Amount;

                            saleProducts.Add(saleProduct);

                            TotalPrice += price;
                            
                        }

                    }

                    db.SaleProduct.AddRange(saleProducts);
                    db.SaveChanges();
                    currentSale.TotalPrice = TotalPrice;
                    db.SaveChanges();
                    TempData["message"] = "Sale Saved With Pending Status";
                }
                return RedirectToAction("Index");
            }

            ViewBag.ProductId = new SelectList(db.Product, "IdProduct", "NameProduct");
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

        /*public ActionResult Delete(int? id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sale sale = db.Sale.Find(id);
            db.Sale.Remove(sale);
            db.SaveChanges();
            return RedirectToAction("Index");
        }*/

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
