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
    public class CreditCardsController : Controller
    {
        private Context db = new Context();

        // GET: CreditCards
        public ActionResult Index()
        {
            var creditCard = db.CreditCard.Include(c => c.Company);
            return View(creditCard.ToList());
        }

        // GET: CreditCards/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditCard creditCard = db.CreditCard.Find(id);
            if (creditCard == null)
            {
                return HttpNotFound();
            }
            return View(creditCard);
        }

        // GET: CreditCards/Create
        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            return View();
        }

        // POST: CreditCards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdCreditCard,NameCreditCard,DescriptionCreditCard,RateCreditCard,CompanyId,Status,Created")] CreditCard creditCard)
        {
            if (ModelState.IsValid)
            {
                db.CreditCard.Add(creditCard);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", creditCard.CompanyId);
            return View(creditCard);
        }

        // GET: CreditCards/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditCard creditCard = db.CreditCard.Find(id);
            if (creditCard == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", creditCard.CompanyId);
            return View(creditCard);
        }

        // POST: CreditCards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCreditCard,NameCreditCard,DescriptionCreditCard,RateCreditCard,CompanyId,Status,Created")] CreditCard creditCard)
        {
            if (ModelState.IsValid)
            {
                db.Entry(creditCard).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", creditCard.CompanyId);
            return View(creditCard);
        }

        // GET: CreditCards/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditCard creditCard = db.CreditCard.Find(id);
            if (creditCard == null)
            {
                return HttpNotFound();
            }
            return View(creditCard);
        }

        // POST: CreditCards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CreditCard creditCard = db.CreditCard.Find(id);
            db.CreditCard.Remove(creditCard);
            db.SaveChanges();
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
