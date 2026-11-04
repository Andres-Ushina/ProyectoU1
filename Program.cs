// COMENTARIO EXPLICATIVO: Punto de entrada: registra MVC, Session, repositorios y hash de contraseñas; después configura el middleware y las rutas HTTP.
var builder = WebApplication.CreateBuilder(args);

// Registra MVC para resolver solicitudes HTTP y renderizar vistas Razor.
// Session usa almacenamiento distribuido en memoria para guardar identidad y rol durante la navegación.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
// Los repositorios se crean por solicitud y centralizan el acceso ADO.NET a SQL Server.
builder.Services.AddScoped<ProyectoU1.Data.LoginRepository>();
builder.Services.AddScoped<ProyectoU1.Data.PeliculaRepository>();
builder.Services.AddScoped<ProyectoU1.Data.AlquilerRepository>();
builder.Services.AddScoped<ProyectoU1.Data.UsuariosRepository>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<ProyectoU1.Models.UsuarioLogin>, Microsoft.AspNetCore.Identity.PasswordHasher<ProyectoU1.Models.UsuarioLogin>>();

// Construye la aplicacion con los servicios registrados arriba.
var app = builder.Build();

// Configure the HTTP request pipeline.
// En produccion se muestran errores controlados y se aplica HSTS.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Orden del pipeline: HTTPS, rutas, Session, permisos y endpoints MVC.
app.UseHttpsRedirection();
app.UseRouting();

// Session debe ejecutarse antes de controladores/filtros que leen el usuario y rol.
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

// Ruta convencional: /Controlador/Accion/Id, con Home/Index como inicio.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
