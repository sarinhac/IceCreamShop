using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Models.Enum;
using IceCreamSystem.Models.ViewModels;
using IceCreamSystem.Services;

namespace IceCreamSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private Context db = new Context();

        public ActionResult Index()
        {
            var employee = db.Employee.Include(e => e.Address).Include(e => e.Company).Include(e => e.Office);
            return View(employee.ToList());
        }


        #region CRUD
        public ActionResult Create()
        {
            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;

            IEnumerable<SelectListItem> phone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = phone;

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
            ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice");
            return View();
        }

        public ActionResult AddOtherPhone()
        {
            IEnumerable<SelectListItem> phone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = phone;

            return PartialView("_Phone");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            #region CATCHING ALL PHONES IN REQUEST
            List<Phone> phones = new List<Phone>();
            List<string> request = Request.Form.ToString().Split('&').Where(p => p.Contains("DDD") || p.Contains("TypePhone") || p.Contains("Number")).ToList();

            for (int i = 0; i < request.Count; i++)
            {
                Phone phoneRequest = new Phone();

                if (request[i].Contains("TypePhone"))
                {
                    phoneRequest.TypePhone = request[i].Replace("TypePhone=", "").ToString().Equals("Mobile") ? (TypePhone)1 : (TypePhone)2; //1 => Mobile | 2 => Landline

                    int index = request.IndexOf(request.Where(p => p.ToString().Contains("DDD=")).FirstOrDefault());
                    phoneRequest.DDD = request[index].Replace("DDD=", "");
                    request[index] = request[index].Replace("DDD=", "");

                    index = request.IndexOf(request.Where(p => p.ToString().Contains("Number=")).FirstOrDefault());
                    phoneRequest.Number = request[index].Replace("Number=", "");
                    request[index] = request[index].Replace("Number=", "");

                    phones.Add(phoneRequest);

                }
                else
                    break;

            }
            #endregion

            if (ModelState.IsValid)
            {
                db.Employee.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;

            IEnumerable<SelectListItem> phone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = phone;

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", employee.CompanyId);
            ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", employee.OfficeId);
            return View(employee);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employee.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employee.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", employee.CompanyId);
            ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", employee.OfficeId);
            return View(employee);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdEmployee,NameEmployee,Birth,Admission,Salary,AddressId,OfficeId,CompanyId,HaveLogin,Permission,LoginUser,PasswordUser,Status,Created")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", employee.CompanyId);
            ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", employee.OfficeId);
            return View(employee);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employee.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employee.Find(id);
            employee.DeactivateEmployee();
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Active(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employee.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult ActiveConfirmed(int id)
        {
            Employee employee = db.Employee.Find(id);
            employee.ReactivateEmployee();
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion

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
