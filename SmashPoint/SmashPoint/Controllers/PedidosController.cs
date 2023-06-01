using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmashPoint.Data;
using SmashPoint.Models;

namespace SmashPoint.Controllers
{
    [RequireProfileComplete]
    public class PedidosController : Controller
    {
        private readonly SmashPointContexto _context;

        public PedidosController(SmashPointContexto context)
        {
            _context = context;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var pedidos = _context.Pedidos.Include(p => p.Cliente)
                .Include(p => p.Cliente)
                .Include(p => p.Descuento)
                .Include(p=>p.EstadoActual)
                .Include(p=> p.Detalles).ThenInclude(d=> d.Producto).ThenInclude(p=> p.Categoria)
                .Include(p=> p.Detalles).ThenInclude(d=> d.EstadoDetalles).ThenInclude(e=> e.Estado).AsQueryable();

            if (User.IsInRole("Usuario"))
            {
                var cliente = _context.Clientes.FirstOrDefault(c => c.Email == User.Identity.Name);
                pedidos = pedidos.Where(p => p.ClienteId == cliente.Id);
                
            }
            if (User.IsInRole("Gestor"))
            {
                var proveedor = _context.Proveedores.FirstOrDefault(c => c.Correo == User.Identity.Name);

                pedidos = pedidos.Where(p=> p.Detalles.Any(d=> d.Producto.ProveedorId == proveedor.Id));
                
                pedidos = pedidos.Include(p => p.Detalles.Where(d => d.Producto.ProveedorId == proveedor.Id));
            }

            int pageSize = 5;
            return View(await PaginatedList<Pedido>.CreateAsync(pedidos.OrderByDescending(p => p.Id).AsNoTracking(),pageNumber ?? 1, pageSize));

            //return View(pedidos.OrderByDescending(p => p.Id).ToList());
        }

        public async Task<IActionResult> SubirEstadoDetalle(int id)
        {
            var detalle = _context.Detalles.Include(d=> d.EstadoDetalles).FirstOrDefault(d=> d.Id ==id);
            var pedido = _context.Pedidos.Include(p=> p.Detalles).ThenInclude(d => d.EstadoDetalles).FirstOrDefault(d => d.Id == detalle.PedidoId);

            var estadoDetalleActual = detalle.EstadoDetalles.OrderByDescending(e => e.EstadoId).FirstOrDefault();

            var estadoDetalle = new EstadoDetalle();
            estadoDetalle.Detalle = detalle;
            estadoDetalle.DetalleId = detalle.Id;
            estadoDetalle.Estado = pedido.EstadoActual;
            if (estadoDetalleActual.EstadoId == 2)
            {
                estadoDetalle.EstadoId = 3;
            }
            else
            {
                estadoDetalle.EstadoId = 5;
            }
            _context.Add(estadoDetalle);
            _context.SaveChanges();

            var estadoMenorTotalId = 0;
            bool esPrimeraVez = true;
            foreach(var detalleA in pedido.Detalles)
            {
                var EstadoMaximo = detalleA.EstadoDetalles.OrderByDescending(d => d.EstadoId).FirstOrDefault();
                if (esPrimeraVez)
                {
                    estadoMenorTotalId = EstadoMaximo.EstadoId;
                    esPrimeraVez = false;
                }
                if (EstadoMaximo.EstadoId<estadoMenorTotalId)
                {
                    estadoMenorTotalId=EstadoMaximo.EstadoId;
                }
            }

            var estadoComprobado = _context.Estados.Find(estadoMenorTotalId);
            pedido.EstadoActual = estadoComprobado;

            pedido.EstadoActualId = estadoComprobado.Id;

            _context.Update(pedido);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> AnularPedido(int id)
        {
            var pedido = _context.Pedidos.Include(p=> p.Detalles).ThenInclude(d=> d.EstadoDetalles).Include(p => p.Detalles).ThenInclude(d=> d.Producto).FirstOrDefault(p=> p.Id==id);
            Proveedor proveedor = _context.Proveedores.FirstOrDefault(p=> p.Correo == User.Identity.Name);
            foreach (var detalle in pedido.Detalles)
            {
                if (User.IsInRole("Gestor"))
                {
                    if (detalle.Producto.ProveedorId == proveedor.Id)
                    {
                        var estadoDetalle = new EstadoDetalle();
                        estadoDetalle.Detalle = detalle;
                        estadoDetalle.DetalleId = detalle.Id;
                        estadoDetalle.Estado = pedido.EstadoActual;
                        estadoDetalle.EstadoId = 4;
                        _context.Add(estadoDetalle);
                        _context.SaveChanges();
                    }
                }
                else
                {
                    var estadoDetalle = new EstadoDetalle();
                    estadoDetalle.Detalle = detalle;
                    estadoDetalle.DetalleId = detalle.Id;
                    estadoDetalle.Estado = pedido.EstadoActual;
                    estadoDetalle.EstadoId = 4;
                    _context.Add(estadoDetalle);
                    _context.SaveChanges();
                }

            }
            if (User.IsInRole("Usuario") || User.IsInRole("Administrador"))
            {
                pedido.EstadoActualId = 4;
                pedido.EstadoActual = _context.Estados.Find(4);
                _context.Update(pedido);
                _context.SaveChanges();
            }
            else
            {

                int contador = 0;
                foreach (var detalleA in pedido.Detalles)
                {
                    var EstadoMaximo = detalleA.EstadoDetalles.OrderByDescending(d => d.EstadoId).FirstOrDefault();
                    if (EstadoMaximo.EstadoId==4)
                    {
                        contador++;
                    }
                }
                if(contador == pedido.Detalles.Count())
                {
                    pedido.EstadoActualId = 4;
                    pedido.EstadoActual = _context.Estados.Find(4);
                    _context.Update(pedido);
                    _context.SaveChanges();
                }
            }

            
            _context.SaveChanges();

            if (HttpContext.Session.GetString("NumPedido")!=null)
            {
                if (pedido.Id == int.Parse(HttpContext.Session.GetString("NumPedido")))
                {
                    HttpContext.Session.Remove("NumPedido");
                    HttpContext.Session.Remove("Carrito");
                }
            }


            return RedirectToAction(nameof(Index));
        }

            // GET: Pedidos/Details/5
            public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Descuento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }



        // GET: Pedidos/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre");
            ViewData["DescuentoId"] = new SelectList(_context.Descuentos, "Id", "Id");
            return View();
        }

        // POST: Pedidos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClienteId,Fecha,PrecioTotal,DescuentoId")] Pedido pedido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pedido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", pedido.ClienteId);
            ViewData["DescuentoId"] = new SelectList(_context.Descuentos, "Id", "Id", pedido.DescuentoId);
            return View(pedido);
        }

        // GET: Pedidos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", pedido.ClienteId);
            ViewData["DescuentoId"] = new SelectList(_context.Descuentos, "Id", "Id", pedido.DescuentoId);
            return View(pedido);
        }

        // POST: Pedidos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Fecha,PrecioTotal,DescuentoId")] Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pedido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(pedido.Id))
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
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nombre", pedido.ClienteId);
            ViewData["DescuentoId"] = new SelectList(_context.Descuentos, "Id", "Id", pedido.DescuentoId);
            return View(pedido);
        }

        // GET: Pedidos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pedidos == null)
            {
                return NotFound();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Descuento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        // POST: Pedidos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pedidos == null)
            {
                return Problem("Entity set 'SmashPointContexto.Pedidos'  is null.");
            }
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PedidoExists(int id)
        {
          return _context.Pedidos.Any(e => e.Id == id);
        }
    }
}
