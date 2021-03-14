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
    public class UnitMeasuresController : Controller
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
                        return View(db.UnitMeasure.Include(c => c.Company).ToList());
                    }
                    else if (Check.IsSupervisor(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.UnitMeasure.Where(c => c.CompanyId == idCompany).Include(c => c.Company).ToList());
                    }
                    else if (Check.IsStockist(permission))
                        return View(db.UnitMeasure.Where(c => c.CompanyId == idCompany).Include(c => c.Company).ToList());
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
                    UnitMeasure unitMeasure = db.UnitMeasure.Find(id);

                    if (unitMeasure == null)
                        return RedirectToAction("Error404", "Error");

                    else if (Check.IsSuperAdmin(permission) || (Check.IsStockist(permission) && idCompany == unitMeasure.CompanyId))
                        return View(unitMeasure);
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
        public ActionResult Create([Bind(Include = "NameUnitMeasure,DescriptionUnitMeasure,CompanyId")] UnitMeasure unitMeasure)
        {
            int idCompany = (int)Session["idCompany"];
            int idUser = (int)Session["idUser"]; //who is login

            if (ModelState.IsValid)
            {
                UnitMeasure unitMeasureDb = db.UnitMeasure.Where(c => c.CompanyId == unitMeasure.CompanyId && c.NameUnitMeasure.Equals(unitMeasure.NameUnitMeasure)).FirstOrDefault();

                if (unitMeasureDb == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.UnitMeasure.Add(unitMeasure);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + unitMeasure.NameUnitMeasure + " " + unitMeasure.DescriptionUnitMeasure,
                                Who = idUser,
                                UnitMeasureId = unitMeasure.IdUnitMeasure,
                                CompanyId = unitMeasure.CompanyId
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "NEW UNIT MEASURE CREATED";

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
                    ViewBag.error = "UNIT MEASURE ALREADY REGISTERED, TRY ANOTHER NAME";
            }

        ReturnIfError:

            int permission = Session["permission"] != null ? (int)Session["permission"] : 0;

            if (permission > 0)
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
                return View(unitMeasure);
            }
            else
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
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    UnitMeasure unitMeasure = db.UnitMeasure.Find(id);

                    if (unitMeasure == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission))
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                    else if (Check.IsSupervisor(permission) && idCompany == unitMeasure.CompanyId)
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

                    return View(unitMeasure);
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
        public ActionResult Edit([Bind(Include = "IdUnitMeasure,NameUnitMeasure,DescriptionUnitMeasure,CompanyId")] UnitMeasure unitMeasure)
        {
            int idUser = (int)Session["idUser"]; //who is login
            int idCompany = (int)Session["idCompany"];
            if (ModelState.IsValid)
            {
                UnitMeasure oldUnit = db.UnitMeasure.Find(unitMeasure.IdUnitMeasure);

                if (oldUnit == null)
                {
                    ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                    return RedirectToAction("Index");
                }

                else if (!oldUnit.Equals(unitMeasure))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + unitMeasure.NameUnitMeasure + " " + unitMeasure.DescriptionUnitMeasure,
                                Old = oldUnit.NameUnitMeasure + " " + oldUnit.DescriptionUnitMeasure,
                                Who = idUser,
                                UnitMeasureId = oldUnit.IdUnitMeasure,
                                CompanyId = unitMeasure.CompanyId
                            };

                            oldUnit.NameUnitMeasure = unitMeasure.NameUnitMeasure;
                            oldUnit.DescriptionUnitMeasure = unitMeasure.DescriptionUnitMeasure;
                            db.SaveChanges();

                            int unitMeasureDB = db.UnitMeasure.Where(u => u.CompanyId == idCompany && u.NameUnitMeasure.Equals(unitMeasure.NameUnitMeasure)).Count();

                            if(unitMeasureDB > 1)
                            {
                                trans.Rollback();
                                ViewBag.error = "UNIT MEASURE ALREADY REGISTERED, TRY ANOTHER NAME";
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

            if(permission > 0)
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
                return View(unitMeasure);
            }
            else
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
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    UnitMeasure unitMeasure = db.UnitMeasure.Find(id);

                    if (unitMeasure == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && idCompany == unitMeasure.CompanyId))
                        return View(unitMeasure);
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
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            int idUser = (int)Session["idUser"];
            int idCompany = (int)Session["idCompany"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    unitMeasure.DeactivateUnitMeasure();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        UnitMeasureId = id,
                        New = "DISABLED",
                        Old = "ACTIVATED",
                        CompanyId = unitMeasure.CompanyId
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "SUCCESSFUL DELETE";
                }
                catch
                {
                    trans.Rollback();
                    ViewBag.error = "ERROR 500, TRAY AGAIN, IF THE ERROR PERSIST CONTACT THE SYSTEM SUPPLIER";
                }
                return RedirectToAction("Index");
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
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    UnitMeasure unitMeasure = db.UnitMeasure.Find(id);

                    if (unitMeasure == null)
                        return RedirectToAction("Error404", "Error");

                    if (Check.IsSuperAdmin(permission) || (Check.IsSupervisor(permission) && idCompany == unitMeasure.CompanyId))
                        return View(unitMeasure);
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
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            int idUser = (int)Session["idUser"];
            int idCompany = (int)Session["idCompany"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    unitMeasure.ReactivateUnitMeasure();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        UnitMeasureId = id,
                        New = "ACTIVATED",
                        Old = "DISABLED",
                        CompanyId = unitMeasure.CompanyId
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
