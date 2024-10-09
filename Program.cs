using EstoqueVendas.Configurations;
using EstoqueVendas.Context;
using EstoqueVendas.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Carregar as configurações de e-mail do appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configurar o NotificacaoService
builder.Services.AddTransient<NotificacaoService>();

// Configuração de cultura
var defaultDateCulture = "pt-BR";
var ci = new CultureInfo(defaultDateCulture);
ci.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
ci.DateTimeFormat.LongTimePattern = "HH:mm:ss";
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
