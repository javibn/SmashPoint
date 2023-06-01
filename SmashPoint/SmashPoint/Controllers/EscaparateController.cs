using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using SmashPoint.Data;
using SmashPoint.Models;
using System.Security.Principal;

namespace SmashPoint.Controllers
{
    [RequireProfileComplete]
    public class Escaparate : Controller
    {
        private readonly SmashPointContexto _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public Escaparate(SmashPointContexto context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        

        // GET: Escaparate
        public async Task<ActionResult> Index(int? id, int? pageNumber)
        {
            var productos = _context.Productos.AsQueryable();
            

            ViewData["Categorias"] =  _context.Categorias.ToList();
            


            if (HttpContext.Session.GetString("NumPedido")!=null)
            {
                ViewData["Carrito"]= _context.Detalles.Where(detalle => detalle.PedidoId==int.Parse(HttpContext.Session.GetString("NumPedido")));
            }
            else
            {
                ViewData["Carrito"]="";
            }
            

            if (id == null)
            {
                productos = productos.Include(p => p.Proveedor).Where(producto => producto.Escaparate == true);
                ViewData["TituloEscaparate"]="Escaparate";
            }
            else
            {
                productos = productos.Include(p=> p.Proveedor).Where(producto => producto.CategoriaId == id);
                ViewData["TituloEscaparate"] = _context.Categorias.Find(id).Descripcion;
            }

            int pageSize = 12;
            return View(await PaginatedList<Producto>.CreateAsync(productos, pageNumber ?? 1, pageSize));

            //return View(await productos.ToListAsync());
        }
        public async Task<ActionResult> Privacy()
        {
            
            return View();
        }



        public async Task<IActionResult> ActualizarCantidad(int id)
        {
            var cantidad = int.Parse(Request.Form["cantidad"]);

            Detalle detalle = await _context.Detalles.Include(d => d.Producto).FirstOrDefaultAsync(detalle => detalle.ProductoId == id && detalle.PedidoId==int.Parse(HttpContext.Session.GetString("NumPedido")));

            detalle.Cantidad+=cantidad;
            detalle.Precio = detalle.Producto.Precio * detalle.Cantidad;

            if (detalle.Cantidad<=0)
            {
                var estadoDetalle = _context.EstadoDetalles.FirstOrDefault(e => e.Detalle== detalle);

                _context.Remove(estadoDetalle);
                _context.Remove(detalle);
            }

            var pedido = _context.Pedidos.Find(int.Parse(HttpContext.Session.GetString("NumPedido")));
            var detalles = _context.Detalles.Where(d => d.PedidoId == pedido.Id);
            

            pedido.PrecioTotal=0;
            foreach (var detalle1 in detalles)
            {
                pedido.PrecioTotal += detalle1.Precio;
            }
            _context.Update(pedido);

            _context.SaveChanges();

            var referer = HttpContext.Request.Headers["Referer"].ToString();

            ActualizarContadorCarrito();

            return Redirect(referer);
        }

        public async Task<IActionResult> AgregarCarrito(int? id)
        {
            var producto = await _context.Productos.Include(p=> p.Categoria).Include(p=> p.Proveedor).FirstOrDefaultAsync(p => p.Id == id);
            return View(producto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarCarrito(int id)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null)
            {
                return NotFound();
            }
            var cliente = _context.Clientes.FirstOrDefault(cliente => cliente.Email == User.Identity.Name);
            

            if (User.IsInRole("Usuario"))
            {
                if (HttpContext.Session.GetString("NumPedido") == null)
                {
                    var estadoPendiente = await _context.Estados.FirstOrDefaultAsync(estado => estado.Descripcion == "Pendiente");
                    // Crear objeto pedido a agregar
                    Pedido pedido = new Pedido();

                    pedido.EstadoActual= estadoPendiente;
                    pedido.EstadoActualId = estadoPendiente.Id;
                    pedido.ClienteId=cliente.Id;
                    pedido.Fecha = DateTime.Now;
                    pedido.PrecioTotal+=producto.Precio;
                    
                    if (ModelState.IsValid)
                    {
                        _context.Add(pedido);
                        await _context.SaveChangesAsync();
                    }

                    
                    Detalle detalle = new Detalle();
                    detalle.PedidoId = pedido.Id;
                    detalle.Cantidad=1;
                    detalle.ProductoId = producto.Id;
                    detalle.Producto = producto;
                    detalle.Precio=producto.Precio*detalle.Cantidad;



                    if (ModelState.IsValid)
                    {
                        _context.Add(detalle);
                        await _context.SaveChangesAsync();
                    }

                    EstadoDetalle estadoDetalle = new EstadoDetalle();
                    estadoDetalle.DetalleId= detalle.Id;
                    estadoDetalle.Detalle= detalle;
                    estadoDetalle.Estado = estadoPendiente;
                    estadoDetalle.Fecha= DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        _context.Add(estadoDetalle);
                        await _context.SaveChangesAsync();
                    }
                    // Se asigna el número de pedido a la variable de sesión
                    // que almacena el número de pedido del carrito
                    HttpContext.Session.SetString("NumPedido", pedido.Id.ToString());
                    HttpContext.Session.SetString("Carrito", 1.ToString());
                }
                else
                {
                    var pedido = _context.Pedidos.Find(int.Parse(HttpContext.Session.GetString("NumPedido")));
                    pedido.PrecioTotal+=producto.Precio;
                    _context.Update(pedido);
                    _context.SaveChanges();

                    Detalle detalle = new Detalle();
                    detalle.PedidoId=int.Parse(HttpContext.Session.GetString("NumPedido"));

                    detalle.Cantidad=1;
                    detalle.Producto = producto;
                    detalle.ProductoId=producto.Id;
                    detalle.Precio=producto.Precio*detalle.Cantidad;

                    if (ModelState.IsValid)
                    {
                        _context.Add(detalle);
                        await _context.SaveChangesAsync();
                    }

                    EstadoDetalle estadoDetalle = new EstadoDetalle();
                    estadoDetalle.DetalleId= detalle.Id;
                    estadoDetalle.Detalle= detalle;
                    estadoDetalle.Estado = await _context.Estados.FirstOrDefaultAsync(estado => estado.Descripcion == "Pendiente");
                    estadoDetalle.Fecha= DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        _context.Add(estadoDetalle);
                        await _context.SaveChangesAsync();
                    }

                    //string actual = HttpContext.Session.GetString("Carrito");
                    
                }
                //var referer = HttpContext.Request.Headers["Referer"].ToString();
                ActualizarContadorCarrito();

                return Redirect("/");
            }
            else
            {
                return Redirect("/Identity/Account/Login");
            }
        }

        public void ActualizarContadorCarrito()
        {
            int cantidad = 0;
            foreach (var detalleCarrito in _context.Detalles.Where(detalle => detalle.PedidoId == int.Parse(HttpContext.Session.GetString("NumPedido"))))
            {
                cantidad+=detalleCarrito.Cantidad;
            }

            HttpContext.Session.SetString("Carrito", cantidad.ToString());
        }

    }
}
