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
                        return View(db.Employee.Include(c => c.Company).Include(e => e.Office).Include(e => e.Address).ToList());
                    }
                    else if (Check.IsAdmin(permission))
                    {
                        ViewBag.permission = true;
                        return View(db.Employee.Where(o => o.CompanyId == idCompany).Include(c => c.Company).Include(e => e.Office).Include(e => e.Address).ToList());
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Home", "Employees");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        #region Actions Login
        public ActionResult Home()
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
                    return View();
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        public ActionResult Login()
        {
            ViewBag.error = TempData["error"] != null ? TempData["error"].ToString() : null;
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (!Check.IsLogOn(idUser, permission, idCompany, userName))
                    return View();
                else
                    return RedirectToAction("Home", "Employees");
            }
            catch
            {
                //Login
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "LoginUser,PasswordUser")] Employee employee)
        {
            var currentUser = db.Employee.SingleOrDefault(
                u => u.LoginUser.Equals(employee.LoginUser));

            if (currentUser != null && currentUser.Status == (StatusGeneral)1)
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

            ViewBag.error = "Login or Password Invalid or You Don't Have Access";
            return View();
        }

        public ActionResult Logout()
        {
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Session.RemoveAll();
                    return RedirectToAction("Login");
                }
                else
                    return RedirectToAction("Home");
            }
            catch
            {
                //Login
                return RedirectToAction("Login");
            }
        }
        #endregion



        #region CRUD

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Employee employee = db.Employee.Find(id);

                    if (employee == null)
                    {
                        return HttpNotFound();
                    }
                    else if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == employee.CompanyId) || Check.IsMe(idUser, employee.IdEmployee))
                    {
                        List<Phone> phones = db.Phone.Where(p => p.EmployeeId == id).ToList();
                        if (phones != null && phones.Count > 0)
                            ViewBag.Phones = phones;

                        return View(employee);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }


        #region CREATE ACTIONS
        public JsonResult GetOffices(int? id)
        {

            var offices =  db.Office.Where(x => x.CompanyId == id).Select(x => new { id = x.IdOffice, name = x.NameOffice }).ToList();;

            return Json(offices, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AddOtherPhone()
        {
            IEnumerable<SelectListItem> phone = new SelectList(Enum.GetValues(typeof(TypePhone)));
            ViewBag.TypePhone = phone;

            return PartialView("_Phone");
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
                    {
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany");
                        ViewBag.OfficeId = new SelectList(db.Office.OrderBy(c => c.CompanyId), "IdOffice", "NameOffice");
                        IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission)));
                        ViewBag.Permission = typePermission;

                    }
                    else if (Check.IsAdmin(permission))
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", idCompany);
                        ViewBag.OfficeId = new SelectList(db.Office.Where(o => o.CompanyId == idCompany), "IdOffice", "NameOffice");

                        IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission))).Where(e => !e.Text.Equals("SuperAdmin"));
                        ViewBag.Permission = typePermission;
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }

                    IEnumerable<SelectListItem> phone = new SelectList(Enum.GetValues(typeof(TypePhone)));
                    ViewBag.TypePhone = phone;


                    return View();
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
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
                        int idUser = (int)Session["idUser"]; //who is login

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
                        Employee em = db.Employee.Where(e => e.NameEmployee.Equals(employee.NameEmployee)).FirstOrDefault();
                        #region Insert news Phones with AddRange
                        foreach (Phone p in phones)
                            p.EmployeeId = em.IdEmployee;

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
                            EmployeeId = em.IdEmployee
                        };
                        db.Log.Add(log);
                        db.SaveChanges();
                        #endregion

                        trans.Commit();
                        TempData["confirm"] = "New Employee Created";
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
            #region  ReturnIfError
            try
            {
                int permission = (int)Session["permission"];
                int idCompany = (int)Session["idCompany"];

                if (Check.IsSuperAdmin(permission))
                {
                    ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", employee.CompanyId);
                    ViewBag.OfficeId = new SelectList(db.Office.OrderBy(c => c.CompanyId), "IdOffice", "NameOffice", employee.OfficeId);
                    IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission)));
                    ViewBag.Permission = typePermission;
                }
                else if (Check.IsAdmin(permission))
                {
                    Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                    List<Company> companies = new List<Company>();
                    companies.Add(company);
                    ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", employee.CompanyId);
                    ViewBag.OfficeId = new SelectList(db.Office.Where(o => o.CompanyId == idCompany), "IdOffice", "NameOffice", employee.OfficeId);

                    IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission))).Where(e => !e.Text.Equals("SuperAdmin"));
                    ViewBag.Permission = typePermission;
                }

                IEnumerable<SelectListItem> phone = new SelectList(Enum.GetValues(typeof(TypePhone)));
                ViewBag.TypePhone = phone;

                return View(employee);
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
            #endregion
        }
        #endregion
        public ActionResult AddOtherDetailsPhone()
        {
            return PartialView("_PhoneDetails");
        }

        public ActionResult CreateUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Employee employee = db.Employee.Find(id);

                    if (employee == null)
                    {
                        return HttpNotFound();
                    }
                    else if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == employee.CompanyId) || Check.IsMe(idUser, employee.IdEmployee))
                    {
                        return View(employee);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser([Bind(Include = "IdEmployee,LoginUser,PasswordUser")] Employee employee)
        {
            Employee emp = db.Employee.Find(employee.IdEmployee);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    Log log = new Log
                    {
                        Who = idUser,
                        EmployeeId = employee.IdEmployee,
                        New = "[CL]"
                    };

                    emp.LoginUser = employee.LoginUser;
                    emp.PasswordUser = HashService.HashPassword(employee.PasswordUser);
                    db.SaveChanges();

                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "User Create successfully";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    ViewBag.error = "An error happened. Please try again";
                    return View(employee);
                }
            }
        }

        public ActionResult ChangePassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Employee employee = db.Employee.Find(id);

                    if (employee == null)
                    {
                        return HttpNotFound();
                    }
                    else if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == employee.CompanyId) || Check.IsMe(idUser, employee.IdEmployee))
                    {
                        employee.PasswordUser = string.Empty;
                        return View(employee);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "IdEmployee,PasswordUser")] Employee employee)
        {
            Employee emp = db.Employee.Find(employee.IdEmployee);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    Log log = new Log
                    {
                        Who = idUser,
                        EmployeeId = employee.IdEmployee,
                        New = "[CP]"
                    };

                    emp.PasswordUser = HashService.HashPassword(employee.PasswordUser);
                    db.SaveChanges();

                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Password updated successfully";
                    return RedirectToAction("Index");
                }
                catch
                {
                    trans.Rollback();
                    ViewBag.error = "An error happened. Please try again";
                    return View(employee);
                }
            }
        }

        #region EDIT ACTIONS
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Employee employee = db.Employee.Find(id);
                    if (employee == null)
                    {
                        return HttpNotFound();
                    }

                    if (Check.IsSuperAdmin(permission))
                    {
                        ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", employee.CompanyId);
                        ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", employee.OfficeId);

                        IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission)), employee.Permission);
                        ViewBag.Permission = typePermission;
                    }
                    else if (Check.IsAdmin(permission) && idCompany == employee.CompanyId)
                    {
                        Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                        List<Company> companies = new List<Company>();
                        companies.Add(company);
                        ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", employee.CompanyId);
                        ViewBag.OfficeId = new SelectList(db.Office.Where(o => o.CompanyId == idCompany).ToList(), "IdOffice", "NameOffice", employee.OfficeId);

                        IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission))).Where(e => !e.Text.Equals("SuperAdmin"));
                        ViewBag.Permission = typePermission;
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }

                    IEnumerable<SelectListItem> phone = new SelectList(Enum.GetValues(typeof(TypePhone)));
                    ViewBag.TypePhone = phone;

                    List<Phone> phones = db.Phone.Where(p => p.EmployeeId == id).ToList();
                    if (phones != null && phones.Count > 0)
                        ViewBag.Phones = phones;

                    return View(employee);
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
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
                else if (request[i].StartsWith("TypePhone=") || request[i].StartsWith("Number=") || request[i].StartsWith("DDD="))
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

                if (oldEmployee == null || oldAddress == null || oldPhones == null)
                {
                    TempData["error"] = "Sorry, but an error happened, Please contact your system supplier";
                    return RedirectToAction("Index");
                }

                int idUser = (int)Session["idUser"];

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

                                if (oldEmployee.HaveLogin == false)
                                {
                                    oldEmployee.LoginUser = null;
                                    oldEmployee.PasswordUser = null;
                                }

                                db.SaveChanges();

                                log.New += "E " + editEmployee.NameEmployee + "/ " + editEmployee.Birth.ToString("dd/MM/yy") + "/ " + editEmployee.Admission.ToString("dd/MM/yy") + "/ " + editEmployee.Salary
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
                            if (!oldPhones.SequenceEqual(editPhones, new Phone()))
                            {
                                //Using Any or All? 
                                //Any=> Returns true if at least one of the elements in the source sequence matches the provided predicate.
                                //All => Returns true if every element in the source sequence matches the provided predicate.

                                List<Phone> phonesChanged = editPhones.Where(p => oldPhones.Any(ph => ph.IdPhone == p.IdPhone && (!ph.DDD.Equals(p.DDD) || !ph.Number.Equals(p.Number) || !ph.TypePhone.Equals(p.TypePhone)))).ToList();
                                List<Phone> phonesNew = editPhones.Where(p => oldPhones.All(ph => ph.IdPhone != p.IdPhone)).ToList();

                                if (phonesChanged != null && phonesChanged.Count > 0)
                                {
                                    foreach (Phone phone in phonesChanged)
                                    {
                                        int index = oldPhones.IndexOf(oldPhones.Where(p => p.IdPhone == phone.IdPhone).FirstOrDefault());

                                        oldPhones[index].DDD = phone.DDD;
                                        oldPhones[index].Number = phone.Number;
                                        oldPhones[index].TypePhone = phone.TypePhone;
                                    }
                                }
                                if (phonesNew != null && phonesNew.Count > 0)
                                {
                                    foreach (Phone p in phonesNew)
                                        p.EmployeeId = oldEmployee.IdEmployee;

                                    db.Phone.AddRange(phonesNew);

                                }

                                db.SaveChanges();

                                if (!log.New.Equals(""))
                                {
                                    db.Log.Add(log);
                                    db.SaveChanges();
                                }
                            }
                            trans.Commit();
                            TempData["confirm"] = "Successful Changes";
                            return RedirectToAction("Index");
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
                    TempData["message"] = "No changes were recorded";
                    return RedirectToAction("Index");
                }
            }

        ReturnIfError:
            int permission = (int)Session["permission"];
            int idCompany = (int)Session["idCompany"];

            if (Check.IsSuperAdmin(permission))
            {
                IEnumerable<SelectListItem> typePhone = new SelectList(Enum.GetValues(typeof(TypePhone)));
                ViewBag.TypePhone = typePhone;
                ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", editEmployee.CompanyId);
                ViewBag.OfficeId = new SelectList(db.Office, "IdOffice", "NameOffice", editEmployee.OfficeId);

                IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission)));
                ViewBag.Permission = typePermission;
            }
            else if (Check.IsAdmin(permission) && idCompany == editEmployee.CompanyId)
            {
                Company company = db.Company.Where(c => c.IdCompany == idCompany).FirstOrDefault();
                List<Company> companies = new List<Company>();
                companies.Add(company);
                ViewBag.CompanyId = new SelectList(companies, "IdCompany", "NameCompany", editEmployee.CompanyId);
                ViewBag.OfficeId = new SelectList(db.Office.Where(o => o.CompanyId == idCompany).ToList(), "IdOffice", "NameOffice", editEmployee.OfficeId);

                IEnumerable<SelectListItem> typePermission = new SelectList(Enum.GetValues(typeof(Permission))).Where(e => !e.Text.Equals("SuperAdmin"));
                ViewBag.Permission = typePermission;
            }


            List<Phone> phones = db.Phone.Where(p => p.EmployeeId == editEmployee.IdEmployee).ToList();
            if (phones != null && phones.Count > 0)
                ViewBag.Phones = phones;


            return View(editEmployee);
        }
        #endregion

        #region DELETE AND ACTIVE ACTIONS
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Employee employee = db.Employee.Find(id);
                    if (employee == null)
                    {
                        return HttpNotFound();
                    }

                    if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == employee.CompanyId))
                    {
                        List<Phone> phones = db.Phone.Where(p => p.EmployeeId == id).ToList();
                        if (phones != null && phones.Count > 0)
                            ViewBag.Phones = phones;

                        return View(employee);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employee.Find(id);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    Log log = new Log
                    {
                        Who = idUser,
                        EmployeeId = id,
                        New = "DISABLED",
                        Old = "ACTIVATED"
                    };

                    employee.DeactivateEmployee();
                    db.SaveChanges();

                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Employee successfully deactivated";
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
            try
            {
                int idUser = Session["idUser"] != null ? (int)Session["idUser"] : 0;
                int permission = Session["permission"] != null ? (int)Session["permission"] : 0;
                int idCompany = Session["permission"] != null ? (int)Session["idCompany"] : 0;
                string userName = Session["permission"] != null ? (string)Session["username"] : null;

                if (Check.IsLogOn(idUser, permission, idCompany, userName))
                {
                    Employee employee = db.Employee.Find(id);
                    if (employee == null)
                    {
                        return HttpNotFound();
                    }

                    if (Check.IsSuperAdmin(permission) || (Check.IsAdmin(permission) && idCompany == employee.CompanyId))
                    {
                        List<Phone> phones = db.Phone.Where(p => p.EmployeeId == id).ToList();
                        if (phones != null && phones.Count > 0)
                            ViewBag.Phones = phones;

                        return View(employee);
                    }
                    else
                    {
                        TempData["error"] = "YOU DO NOT HAVE PERMISSION";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return RedirectToAction("LogIn", "Employees");
            }
            catch
            {
                TempData["error"] = "You need to login";
                return RedirectToAction("LogIn", "Employees");
            }
        }

        [HttpPost, ActionName("Active")]
        [ValidateAntiForgeryToken]
        public ActionResult ActiveConfirmed(int id)
        {
            Employee employee = db.Employee.Find(id);
            int idUser = (int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    Log log = new Log
                    {
                        Who = idUser,
                        EmployeeId = id,
                        New = "ACTIVATED",
                        Old = "DISABLED"
                    };

                    employee.ReactivateEmployee();
                    db.SaveChanges();

                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Employee successfully reactivated";
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
        #endregion
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
