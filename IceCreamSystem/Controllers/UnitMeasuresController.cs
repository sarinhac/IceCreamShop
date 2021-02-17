using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;

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

            var unitMeasure = db.UnitMeasure.Include(u => u.Company);
            return View(unitMeasure.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            if (unitMeasure == null)
            {
                return HttpNotFound();
            }
            return View(unitMeasure);
        }

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NameUnitMeasure,DescriptionUnitMeasure,CompanyId")] UnitMeasure unitMeasure)
        {
            if (ModelState.IsValid)
            {
                UnitMeasure unitMeasureDb = db.UnitMeasure.Where(c => c.CompanyId == unitMeasure.CompanyId && c.NameUnitMeasure.Equals(unitMeasure.NameUnitMeasure)).FirstOrDefault();

                if (unitMeasureDb == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = 1;//(int)Session["idUser"]; //who is login
                            db.UnitMeasure.Add(unitMeasure);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + unitMeasure.NameUnitMeasure + " " + unitMeasure.DescriptionUnitMeasure,
                                Who = idUser,
                                UnitMeasureId = unitMeasure.IdUnitMeasure
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "New Unit Measure Created";
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
                    TempData["message"] = "This Unit Measure already exists, try another name";

                return RedirectToAction("Index");
            }

        ReturnIfError:
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", unitMeasure.CompanyId);
            return View(unitMeasure);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            if (unitMeasure == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", unitMeasure.CompanyId);
            return View(unitMeasure);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdUnitMeasure,NameUnitMeasure,DescriptionUnitMeasure,CompanyId")] UnitMeasure unitMeasure)
        {
            if (ModelState.IsValid)
            {
                UnitMeasure oldUnit = db.UnitMeasure.Find(unitMeasure.IdUnitMeasure);

                if (oldUnit == null)
                {
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }
                else if (!oldUnit.Equals(unitMeasure))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = 1;//(int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + unitMeasure.NameUnitMeasure + " " + unitMeasure.DescriptionUnitMeasure,
                                Old = oldUnit.NameUnitMeasure + " " + oldUnit.DescriptionUnitMeasure,
                                Who = idUser,
                                UnitMeasureId = oldUnit.IdUnitMeasure
                            };

                            oldUnit.NameUnitMeasure = unitMeasure.NameUnitMeasure;
                            oldUnit.DescriptionUnitMeasure = unitMeasure.DescriptionUnitMeasure;
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
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", unitMeasure.CompanyId);
            return View(unitMeasure);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            if (unitMeasure == null)
            {
                return HttpNotFound();
            }
            return View(unitMeasure);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            int idUser = (int)Session["idUser"];

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
                return RedirectToAction("Index");
            }
        }

        public ActionResult Active(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            if (unitMeasure == null)
            {
                return HttpNotFound();
            }
            return View(unitMeasure);
        }

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult ActiveConfirmed(int id)
        {
            UnitMeasure unitMeasure = db.UnitMeasure.Find(id);
            int idUser = (int)Session["idUser"];

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
                        Old = "DISABLED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Successful Reactivation";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    TempData["error"] = "An error happened. Please try again";
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
