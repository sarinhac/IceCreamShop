using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using IceCreamSystem.Models.Enum;
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

        #region Actions Login
        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "LoginUser,PasswordUser")] Employee employee)
        {
            var currentUser = db.Employee.SingleOrDefault(
                u => u.LoginUser.Equals(employee.LoginUser));

            if (currentUser != null)
            {
                if (HashService.ValidatePassword(employee.PasswordUser, currentUser.PasswordUser))
                {
                    Session.Add("userName", currentUser.NameEmployee);
                    Session.Add("idUser", currentUser.IdEmployee);
                    Session.Add("idCompany", currentUser.CompanyId);
                    Session.Add("permission", currentUser.Permission);

                    return View("Home", Session);
                }
            }

            ViewBag.error = "Login or Password Invalid, Please try again";
            return View();
        }
        #endregion

        #region CRUD

        #region Actions Create 
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
        public ActionResult Create([Bind(Include = "NameEmployee, Birth, Admission, Salary, OfficeId, CompanyId, HaveLogin, Permission, LoginUser, PasswordUser")] Employee employee,
            [Bind(Include = "Cep, Logradouro, Numero, Bairro, Cidade, Uf")] Address address)
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
                var currentUser = db.Employee.SingleOrDefault(
                u => u.LoginUser.Equals(employee.LoginUser));

                if (currentUser == null && employee.LoginUser != null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            #region SAVE NEW EMPLOYEE
                            int idUser = (int)Session["idUser"]; //who is login
                            if (employee.HaveLogin)
                                employee.PasswordUser = HashService.HashPassword(employee.PasswordUser);

                            db.Address.Add(address);
                            db.SaveChanges();

                            employee.AddressId = address.IdAddress;
                            db.Employee.Add(employee);
                            db.SaveChanges();

                            foreach (Phone p in phones)
                                p.EmployeeId = employee.IdEmployee;

                            db.Phone.AddRange(phones);
                            db.SaveChanges();

                            Log log = new Log
                            {
                                New = employee.NameEmployee + "/BD " + employee.Birth.ToString("dd/MM/yy") + "/AD " + employee.Admission.ToString("dd/MM/yy") + "/ " + employee.Salary
                                + "/Office " + employee.OfficeId + "/Address " + address.Cep + "/ " + address.Numero
                                + "/ " + address.Cidade + "/ " + address.Uf + "/ " + employee.LoginUser != null ? employee.LoginUser : "",

                                Who = idUser,
                                EmployeeId = employee.IdEmployee
                            };
                            db.Log.Add(log);
                            db.SaveChanges();

                            trans.Commit();
                            return RedirectToAction("Index");
                            #endregion
                        }
                        catch
                        {
                            trans.Rollback();
                            goto ReturnIfError;
                        }
                    }
                }
                goto ReturnIfError;
            }

        ReturnIfError:
            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;

            ViewBag.Phones = phones;

            IEnumerable<SelectListItem> typePhone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = typePhone;

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", employee.CompanyId);
            ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", employee.OfficeId);
            return View(employee);
        }

        #endregion

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

        #region Actions Edit
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

            List<Phone> phones = db.Phone.Where(p => p.EmployeeId == id).ToList();
            ViewBag.Phones = phones;

            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;

            IEnumerable<SelectListItem> typePhone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = typePhone;
           
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
        #endregion

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
