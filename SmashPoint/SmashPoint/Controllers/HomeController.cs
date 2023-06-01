using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmashPoint.Data;
using SmashPoint.Models;

namespace SmashPoint.Controllers
{
    public class HomeController : Controller
    {
        private readonly SmashPointContexto _context;
        public HomeController(SmashPointContexto context)
        {
            _context = context;
        }
        // GET: HomeController
        public ActionResult Index()
        {
            if (User.IsInRole("Usuario"))
            {

                var cliente = _context.Clientes.FirstOrDefault(cliente => cliente.Email == User.Identity.Name);

                if (cliente != null)
                {
                    HttpContext.Session.SetString("Username", cliente.Nombre);
                }
            }
            if (User.IsInRole("Gestor"))
            {
                Proveedor proveedor = _context.Proveedores.Where(e => e.Correo == User.Identity.Name).FirstOrDefault();

                if (proveedor != null)
                {
                    HttpContext.Session.SetString("Username", proveedor.Nombre);
                }
            }
            if (User.IsInRole("Administrador"))
            {
                HttpContext.Session.SetString("Username", User.Identity.Name);
            
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Escaparate");
            }
        }
   
    }
}
