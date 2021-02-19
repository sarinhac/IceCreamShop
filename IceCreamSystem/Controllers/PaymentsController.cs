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
    public class PaymentsController : Controller
    {
        private Context db = new Context();

        public ActionResult Index()
        {
            ViewBag.confirm = TempData["confirm"] != null ? TempData["confirm"].ToString() : null;
            ViewBag.error = TempData["error"] != null ? TempData["error"].ToString() : null;

            var payment = db.Payment.Include(p => p.Company).Include(p => p.CreditCard).Include(p => p.DebitCard).Include(p => p.Sale);
            return View(payment.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Payment payment = db.Payment.Find(id);
            
            if (payment == null)
            {
                return HttpNotFound();
            }

            #region VIEWBAGS
            ViewBag.Rate = payment.TypePayment == (TypePayment)1 ? null : payment.TypePayment == (TypePayment)2 ? payment.CreditCard.RateCreditCard.ToString() : payment.DebitCard.Rate.ToString();

            if(payment.TypePayment == (TypePayment)2) //credit
            {
                decimal installmentGross = Math.Round((payment.TotalPrice / payment.InstallmentNumber), 2);
                ViewBag.InstallmentGross = installmentGross.ToString();
            }
            else
                ViewBag.InstallmentGross = payment.TotalPrice.ToString();

            if (payment.TypePayment == (TypePayment)2) //credit
            {
                decimal TotalNetValue = Math.Round(payment.TotalPrice - ((payment.TotalPrice * payment.CreditCard.RateCreditCard) / 100), 2);
                ViewBag.TotalNetValue  = TotalNetValue.ToString();
            }
            else if (payment.TypePayment == (TypePayment)3) //debit
            {
                decimal TotalNetValue = Math.Round(payment.TotalPrice - ((payment.TotalPrice * payment.DebitCard.Rate) / 100), 2);
                ViewBag.TotalNetValue = TotalNetValue.ToString();
            }
            else
                ViewBag.TotalNetValue = payment.TotalPrice.ToString();
            #endregion

            return View(payment);
        }

        public ActionResult Create(int id)
        {
            ViewBag.SaleId = id;
            IEnumerable<SelectListItem> typePayment = new SelectList(Enum.GetValues(typeof(TypePayment)));
            ViewBag.TypePayment = typePayment;

            string text = "--Select--";
            ViewBag.Text = text;
            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", "");
            ViewBag.CreditCardId = new SelectList(db.CreditCard, "IdCreditCard", "NameCreditCard");
            ViewBag.DebitCardId = new SelectList(db.DebitCard, "IdDebitCard", "NameDebitCard");

            return PartialView("_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SaleId,TypePayment,DebitCardId,CreditCardId,CompanyId,forecastDatePayment,InstallmentNumber,CodePaymentCard")] Payment payment)
        {
            string[] productsInCookie = Request.Cookies["products"].Value.Split('/');
            int companyId = 1;// (int)Session["companyId"];
            List<SaleProduct> saleProducts = SalesProductsService.ReturnSaleProducts(productsInCookie, companyId, payment.SaleId);
            decimal TotalPrice = SalesProductsService.GetTotalPrice(saleProducts);

            if (payment.SaleId > 0)
            {
                int idUser = 1;// (int)Session["idUser"]; //who is login
                Sale sale = db.Sale.Find(payment.SaleId);
                if (sale != null)
                {
                    using (var trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (payment.TypePayment == (TypePayment) 2 || payment.TypePayment == (TypePayment) 3)
                            {
                                #region SALE ON CARD
                                if (payment.CreditCardId > 0)
                                {
                                    #region CREDIT SALE
                                    CreditCard card = db.CreditCard.Find(payment.CreditCardId);
                                    List<Payment> installmentSale = new List<Payment>();
                                    List<Log> logs = new List<Log>();

                                    if (payment.InstallmentNumber == 0)
                                        payment.InstallmentNumber = 1;
                                    
                                    decimal tax = ((TotalPrice / payment.InstallmentNumber) * card.RateCreditCard) / 100;
                                    decimal installment = Math.Round((TotalPrice / payment.InstallmentNumber) - tax, 2);
                                    
                                    #region CREATE CREATE PAYMENT FOR EACH INSTALLMENT
                                    for (int i = 1; i <= payment.InstallmentNumber; i++)
                                    {
                                        int days = 30 * i;
                                        int status = card.Company.FlAuthoritativeReceipt ? 1 : 2; //Pay or Payable

                                        Payment pay = new Payment
                                        {
                                            SaleId = payment.SaleId,
                                            Created = payment.Created,
                                            EmployeeId = idUser,
                                            CreditCardId = payment.CreditCardId,
                                            CompanyId = payment.CompanyId,
                                            TypePayment = payment.TypePayment,
                                            Status = (StatusPayment)status, 
                                            InstallmentNumber = i,
                                            CodePaymentCard = payment.CodePaymentCard,
                                            TotalPrice = TotalPrice,
                                            InstallmentPrice = installment,
                                            forecastDatePayment = DateTime.Now.AddDays(days)
                                        };

                                        installmentSale.Add(pay);
                                    }

                                    db.Payment.AddRange(installmentSale);
                                    db.SaveChanges();
                                    #endregion

                                    #region CREATE LOGS
                                    for (int i = 0; i < installmentSale.Count; i++)
                                    {
                                        Log log = new Log
                                        {
                                            //[C] in DB refers to an Create
                                            New = "[C]" + installmentSale[i].TypePayment + " " + installmentSale[i].CreditCardId + " " + installmentSale[i].InstallmentNumber + " " + installmentSale[i].forecastDatePayment + " " + installmentSale[i].InstallmentPrice + " " + installmentSale[i].TotalPrice + " " + installmentSale[i].Status,
                                            Who = idUser,
                                            PaymentId = installmentSale[i].IdPayment,
                                            SaleId = installmentSale[i].SaleId,
                                            CompanyId = installmentSale[i].CompanyId
                                        };

                                        logs.Add(log);
                                    }

                                    db.Log.AddRange(logs);
                                    db.SaveChanges();
                                    #endregion
                                    #endregion //CREDIT SALE
                                    
                                }
                                else if(payment.DebitCardId > 0)
                                {
                                    #region DEBIT SALE
                                    DebitCard card = db.DebitCard.Find(payment.DebitCardId);

                                    payment.EmployeeId = idUser;
                                    payment.Status = card.Company.FlAuthoritativeReceipt ? (StatusPayment) 1 : (StatusPayment) 2; //Pay or Payable
                                    payment.InstallmentNumber = 0;
                                    payment.forecastDatePayment = payment.Created.Date.AddDays(1);
                                    payment.TotalPrice = TotalPrice;
                                    
                                    decimal tax = (TotalPrice * card.Rate) / 100;
                                    decimal installment = Math.Round(TotalPrice - tax, 2);
                                    payment.InstallmentPrice = installment;

                                    db.Payment.Add(payment);
                                    db.SaveChanges();

                                    Log log = new Log
                                    {
                                        //[C] in DB refers to an Create
                                        New = "[C]" + payment.TypePayment + " " + payment.CreditCardId + " " + payment.InstallmentNumber + " " + payment.Status,
                                        Who = idUser,
                                        PaymentId = payment.IdPayment,
                                        SaleId = payment.SaleId,
                                        CompanyId = payment.CompanyId,
                                    };

                                    db.Log.Add(log);
                                    db.SaveChanges();
                                    #endregion
                                }
                                else
                                {
                                    trans.Rollback();
                                    ViewBag.error = "Sorry, but an error happened, try again, if the error continues please contact your system supplier";
                                    goto ReturnIfError;
                                }
                                #endregion //SALE ON CARD
                            }
                            else
                            {
                                #region CASH SALE
                                payment.EmployeeId = idUser;
                                payment.Status = (StatusPayment)1; //Pay
                                payment.InstallmentNumber = 0;
                                payment.forecastDatePayment = payment.Created.Date;
                                payment.TotalPrice = TotalPrice;
                                payment.InstallmentPrice = TotalPrice;

                                db.Payment.Add(payment);
                                db.SaveChanges();

                                Log log = new Log
                                {
                                    //[C] in DB refers to an Create
                                    New = "[C]" + payment.TypePayment + " " + payment.CreditCardId + " " + payment.InstallmentNumber + " " + payment.Status,
                                    Who = idUser,
                                    PaymentId = payment.IdPayment,
                                    SaleId = payment.SaleId,
                                    CompanyId = payment.CompanyId,
                                };

                                db.Log.Add(log);
                                db.SaveChanges();
                                #endregion
                            }

                            sale.TotalPrice = TotalPrice;
                            sale.Status = (SaleStatus)2; //FINISHED
                            db.SaveChanges();

                            trans.Commit();
                            TempData["confirm"] = "Sale Saved With FINISHED Status";
                            return RedirectToAction("Index", "Sales");

                        }
                        catch
                        {
                            trans.Rollback();
                            ViewBag.error = "Sorry, but an error happened, try again, if the error continues please contact your system supplier";
                            goto ReturnIfError;
                        }

                    }
                }
            }

        ReturnIfError:

            ViewBag.CompanyId = new SelectList(db.Company, "IdCompany", "NameCompany", payment.CompanyId);
            ViewBag.CreditCardId = new SelectList(db.CreditCard, "IdCreditCard", "NameCreditCard", payment.CreditCardId);
            ViewBag.DebitCardId = new SelectList(db.DebitCard, "IdDebitCard", "NameDebitCard", payment.DebitCardId);
            ViewBag.SaleId = new SelectList(db.Sale, "IdSale", "IdSale", payment.SaleId);
            return View(payment);
        }

        public ActionResult Pay(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payment.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }

            #region VIEWBAGS
            ViewBag.Rate = payment.TypePayment == (TypePayment)1 ? null : payment.TypePayment == (TypePayment)2 ? payment.CreditCard.RateCreditCard.ToString() : payment.DebitCard.Rate.ToString();

            if (payment.TypePayment == (TypePayment)2) //credit
            {
                decimal installmentGross = Math.Round((payment.TotalPrice / payment.InstallmentNumber), 2);
                ViewBag.InstallmentGross = installmentGross.ToString();
            }
            else
                ViewBag.InstallmentGross = payment.TotalPrice.ToString();

            if (payment.TypePayment == (TypePayment)2) //credit
            {
                decimal TotalNetValue = Math.Round(payment.TotalPrice - ((payment.TotalPrice * payment.CreditCard.RateCreditCard) / 100), 2);
                ViewBag.TotalNetValue = TotalNetValue.ToString();
            }
            else if (payment.TypePayment == (TypePayment)3) //debit
            {
                decimal TotalNetValue = Math.Round(payment.TotalPrice - ((payment.TotalPrice * payment.DebitCard.Rate) / 100), 2);
                ViewBag.TotalNetValue = TotalNetValue.ToString();
            }
            else
                ViewBag.TotalNetValue = payment.TotalPrice.ToString();
            #endregion

            return View(payment);
        }

        [HttpPost, ActionName("Pay")]
        [ValidateAntiForgeryToken]
        public ActionResult PayConfirmed(int id)
        {
            Payment payment = db.Payment.Find(id);
            int idUser = 1;//(int)Session["idUser"];

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    payment.MarkPaid();
                    db.SaveChanges();

                    Log log = new Log
                    {
                        Who = idUser,
                        PaymentId = id,
                        CompanyId = payment.CompanyId,
                        SaleId = payment.SaleId,
                        New = "PAY",
                        Old = payment.Status.ToString().ToUpper()
                    };

                    db.Log.Add(log);
                    db.SaveChanges();

                    trans.Commit();
                    TempData["confirm"] = "Payment Done";
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
