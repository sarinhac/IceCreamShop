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
    public class CreditCardsController : Controller
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
                        return View(db.CreditCard.Include(c => c.Company).ToList());
                    }
                    else if (Check.IsSupervisor(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.CreditCard.Where(c => c.CompanyId == idCompany).Include(c => c.Company).ToList());
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
                    CreditCard creditCard = db.CreditCard.Find(id);
                    if (creditCard == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, creditCard.CompanyId)))
                        return View(creditCard);
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
                            TempData["confirm"] = "NEW CREDIT CARD CREATED";
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
                    ViewBag.error = "COMPANY ALREADY REGISTERED, TRY ANOTHER NAME";

            }

        ReturnIfError:
            int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
            int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
            if (permission > 0)
            {
                if (Check.IsSuperAdmin(permission))
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", creditCard.IdCreditCard);
                else if (Check.IsSupervisor(permission))
                {
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                }
                return View(creditCard);
            }
            else
            {
                TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                return RedirectToAction("Home", "Employees");
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
                    CreditCard creditCard = db.CreditCard.Find(id);
                    if (creditCard == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission))
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                    else if (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, creditCard.CompanyId))
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

                    return View(creditCard);
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

                            int creditCardDB = db.CreditCard.Where(u => u.CompanyId == oldCreditCard.CompanyId && u.NameCreditCard.Equals(creditCard.NameCreditCard)).Count();

                            if (creditCardDB > 1)
                            {
                                trans.Rollback();
                                ViewBag.error = "CREDIT CARD ALREADY REGISTERED, TRY ANOTHER NAME";
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
            int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
            if(permission > 0)
            {
                if (Check.IsSuperAdmin(permission))
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                else if (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, creditCard.CompanyId))
                {
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                }
                return View(creditCard);
            }
            else
            {
                TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                return RedirectToAction("Home", "Employees");
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
                    CreditCard creditCard = db.CreditCard.Find(id);
                    if (creditCard == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, creditCard.CompanyId)))
                        return View(creditCard);
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
                    TempData["confirm"] = "SUCCESSFUL DELETE";
                }
                catch
                {
                    trans.Rollback();
                    TempData["error"] = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
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
                    CreditCard creditCard = db.CreditCard.Find(id);
                    if (creditCard == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && Check.IsSameCompany(idCompany, creditCard.CompanyId)))
                        return View(creditCard);
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
                    TempData["confirm"] = "SUCCESSFUL REACTIVATION";
                }
                catch
                {
                    trans.Rollback();
                    TempData["error"] = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
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
