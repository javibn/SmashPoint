using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmashPoint.Models;
using SmashPoint.Data;

namespace SmashPoint
{
    public class RequireProfileCompleteAttribute : TypeFilterAttribute
    {
        public RequireProfileCompleteAttribute() : base(typeof(RequireProfileCompleteFilter)) { }

        private class RequireProfileCompleteFilter : IAsyncActionFilter
        {
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly SmashPointContexto _context;

            public RequireProfileCompleteFilter(IHttpContextAccessor httpContextAccessor, SmashPointContexto context)
            {
                _httpContextAccessor = httpContextAccessor;
                _context = context;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var user = _httpContextAccessor.HttpContext.User;
                                
                string? emailUsuario = user.Identity.Name;
                Proveedor? proveedor = _context.Proveedores.Where(e => e.Correo == emailUsuario)
                .FirstOrDefault();
                if (user.Identity.IsAuthenticated && user.IsInRole("Gestor") && proveedor == null)
                {
                    context.Result = new RedirectToActionResult("Create","MisDatosProveedor", null);
                    return;
                }

                await next();
            }

            /*
            public void ComprobarNombre()
            {
                var user = _httpContextAccessor.HttpContext.User;
                if (user.IsInRole("Usuario"))
                {
                    var cliente = _context.Clientes.FirstOrDefault(cliente => cliente.Email == user.Identity.Name);

                    if (cliente != null)
                    {
                        HttpContext.Session.SetString("Username", cliente.Nombre);
                    }
                }
                if (user.IsInRole("Gestor"))
                {
                    Proveedor proveedor = _context.Proveedores.Where(e => e.Correo == user.Identity.Name).FirstOrDefault();

                    if (proveedor != null)
                    {
                        HttpContext.Session.SetString("Username", proveedor.Nombre);
                    }
                }
                if (user.IsInRole("Administrador"))
                {
                    HttpContext.Session.SetString("Username",  user.Identity.Name);
                }
            }*/
        }
    }
}
