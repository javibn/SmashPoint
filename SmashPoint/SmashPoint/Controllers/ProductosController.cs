using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmashPoint.Data;
using SmashPoint.Models;

namespace SmashPoint.Controllers
{
    [RequireProfileComplete]
    [Authorize(Roles = "Administrador,Gestor")]
    public class ProductosController : Controller
    {
        private readonly SmashPointContexto _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductosController(SmashPointContexto context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Productos
        public async Task<IActionResult> Index(int? pageNumber)
        {
            IQueryable<Producto> smashPointContexto;
            if (User.IsInRole("Gestor")){
                var proveedorActual = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                smashPointContexto = _context.Productos.Include(p => p.Categoria).Include(p => p.Proveedor).Where(p => p.ProveedorId == proveedorActual.Id);
                ViewData["Title"] = "Mis Productos";
            }
            else
            {
                smashPointContexto = _context.Productos.Include(p => p.Categoria).Include(p => p.Proveedor);
                ViewData["Title"] = "Productos";
            }

            int pageSize = 4;
            return View(await PaginatedList<Producto>.CreateAsync(smashPointContexto, pageNumber ?? 1, pageSize));
            
            //return View(await smashPointContexto.ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Productos == null)
            {
                return NotFound();
            }
            var producto = await _context.Productos
                                .Include(p => p.Categoria)
                                .Include(p => p.Proveedor)
                                .FirstOrDefaultAsync(m => m.Id == id);
            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);

                

                if (producto == null)
                {
                    return NotFound();
                }

                if (proveedor == null || proveedor.Id != producto.ProveedorId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Descripcion");
            ViewData["Proveedores"] = new SelectList(_context.Proveedores, "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Descripcion,Texto,PrecioCadena,Stock,Escaparate,Imagen,CategoriaId,ProveedorId")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Gestor"))
                {
                    producto.Proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                    producto.ProveedorId = producto.Proveedor.Id;
                }
                var descripcioBuscar = producto.Descripcion; 
                _context.Add(producto);
                _context.SaveChanges();

                producto = _context.Productos.FirstOrDefault(p => p.Descripcion == descripcioBuscar);
                string strNombreFichero;
                if (Request.Form.Files.Count > 0) {
                    IFormFile file = Request.Form.Files[0];
                    string strRutaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                    string strExtension = file.ContentType;
                    int posicionBarra = strExtension.IndexOf('/');
                    strNombreFichero = producto.Id.ToString() + ".png";
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

                

                // Actualizar producto con nueva imagen
                producto.Imagen = strNombreFichero;
                _context.Update(producto);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Descripcion", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores, "Id", "Id", producto.ProveedorId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Productos == null)
            {
                return NotFound();
            }

            

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                if (proveedor==null || proveedor.Id != producto.ProveedorId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Descripcion", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores, "Id", "Nombre", producto.ProveedorId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion,Texto,PrecioCadena ,Stock,Escaparate,Imagen,CategoriaId,ProveedorId")] Producto producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                if (proveedor==null || proveedor.Id != producto.ProveedorId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Descripcion", producto.CategoriaId);
            ViewData["ProveedorId"] = new SelectList(_context.Proveedores, "Id", "Id", producto.ProveedorId);
            return View(producto);
        }

        // GET: Productos/CambiarImagen/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            var producto = _context.Productos.Find(id);

            try
            {
                _context.Remove(producto);
                _context.SaveChanges();
            }
            catch
            {
                return RedirectToAction(nameof(Error));
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Error()
        {
            return View();
        }

        // GET: Productos/CambiarImagen/5
        public async Task<IActionResult> CambiarImagen(int? id)
        {
            if (id == null || _context.Productos == null)
            {
                return NotFound();
            }
            var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }


            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                if (proveedor==null || proveedor.Id != producto.ProveedorId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(producto);
        }

        // POST: Productos/CambiarImagen/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarImagen(int? id, IFormFile imagen)
        {
            if (id == null)
            {
                return NotFound();
            }
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                if (proveedor==null || proveedor.Id != producto.ProveedorId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            if (imagen == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                // Copiar archivo de imagen
                string strRutaImagenes = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                string strExtension = Path.GetExtension(imagen.FileName);
                string strNombreFichero = producto.Id.ToString() + ".png";
                string strRutaFichero = Path.Combine(strRutaImagenes, strNombreFichero);
                if (System.IO.File.Exists(strRutaFichero))
                {
                    System.IO.File.Delete(strRutaFichero);
                }
                using (var fileStream = new FileStream(strRutaFichero, FileMode.Create))
                {
                    imagen.CopyTo(fileStream);
                }
                // Actualizar producto con nueva imagen
                producto.Imagen = strNombreFichero;
                try
                {
                    _context.Update(producto);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction("Index", "Productos" );
        }


        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Productos == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                if (proveedor==null || proveedor.Id != producto.ProveedorId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Productos == null)
            {
                return Problem("Entity set 'SmashPointContexto.Productos'  is null.");
            }
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(p => p.Correo == User.Identity.Name);
                if (proveedor==null || proveedor.Id != producto.ProveedorId)
                {
                    return RedirectToAction(nameof(Index));
                }
            }


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
