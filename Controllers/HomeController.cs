using EstoqueVendas.Context;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EstoqueVendas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;


        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _db = context;
        }


        public IActionResult Index()
        {
            var estoque = _db.EntradaProduto
                .Include(p => p.Produto)
                .Where(e => e.Ativo == true)
                .GroupBy(e => e.Produto.ProdutoNome)
                .Select(g => new
                {
                    ProdutoNome = g.Key,
                    Quantidade = g.Count(),
                    TotalPrecoCusto = g.Sum(e => e.PrecoCusto)
                })
                .OrderBy(p => p.ProdutoNome)
                .ToList();

            return View(estoque);
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
