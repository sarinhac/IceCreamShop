using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;

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

            var office = db.Office.Include(o => o.Company);
            return View(office.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Office office = db.Office.Find(id);
            if (office == null)
            {
                return HttpNotFound();
            }
            return View(office);
        }

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NameOffice,DescriptionOffice,Discount,CompanyId")] Office office)
        {
            if (ModelState.IsValid)
            {
                Office officeDb = db.Office.Where(c => c.CompanyId == office.CompanyId && c.NameOffice.Equals(office.NameOffice)).FirstOrDefault();

                if (officeDb == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = (int)Session["idUser"]; //who is login
                            db.Office.Add(office);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + office.NameOffice + " " + office.DescriptionOffice,
                                Who = idUser,
                                OfficeId = office.IdOffice
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "New Office Created";
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
                    TempData["message"] = "This Office already exists, try another name";

                return RedirectToAction("Index");
            }

        ReturnIfError:
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", office.CompanyId);
            return View(office);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Office office = db.Office.Find(id);
            if (office == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", office.CompanyId);
            return View(office);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdOffice,NameOffice,DescriptionOffice,Discount,CompanyId")] Office office)
        {
            if (ModelState.IsValid)
            {
                Office oldOffice = db.Office.Find(office.IdOffice);

                if (oldOffice == null)
                {
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }
                else if (!oldOffice.Equals(office))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = (int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + office.NameOffice + " " + office.DescriptionOffice,
                                Old = oldOffice.NameOffice + " " + oldOffice.DescriptionOffice,
                                Who = idUser,
                                OfficeId = oldOffice.IdOffice
                            };

                            oldOffice.NameOffice = office.NameOffice;
                            oldOffice.DescriptionOffice = office.DescriptionOffice;
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
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", office.CompanyId);
            return View(office);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Office office = db.Office.Find(id);
            if (office == null)
            {
                return HttpNotFound();
            }
            return View(office);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Office office = db.Office.Find(id);
            int idUser = (int)Session["idUser"];

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
                        Old = "ACTIVATED"
                    };
                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Successful Delete";
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

        public ActionResult Active(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Office office = db.Office.Find(id);
            if (office == null)
            {
                return HttpNotFound();
            }
            return View(office);
        }

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult ActiveConfirmed(int id)
        {
            Office office = db.Office.Find(id);
            int idUser = 1;// (int)Session["idUser"];

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
