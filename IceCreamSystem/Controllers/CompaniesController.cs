using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Services;

namespace IceCreamSystem.Controllers
{
    public class CompaniesController : Controller
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
                        return View(db.Company.ToList());
                    }
                    else if (Check.IsAdmin(permission))
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        return View(companies);
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
            {
                return RedirectToAction("Error500", "Error");
            }
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Company company = db.Company.Find(id);
                    if (company == null)
                        return RedirectToAction("Error404", "Error");
                    else if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && Check.IsSameCompany(idCompany, company.IdCompany)))
                        return View(company);
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

        public ActionResult Create()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    if (Check.IsSuperAdmin(permission))
                        return View();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NameCompany,FlAuthoritativeReceipt")] Company company)
        {
            if (ModelState.IsValid)
            {
                Company companyDb = db.Company.Where(c => c.NameCompany.Equals(company.NameCompany)).FirstOrDefault();
                if (companyDb == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = (int)Session["idUser"]; //who is login
                            db.Company.Add(company);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + company.NameCompany + " " + company.FlAuthoritativeReceipt,
                                Who = idUser,
                                CompanyId = company.IdCompany
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "NEW COMPANY CREATED";
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
                {
                    ViewBag.error = "COMPANY ALREADY REGISTERED, TRY ANOTHER NAME";
                    goto ReturnIfError;
                }
                return RedirectToAction("Index");
            }

        ReturnIfError:

            return View(company);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("Error500", "Error");

            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Company company = db.Company.Find(id);
                    if (company == null)
                        return RedirectToAction("Error404", "Error");
                    else if (Check.IsSuperAdmin(permission))
                        return View(company);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCompany,NameCompany,FlAuthoritativeReceipt")] Company company)
        {
            if (ModelState.IsValid)
            {
                Company oldCompany = db.Company.Find(company.IdCompany);

                if (oldCompany == null)
                    return RedirectToAction("Error500", "Error");
                else if (!oldCompany.Equals(company))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = (int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + company.NameCompany + " " + company.FlAuthoritativeReceipt,
                                Old = oldCompany.NameCompany + " " + oldCompany.FlAuthoritativeReceipt,
                                Who = idUser,
                                CompanyId = oldCompany.IdCompany
                            };

                            oldCompany.NameCompany = company.NameCompany;
                            oldCompany.FlAuthoritativeReceipt = company.FlAuthoritativeReceipt;
                            db.SaveChanges();
                            
                            int companyDb = db.Company.Where(c => c.NameCompany.Equals(company.NameCompany)).Count();

                            if (companyDb > 1)
                            {
                                trans.Rollback();
                                ViewBag.error = "NAME COMPANY ALREADY REGISTERED, TRY ANOTHER NAME";
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
            return View(company);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("Error500", "Error");

            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Company company = db.Company.Find(id);
                    if (company == null)
                        return RedirectToAction("Error404", "Error");
                    else if (Check.IsSuperAdmin(permission))
                        return View(company);
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
            Company company = db.Company.Find(id);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    company.DeactivateCompany();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        CompanyId = id,
                        New = "DISABLED",
                        Old = "ACTIVATED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "SUCCESSFUL DELETE";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    return RedirectToAction("Error500", "Error");
                }

            }

        }
        public ActionResult Active(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error500", "Error");
            }
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Company company = db.Company.Find(id);
                    if (company == null)
                        return RedirectToAction("Error404", "Error");
                    else if (Check.IsSuperAdmin(permission))
                        return View(company);
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
            Company company = db.Company.Find(id);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    company.ReactivateCompany();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        CompanyId = id,
                        New = "ACTIVATED",
                        Old = "DISABLED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "SUCCESSFUL REACVTIVATION";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    return RedirectToAction("Error500", "Error");
                }
            }
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
