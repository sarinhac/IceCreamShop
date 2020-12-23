using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;

namespace IceCreamSystem.Controllers
{
    public class CategoriesController : Controller
    {
        private Context db = new Context();

        public ActionResult Index()
        {
            ViewBag.message = TempData["message"] != null ? TempData["message"].ToString() : null;
            ViewBag.confirm = TempData["confirm"] != null ? TempData["confirm"].ToString() : null;
            ViewBag.error = TempData["error"] != null ? TempData["error"].ToString() : null;

            var category = db.Category.Include(c => c.Company);
            return View(category.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        public ActionResult Create()
        {
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NameCategory,DescriptionCategory,CompanyId")] Category category)
        {
            if (ModelState.IsValid)
            {
                Category categoryDb = db.Category.Where(c => c.NameCategory.Equals(category.NameCategory) && c.CompanyId == category.CompanyId).FirstOrDefault();
                if (categoryDb == null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int idUser = (int)Session["idUser"]; //who is login
                            db.Category.Add(category);
                            db.SaveChanges();

                            #region Register Log
                            Log log = new Log
                            {
                                //[C] in DB refers to an Create
                                New = "[C]" + category.NameCategory + " " + category.DescriptionCategory,
                                Who = idUser,
                                CategoryId = category.IdCategory
                            };
                            db.Log.Add(log);
                            db.SaveChanges();
                            #endregion

                            trans.Commit();
                            TempData["confirm"] = "New Category Created";
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
                    TempData["message"] = "This Category already exists, try another name";

                return RedirectToAction("Index");
            }

        ReturnIfError:
            return View(category);

        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", category.CompanyId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdCategory,NameCategory,DescriptionCategory,CompanyId")] Category category)
        {
            if (ModelState.IsValid)
            {
                Category oldCategory = db.Category.Find(category.IdCategory);

                if (oldCategory == null)
                {
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }
                else if (!oldCategory.Equals(category))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        int idUser = (int)Session["idUser"]; //who is login
                        try
                        {
                            Log log = new Log
                            {
                                //[U] in DB refers to an Update
                                New = "[U]" + category.NameCategory + " " + category.DescriptionCategory,
                                Old = oldCategory.NameCategory + " " + oldCategory.DescriptionCategory,
                                Who = idUser,
                                CategoryId = oldCategory.IdCategory
                            };

                            oldCategory.NameCategory = category.NameCategory;
                            oldCategory.DescriptionCategory = category.DescriptionCategory;
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
                return View(category);

        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Category.Find(id);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    category.DeactivateCategory();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        CategoryId = id,
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
            Category category = db.Category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult ActiveConfirmed(int id)
        {
            Category category = db.Category.Find(id);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    category.ReactivateCategory();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        CategoryId = id,
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
