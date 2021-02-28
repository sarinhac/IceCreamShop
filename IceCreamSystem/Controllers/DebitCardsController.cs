using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Services;

namespace IceCreamSystem.Controllers
{
    public class DebitCardsController : Controller
    {
        private Context db = new Context();

        public ActionResult Index()
        {
            ViewBag.message = TempData["message"] != null ? TempData["message"].ToString() : null;
            ViewBag.confirm = TempData["confirm"] != null ? TempData["confirm"].ToString() : null;
            ViewBag.error = TempData["error"] != null ? TempData["error"].ToString() : null;

            try
            {
                int idUser = (int)Session["idUser"];
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];
                string userName = (string)Session["username"];

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSuperAdmin(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.DebitCard.Include(c => c.Company).ToList());
                    }
                    else if (Check.IsSupervisor(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.DebitCard.Where(c => c.CompanyId == idCompany).Include(c => c.Company).ToList());
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
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

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = (int)Session["idUser"];
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];
                string userName = (string)Session["username"];

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    DebitCard debitCard = db.DebitCard.Find(id);
                    if (debitCard == null)
                    {
                        return HttpNotFound();
                    }

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, debitCard.CompanyId)))
                        return View(debitCard);
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
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

        public ActionResult Create()
        {
            try
            {
                int idUser = (int)Session["idUser"];
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];
                string userName = (string)Session["username"];

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSuperAdmin(permission))
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                    else if (Check.IsSupervisor(permission))
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NameDebitCard,DescriptionDebitCard,Rate,CompanyId")] DebitCard debitCard)
        {
            if (ModelState.IsValid)
            {
                DebitCard debitCardDB = db.DebitCard.Where(c => c.CompanyId == debitCard.CompanyId && c.NameDebitCard.Equals(debitCard.NameDebitCard)).FirstOrDefault();

                if (debitCardDB == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = (int)Session["idUser"]; //who is login

                            db.DebitCard.Add(debitCard);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + debitCard.NameDebitCard + " " + debitCard.DescriptionDebitCard + " " + debitCard.Rate,
                                Who = idUser,
                                DebitCardId = debitCard.IdDebitCard,
                                CompanyId = debitCard.CompanyId
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "New Debit Card Created";
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
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", debitCard.CompanyId);
            return View(debitCard);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = (int)Session["idUser"];
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];
                string userName = (string)Session["username"];

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    DebitCard debitCard = db.DebitCard.Find(id);
                    if (debitCard == null)
                    {
                        return HttpNotFound();
                    }

                    if (Check.IsSuperAdmin(permission))
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                    else if (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, debitCard.CompanyId))
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
                    }

                    return View(debitCard);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdDebitCard,NameDebitCard,DescriptionDebitCard,Rate,CompanyId")] DebitCard debitCard)
        {
            if (ModelState.IsValid)
            {
                DebitCard oldDebitCard = db.DebitCard.Find(debitCard.IdDebitCard);

                if (oldDebitCard == null)
                {
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }
                else if (!oldDebitCard.Equals(debitCard))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = (int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + debitCard.NameDebitCard + " " + debitCard.DescriptionDebitCard + " " + debitCard.Rate,
                                Old = oldDebitCard.NameDebitCard + " " + oldDebitCard.DescriptionDebitCard + " " + oldDebitCard.Rate,
                                Who = idUser,
                                DebitCardId = oldDebitCard.IdDebitCard,
                                CompanyId = oldDebitCard.CompanyId
                            };
                            oldDebitCard.NameDebitCard = debitCard.NameDebitCard;
                            oldDebitCard.DescriptionDebitCard = debitCard.DescriptionDebitCard;
                            oldDebitCard.Rate = debitCard.Rate;

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
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", debitCard.CompanyId);
            return View(debitCard);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = (int)Session["idUser"];
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];
                string userName = (string)Session["username"];

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    DebitCard debitCard = db.DebitCard.Find(id);
                    if (debitCard == null)
                    {
                        return HttpNotFound();
                    }

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, debitCard.CompanyId)))
                        return View(debitCard);
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DebitCard debitCard = db.DebitCard.Find(id);
            using (var trans = db.Database.BeginTransaction())
            {
                int idUser = (int)Session["idUser"]; //who is login
                try
                {
                    debitCard.DeactivateDebitCard();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        DebitCardId = id,
                        CompanyId = debitCard.CompanyId,
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
            try
            {
                int idUser = (int)Session["idUser"];
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];
                string userName = (string)Session["username"];

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    DebitCard debitCard = db.DebitCard.Find(id);
                    if (debitCard == null)
                    {
                        return HttpNotFound();
                    }

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, debitCard.CompanyId)))
                        return View(debitCard);
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
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

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult AtiveConfirmed(int id)
        {
            DebitCard debitCard = db.DebitCard.Find(id);
            using (var trans = db.Database.BeginTransaction())
            {
                int idUser = (int)Session["idUser"]; //who is login
                try
                {
                    debitCard.ReactivateDebitCard();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        DebitCardId = id,
                        CompanyId = debitCard.CompanyId,
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
