using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
using NuGet.Protocol.Plugins;
using SmashPoint.Data;
using SmashPoint.Models;
using System.Data;
using SmashPoint.Controllers;

namespace SmashPoint.Controllers
{
    [RequireProfileComplete]
    [Authorize(Roles = "Usuario")]
    public class CarritoController : Controller
    {
        private readonly SmashPointContexto _context;
        private readonly Escaparate _escaparate;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CarritoController(SmashPointContexto context, IWebHostEnvironment webHostEnvironment, Escaparate escaparate)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _escaparate = escaparate;
        }

        public async Task<IActionResult> Index()
        {
            if(HttpContext.Session.GetString("NumPedido") != null)
            {
                Pedido pedido = await _context.Pedidos.Include(p=>p.Descuento).FirstOrDefaultAsync(p => p.Id == int.Parse(HttpContext.Session.GetString("NumPedido")));
                
                var detalles = _context.Detalles.Where(d => d.PedidoId == pedido.Id);
                ViewData["Detalles"] = detalles.Include(d => d.Producto)
                            .Include(d=> d.Producto.Categoria);

                pedido.PrecioTotal=0;
                foreach (var detalle in detalles)
                {
                    pedido.PrecioTotal += detalle.Precio;
                }
                if (pedido.Descuento != null)
                {
                    ViewData["Descuento"] = pedido.Descuento;
                    pedido.PrecioTotal = pedido.PrecioTotal * pedido.Descuento.Valor;
                }
                _context.Update(pedido);
                _context.SaveChanges();

                //_escaparate.ActualizarContadorCarrito();

                return View(pedido);
            }
            else
            {
                return RedirectToAction("CarritoVacio", "Carrito");
            }
            
        }

        public async Task<IActionResult> CarritoVacio()
        {
            return View();
        }

            public async Task<IActionResult> RevisarDescuento()
        {
            var pedido = await _context.Pedidos.Include(p=> p.Descuento).FirstOrDefaultAsync(p => p.Id == int.Parse(HttpContext.Session.GetString("NumPedido")));

            if (pedido.DescuentoId == null)
            {
                var descuento = _context.Descuentos.FirstOrDefault(d => d.Codigo == Request.Form["Codigo"].ToString());

                if (descuento != null)
                {
                    pedido.Descuento = descuento;
                    pedido.DescuentoId = descuento.Id;
                }
                _context.Update(pedido);
                _context.SaveChanges();
            }

            return Redirect(nameof(Index));
        }

        public async Task<IActionResult> ConfirmarPedido(int id)
        {
            var pedido = _context.Pedidos.Find(id);

            var detalles = _context.Detalles.Where(d => d.PedidoId == pedido.Id);
            var estadoConfirmado = _context.Estados.FirstOrDefault(e => e.Descripcion=="Confirmado");
            pedido.EstadoActual= estadoConfirmado;
            pedido.EstadoActualId= estadoConfirmado.Id;
            _context.Update(pedido);

            foreach(var detalle in detalles)
            {

                var producto = _context.Productos.Find(detalle.ProductoId);
                producto.Stock -= detalle.Cantidad;
                _context.Update(producto);

                var estadoDetalle = new EstadoDetalle();
                estadoDetalle.Detalle = detalle;
                estadoDetalle.DetalleId= detalle.Id;
                estadoDetalle.EstadoId = estadoConfirmado.Id;
                estadoDetalle.Estado = estadoConfirmado;
                estadoDetalle.Fecha = DateTime.Now;

                _context.Update(estadoDetalle);
            }

            _context.SaveChanges();

            HttpContext.Session.Remove("NumPedido");
            HttpContext.Session.Remove("Carrito");

            return Redirect("/");
        }

        public async Task<IActionResult> EliminarLinea(int id)
        {
            var detalle = _context.Detalles.Find(id);
            var estadoDetalle = _context.EstadoDetalles.FirstOrDefault(e => e.DetalleId== id);

            _context.Remove(estadoDetalle);
            _context.Remove(detalle);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
