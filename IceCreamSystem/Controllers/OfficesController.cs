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
    public class OfficesController : Controller
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
                        return View(db.Office.Include(c => c.Company).ToList());
                    }
                    else if (Check.IsAdmin(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.Office.Where(o => o.CompanyId == idCompany).Include(c => c.Company).ToList());
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
                    Office office = db.Office.Find(id);

                    if (office == null)
                        return RedirectToAction("Error404", "Error");

                    else if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == office.CompanyId))
                        return View(office);
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
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                    else if (Check.IsAdmin(permission))
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
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
        public ActionResult Create([Bind(Include = "NameOffice,DescriptionOffice,Discount,CompanyId")] Office office)
        {
            int idCompany = (int)Session["idCompany"];
            int idUser = (int)Session["idUser"]; //who is login

            if (ModelState.IsValid)
            {
                Office officeDb = db.Office.Where(c => c.CompanyId == office.CompanyId && c.NameOffice.Equals(office.NameOffice)).FirstOrDefault();

                if (officeDb == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.Office.Add(office);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + office.NameOffice + " " + office.DescriptionOffice + " " + office.Discount,
                                Who = idUser,
                                OfficeId = office.IdOffice,
                                CompanyId = office.CompanyId
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "NEW OFFICE CREATED";
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
                {
                    ViewBag.error = "OFFICE ALREADY REGISTERED, TRY ANOTHER NAME";
                }
            }

        ReturnIfError:
            try
            {
                int permission = (int)Session["permission"];
                
                if (Check.IsSuperAdmin(permission))
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", office.CompanyId);
                else if (Check.IsAdmin(permission))
                {
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", office.CompanyId);
                }
                return View(office);
            }
            catch
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
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Office office = db.Office.Find(id);

                    if (office == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission))
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                    else if (Check.IsAdmin(permission) && idCompany == office.CompanyId)
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                    return View(office);
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
        public ActionResult Edit([Bind(Include = "IdOffice,NameOffice,DescriptionOffice,Discount,CompanyId")] Office office)
        {
            int idUser = (int)Session["idUser"]; //who is login
            int idCompany = (int)Session["idCompany"];

            if (ModelState.IsValid)
            {
                Office oldOffice = db.Office.Find(office.IdOffice);

                if (oldOffice == null)
                    return RedirectToAction("Error500", "Error");

                else if (!oldOffice.Equals(office))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + office.NameOffice + " " + office.DescriptionOffice + " " + office.Discount,
                                Old = oldOffice.NameOffice + " " + oldOffice.DescriptionOffice + " " + oldOffice.Discount,
                                Who = idUser,
                                OfficeId = oldOffice.IdOffice,
                                CompanyId = office.CompanyId
                            };

                            oldOffice.NameOffice = office.NameOffice;
                            oldOffice.DescriptionOffice = office.DescriptionOffice;
                            oldOffice.Discount = office.Discount;
                            db.SaveChanges();
                            
                            int officeDb = db.Office.Where(c => c.CompanyId == office.CompanyId && c.NameOffice.Equals(office.NameOffice)).Count();

                            if (officeDb > 1)
                            {
                                trans.Rollback();
                                ViewBag.error = "OFFICE ALREADY REGISTERED, TRY ANOTHER NAME";
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
            try
            {
                int permission = (int)Session["permission"];

                if (Check.IsSuperAdmin(permission))
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", office.CompanyId);
                else if (Check.IsAdmin(permission))
                {
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", office.CompanyId);
                }
                return View(office);
            }
            catch
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
                int idCompany = Session["idCompany"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["username"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Office office = db.Office.Find(id);

                    if (office == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == office.CompanyId))
                        return View(office);
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
            Office office = db.Office.Find(id);
            int idUser = (int)Session["idUser"];
            int idCompany = (int)Session["idCompany"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    office.DeactivateOffice();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        OfficeId = id,
                        New = "DISABLED",
                        Old = "ACTIVATED",
                        CompanyId = office.CompanyId
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
                    ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                    return RedirectToAction("Index");
                }
            }
        }

        public ActionResult Active(int? id)
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
                    Office office = db.Office.Find(id);

                    if (office == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == office.CompanyId))
                        return View(office);
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
            Office office = db.Office.Find(id);
            int idUser = (int)Session["idUser"];
            int idCompany = (int)Session["idCompany"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    office.ReactivateOffice();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        OfficeId = id,
                        New = "ACTIVATED",
                        Old = "DISABLED",
                        CompanyId = office.CompanyId
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "SUCCESSFUL REACTIVATION";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                    return RedirectToAction("Index");
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
