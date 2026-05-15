using Biblioteca.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de Serviços
builder.Services.AddControllersWithViews();

// Banco de Dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=biblioteca.db"));

// 2. Configuração de Autenticação (Cookie)
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.Cookie.Name = "UsuarioLogin";
        config.LoginPath = "/Login";
        config.AccessDeniedPath = "/Home/AcessoNegado";
        
        // Define quanto tempo o login dura (ex: 60 minutos)
        config.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
        
        // Se o usuário usar o sistema, o tempo de expiração é renovado
        config.SlidingExpiration = true; 
    });
    
var app = builder.Build();

// 3. Configuração do Pipeline de Requisições (A ORDEM IMPORTA AQUI)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // Segurança extra para HTTPS
app.UseStaticFiles();

app.UseRouting();

// A sequência mágica para o login funcionar:
app.UseAuthentication(); // Primeiro: Quem é você?
app.UseAuthorization();  // Segundo: O que você pode fazer?

// Removi a segunda chamada de Authorization que estava aqui embaixo

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();