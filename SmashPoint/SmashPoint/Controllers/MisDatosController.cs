using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmashPoint.Data;
using SmashPoint.Models;
using System.Data;

namespace SmashPoint.Controllers
{
    [RequireProfileComplete]
    [Authorize(Roles = "Usuario")]
    public class MisDatosController : Controller
    {

        private readonly SmashPointContexto _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MisDatosController(SmashPointContexto context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult Index()
        {
            Cliente cliente = _context.Clientes.FirstOrDefault(p => p.Email == User.Identity.Name);

            return View(cliente);
        }

        // POST: MisDatos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,Nombre,Email,Telefono,Direccion,Poblacion, CodigoPostal, Nif")] Cliente cliente)
        {
            Cliente clienteActual = _context.Clientes.FirstOrDefault(p => p.Email == User.Identity.Name);
            clienteActual.Nombre = cliente.Nombre;
            clienteActual.Telefono = cliente.Telefono;
            clienteActual.Direccion = cliente.Direccion;
            clienteActual.Poblacion = cliente.Poblacion;
            clienteActual.CodigoPostal = cliente.CodigoPostal;
            clienteActual.Nif = cliente.Nif;


            _context.Update(clienteActual);
            _context.SaveChanges();
            return RedirectToAction("Index", "MisDatos");

        }
    }
}
