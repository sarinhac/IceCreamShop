﻿using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;

namespace IceCreamSystem.Controllers
{
    public class CreditCardsController : Controller
    {
        private Context db = new Context();

        public ActionResult Index()
        {
            ViewBag.message = TempData["message"] != null ? TempData["message"].ToString() : null;
            ViewBag.confirm = TempData["confirm"] != null ? TempData["confirm"].ToString() : null;
            ViewBag.error = TempData["error"] != null ? TempData["error"].ToString() : null;

            var creditCard = db.CreditCard.Include(c => c.Company);
            return View(creditCard.ToList());
        }

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

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NameCreditCard,DescriptionCreditCard,RateCreditCard,CompanyId")] CreditCard creditCard)
        {
            if (ModelState.IsValid)
            {
                CreditCard creditCardDB = db.CreditCard.Where(c => c.CompanyId == creditCard.CompanyId && c.NameCreditCard.Equals(creditCard.NameCreditCard)).FirstOrDefault();

                if (creditCardDB == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = (int)Session["idUser"]; //who is login

                            db.CreditCard.Add(creditCard);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + creditCard.NameCreditCard + " " + creditCard.DescriptionCreditCard + " " + creditCard.RateCreditCard,
                                Who = idUser,
                                CreditCardId = creditCard.IdCreditCard,
                                CompanyId = creditCard.CompanyId
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "New Credit Card Created";
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
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", creditCard.CompanyId);
            return View(creditCard);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCreditCard,NameCreditCard,DescriptionCreditCard,RateCreditCard,CompanyId")] CreditCard creditCard)
        {
            if (ModelState.IsValid)
            {
                CreditCard oldCreditCard = db.CreditCard.Find(creditCard.IdCreditCard);

                if (oldCreditCard == null)
                {
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }
                else if (!oldCreditCard.Equals(creditCard))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = (int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + creditCard.NameCreditCard + " " + creditCard.DescriptionCreditCard + " " + creditCard.RateCreditCard,
                                Old = oldCreditCard.NameCreditCard + " " + oldCreditCard.DescriptionCreditCard + " " + oldCreditCard.RateCreditCard,
                                Who = idUser,
                                CreditCardId = oldCreditCard.IdCreditCard,
                                CompanyId = oldCreditCard.CompanyId
                            };
                            oldCreditCard.NameCreditCard = creditCard.NameCreditCard;
                            oldCreditCard.DescriptionCreditCard = creditCard.DescriptionCreditCard;
                            oldCreditCard.RateCreditCard = creditCard.RateCreditCard;

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
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", creditCard.CompanyId);
            return View(creditCard);
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CreditCard creditCard = db.CreditCard.Find(id);
            using (var trans = db.Database.BeginTransaction())
            {
                int idUser = (int)Session["idUser"]; //who is login
                try
                {
                    creditCard.DeactivateCreditCard();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        CreditCardId = id,
                        CompanyId = creditCard.CompanyId,
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
            CreditCard creditCard = db.CreditCard.Find(id);
            if (creditCard == null)
            {
                return HttpNotFound();
            }
            return View(creditCard);
        }

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult AtiveConfirmed(int id)
        {
            CreditCard creditCard = db.CreditCard.Find(id);
            using (var trans = db.Database.BeginTransaction())
            {
                int idUser = (int)Session["idUser"]; //who is login
                try
                {
                    creditCard.ReactivateCreditCard();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        CreditCardId = id,
                        CompanyId = creditCard.CompanyId,
                        New = "ACTIVATED",
                        Old = "DISABLED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Successful Reactivation";
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