using System.Web.Mvc;

namespace IceCreamSystem.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Error500()
        {
            return View();
        }

        public ActionResult Error404()
        {
            return View();
        }
    }
}