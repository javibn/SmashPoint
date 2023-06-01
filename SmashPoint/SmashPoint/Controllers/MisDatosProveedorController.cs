using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SmashPoint.Data;
using SmashPoint.Models;
using System.Data;

namespace SmashPoint.Controllers
{
    
    [Authorize(Roles = "Gestor")]
    public class MisDatosProveedorController : Controller
    {
        private readonly SmashPointContexto _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MisDatosProveedorController(SmashPointContexto context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [RequireProfileComplete]
        public IActionResult Index()
        {
            Proveedor proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);

            return View(proveedor);
        }
        // POST: MisDatos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,Nombre,Correo,Telefono,Direccion")] Proveedor proveedor)
        {
            Proveedor proveedorActual = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
            proveedorActual.Nombre = proveedor.Nombre;
            proveedorActual.Telefono = proveedor.Telefono;
            proveedorActual.Direccion = proveedor.Direccion;
            
            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files[0];
                string strRutaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                string strExtension = file.ContentType;
                int posicionBarra = strExtension.IndexOf('/');
                strExtension = strExtension.Substring(posicionBarra + 1);
                string strRutaFichero = Path.Combine(strRutaImagenes, "P"+proveedorActual.Id.ToString() + "." + "png");
                if (System.IO.File.Exists(strRutaFichero))
                {
                    System.IO.File.Delete(strRutaFichero);
                }
                using (var fileStream = new FileStream(strRutaFichero, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            _context.Update(proveedorActual);
            _context.SaveChanges();
            return RedirectToAction("Index", "MisDatosProveedor");
            
        }



        // GET: MisDatos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MisDatos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Imagen,Direccion, Telefono")] Proveedor proveedor)
        {
            // Asignar el Email del usuario actual
            proveedor.Correo = User.Identity.Name;
            _context.Add(proveedor);
            await _context.SaveChangesAsync();

            proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);


            string strNombreFichero;
            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files[0];
                string strRutaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                string strExtension = file.ContentType;
                int posicionBarra = strExtension.IndexOf('/');
                strExtension = strExtension.Substring(posicionBarra + 1);
                strNombreFichero = "P"+proveedor.Id.ToString() + "." + strExtension;
                string strRutaFichero = Path.Combine(strRutaImagenes, strNombreFichero);
                using (var fileStream = new FileStream(strRutaFichero, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            else
            {
                strNombreFichero = "imagen-no-disponible.jpg";
            }
            proveedor.Imagen = strNombreFichero;

            _context.Update(proveedor);
            _context.SaveChanges();

            return Redirect("/Escaparate");
        }
    }


    

}
