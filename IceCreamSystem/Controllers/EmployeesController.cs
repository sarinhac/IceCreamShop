using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Globalization;
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

            ViewBag.error = TempData["error"] != null ? TempData["error"].ToString() : null;
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
            }
            #endregion

            if (ModelState.IsValid)
            {
                if (employee.HaveLogin)
                {
                    var currentUser = db.Employee.SingleOrDefault(u => u.LoginUser.Equals(employee.LoginUser));
                    if (currentUser == null)
                        employee.PasswordUser = HashService.HashPassword(employee.PasswordUser);
                    else
                    {
                        ViewBag.error = "Please choose another Login User";
                        goto ReturnIfError;
                    }
                }

                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        #region SAVE NEW EMPLOYEE
                        int idUser = 1; //who is login

                        #region Insert new Address
                        //Using (System.Data.Entity) Add -> Adds the given entity to the context that it will be inserted into the database when SaveChanges is called.
                        db.Address.Add(address);
                        db.SaveChanges();
                        #endregion

                        #region Insert New Employee                   

                        employee.AddressId = address.IdAddress;
                        db.Employee.Add(employee);
                        db.SaveChanges();

                        #endregion

                        #region Insert news Phones with AddRange
                        foreach (Phone p in phones)
                            p.EmployeeId = employee.IdEmployee;

                        //Using (System.Data.Entity) AddRange -> Adds a collection of entities into context that it will be inserted into the database when SaveChanges is called.
                        db.Phone.AddRange(phones);
                        db.SaveChanges();
                        #endregion

                        #region Register Log
                        Log log = new Log
                        {
                            New = employee.NameEmployee + "-BD " + employee.Birth.ToString("dd/MM/yy") + "-AD " + employee.Admission.ToString("dd/MM/yy") + "-" + employee.Salary
                                + "-Office " + employee.OfficeId + "-Address " + address.Cep + "/" + address.Numero
                                + "/" + address.Cidade + "/" + address.Uf,
                            Who = idUser,
                            EmployeeId = employee.IdEmployee
                        };
                        db.Log.Add(log);
                        db.SaveChanges();
                        #endregion

                        trans.Commit();
                        return RedirectToAction("Index");
                        #endregion
                    }
                    catch
                    {
                        trans.Rollback();
                        ViewBag.error = "Sorry, but an error happened";
                        goto ReturnIfError;
                    }
                }
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

            IEnumerable<SelectListItem> typePhone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = typePhone;

            List<Phone> phones = db.Phone.Where(p => p.EmployeeId == id).ToList();
            if (phones != null && phones.Count > 0)
                ViewBag.Phones = phones;
            else
            {
                TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                return RedirectToAction("Index");
            }

            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", employee.CompanyId);
            ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", employee.OfficeId);

            return View(employee);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdEmployee,NameEmployee,Birth,Admission,Salary,AddressId,Address,OfficeId,CompanyId,HaveLogin,Permission")] Employee editEmployee)
        {
            Address editAddress = editEmployee.Address;
            editAddress.IdAddress = editEmployee.AddressId;

            #region CATCHING ALL PHONES IN REQUEST
            List<Phone> editPhones = new List<Phone>();
            List<string> request = Request.Form.ToString().Split('&').Where(p => p.Contains("DDD") || p.Contains("TypePhone") || p.Contains("Number") || p.Contains("IdPhone")).ToList();

            for (int i = 0; i < request.Count; i++)
            {
                Phone phoneRequest = new Phone();

                if (request[i].Contains("IdPhone"))
                {
                    phoneRequest.IdPhone = Convert.ToInt32(request[i].Replace("IdPhone=", ""));

                    int index = request.IndexOf(request.Where(p => p.ToString().Contains("TypePhone=")).FirstOrDefault());
                    phoneRequest.TypePhone = request[index].Replace("TypePhone=", "").ToString().Equals("Mobile") ? (TypePhone)1 : (TypePhone)2; //1 => Mobile | 2 => Landline
                    request[index] = request[index].Replace("TypePhone=", "");

                    index = request.IndexOf(request.Where(p => p.ToString().Contains("DDD=")).FirstOrDefault());
                    phoneRequest.DDD = request[index].Replace("DDD=", "");
                    request[index] = request[index].Replace("DDD=", "");

                    index = request.IndexOf(request.Where(p => p.ToString().Contains("Number=")).FirstOrDefault());
                    phoneRequest.Number = request[index].Replace("Number=", "");
                    request[index] = request[index].Replace("Number=", "");

                    editPhones.Add(phoneRequest);
                }
                else if(request[i].StartsWith("TypePhone=") || request[i].StartsWith("Number=") || request[i].StartsWith("DDD="))
                {
                    phoneRequest.TypePhone = request[i].Replace("TypePhone=", "").ToString().Equals("Mobile") ? (TypePhone)1 : (TypePhone)2; //1 => Mobile | 2 => Landline

                    int index = request.IndexOf(request.Where(p => p.ToString().Contains("DDD=")).FirstOrDefault());
                    phoneRequest.DDD = request[index].Replace("DDD=", "");
                    request[index] = request[index].Replace("DDD=", "");

                    index = request.IndexOf(request.Where(p => p.ToString().Contains("Number=")).FirstOrDefault());
                    phoneRequest.Number = request[index].Replace("Number=", "");
                    request[index] = request[index].Replace("Number=", "");

                    editPhones.Add(phoneRequest);
                }
            }
            #endregion

            if (ModelState.IsValid)
            {                
                if (!editEmployee.HaveLogin)
                    editEmployee.Permission = null;

                Employee oldEmployee = db.Employee.Find(editEmployee.IdEmployee);
                Address oldAddress = db.Address.Find(editAddress.IdAddress);
                List<Phone> oldPhones = db.Phone.Where(p => p.EmployeeId == editEmployee.IdEmployee).ToList();
                int idUser = 1;//(int)Session["idUser"];

                bool a = oldEmployee.Equals(editEmployee);
                bool b = oldAddress.Equals(editAddress);
                bool c = oldPhones.SequenceEqual(editPhones, new Phone());
                if (!oldEmployee.Equals(editEmployee) || !oldAddress.Equals(editAddress) || !oldPhones.SequenceEqual(editPhones, new Phone()))
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        Log log = new Log { Who = idUser, EmployeeId = editEmployee.IdEmployee, New = "", Old = "" };

                        try
                        {
                            if (!oldEmployee.Equals(editEmployee))
                            {
                                oldEmployee.NameEmployee = editEmployee.NameEmployee;
                                oldEmployee.Birth = editEmployee.Birth;
                                oldEmployee.Admission = editEmployee.Admission;
                                oldEmployee.Salary = editEmployee.Salary;
                                oldEmployee.OfficeId = editEmployee.OfficeId;
                                oldEmployee.CompanyId = editEmployee.CompanyId;
                                oldEmployee.HaveLogin = editEmployee.HaveLogin;
                                oldEmployee.Permission = editEmployee.Permission;

                                db.SaveChanges();

                                log.New += "E "+ editEmployee.NameEmployee + "/ " + editEmployee.Birth.ToString("dd/MM/yy") + "/ " + editEmployee.Admission.ToString("dd/MM/yy") + "/ " + editEmployee.Salary
                                + "/ " + editEmployee.OfficeId + "/ " + editEmployee.CompanyId + "/ " + Convert.ToByte(editEmployee.HaveLogin) + "/ " + Convert.ToInt32(editEmployee.Permission);

                                log.Old += "E " + oldEmployee.NameEmployee + "/ " + oldEmployee.Birth.ToString("dd/MM/yy") + "/ " + oldEmployee.Admission.ToString("dd/MM/yy") + "/ " + oldEmployee.Salary
                                + "/ " + oldEmployee.OfficeId + "/ " + oldEmployee.CompanyId + "/ " + Convert.ToByte(oldEmployee.HaveLogin) + "/ " + Convert.ToInt32(oldEmployee.Permission);
                            }
                            if (!oldAddress.Equals(editAddress))
                            {
                                oldAddress.Cep = editAddress.Cep;
                                oldAddress.Logradouro = editAddress.Logradouro;
                                oldAddress.Complemento = editAddress.Complemento;
                                oldAddress.Numero = editAddress.Numero;
                                oldAddress.Bairro = editAddress.Bairro;
                                oldAddress.Cidade = editAddress.Cidade;
                                oldAddress.Uf = editAddress.Uf;
                                
                                db.SaveChanges();

                                log.New += "P " + editAddress.Cep + "/ " + editAddress.Logradouro + "/ " + editAddress.Numero + "/ " + editAddress.Bairro
                                + "/ " + editAddress.Cidade + "/ " + editAddress.Uf;

                                log.Old += "P " + oldAddress.Cep + "/ " + oldAddress.Logradouro + "/ " + oldAddress.Numero + "/ " + oldAddress.Bairro
                                + "/ " + oldAddress.Cidade + "/ " + oldAddress.Uf;
                            }
                            if(!oldPhones.SequenceEqual(editPhones, new Phone()))
                            {

                               //List<Phone> phonesOff = oldPhones.Where(p => editPhones.All(ph => ph.IdPhone != p.IdPhone)).ToList();
                               List<Phone> phonesChanged = editPhones.Where(p => oldPhones.All(ph => ph.IdPhone == p.IdPhone && (!ph.DDD.Equals(p.DDD) || !ph.Number.Equals(p.Number) || !ph.TypePhone.Equals(p.TypePhone)))).ToList();
                               List<Phone> phonesNew = editPhones.Where(p => oldPhones.All(ph => ph.IdPhone != p.IdPhone)).ToList();

                                /*if (phonesOff != null && phonesOff.Count > 0)
                                {
                                    //trocar status
                                }*/
                                if(phonesChanged != null && phonesChanged.Count > 0)
                                {
                                    foreach (Phone phone in phonesChanged)
                                    {
                                        int index = oldPhones.IndexOf(oldPhones.Where(p => p.IdPhone == phone.IdPhone).FirstOrDefault());

                                        oldPhones[index].DDD = phone.DDD;
                                        oldPhones[index].Number = phone.Number;
                                        oldPhones[index].TypePhone = phone.TypePhone;
                                    }
                                } 
                                if(phonesNew != null && phonesNew.Count > 0)
                                {
                                    foreach (Phone p in phonesNew)
                                        p.EmployeeId = oldEmployee.IdEmployee;

                                    db.Phone.AddRange(phonesNew);
                                    
                                }

                                db.SaveChanges();

                                trans.Commit();
                                return RedirectToAction("Index");
                            }
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
                {
                    TempData["error"] = "No changes were recorded";
                    return RedirectToAction("Index");
                }
            }

        ReturnIfError:
            IEnumerable<SelectListItem> typePhone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = typePhone;
            ViewBag.Phones = editPhones;

            IEnumerable<SelectListItem> permission = new SelectList(Enum.GetValues(typeof(Permission)));
            ViewBag.Permission = permission;

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", editEmployee.CompanyId);
            ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", editEmployee.OfficeId);
            return View(editEmployee);
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
            Employee employee = db.Employee.Include(e => e.Address).Include(e => e.Company).Include(e => e.Office).Where(e => e.IdEmployee == id).FirstOrDefault();
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
