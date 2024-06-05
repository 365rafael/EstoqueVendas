using EstoqueVendas.Context;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueVendas.Controllers
{
    public class SaidaProdutoController : Controller
    {
        private readonly AppDbContext _db;


        public SaidaProdutoController(AppDbContext context)
        {
            _db = context;
        }
        public IActionResult Index()
        {
            //calcualr lucro dos últimos 30 dias
            var hoje = DateTime.Today;
            var dataLimite = hoje.AddDays(-30); // Obtém a data de 30 dias atrás

            var lucroTotal = _db.SaidaProduto
                .Where(s => s.DataSaida >= dataLimite && s.DataSaida <= hoje)
                .Sum(s => s.LucroVenda);

            ViewBag.LucroTotal30Dias = lucroTotal;

            // Calcular total de SaidaProduto.Ativado == true dos últimos 30 dias
            var totalAtivadosUltimos30Dias = _db.SaidaProduto
                .Where(s => s.DataSaida >= dataLimite && s.DataSaida <= hoje && s.Ativado == true)
                .Count();

            ViewBag.TotalAtivadosUltimos30Dias = totalAtivadosUltimos30Dias;

            IEnumerable<SaidaProduto> SaidaProdutos = _db.SaidaProduto
                .Include(p => p.Produto)
                .ThenInclude(p => p.Fornecedor)
                .OrderByDescending(f => f.DataSaida)
                .ToList();
            return View(SaidaProdutos);
        }


        public IActionResult Cadastrar()
        {
            var produtos = _db.Produto.OrderBy(f => f.ProdutoNome).ToList();
            ViewBag.Produtos = produtos;
            var entradas = _db.EntradaProduto.Where(e => e.Ativo == true).ToList();
            ViewBag.Entradas = entradas;
            return View();
        }


        [HttpPost]
        public IActionResult Cadastrar(SaidaProduto SaidaProduto)
        {
            if (SaidaProduto.ProdutoId < 1)
            {
                return Cadastrar();
            }
            if (SaidaProduto != null)
            {


                // Buscando a EntradaProduto com o mesmo NumeroSerie
                var entradaProduto = _db.EntradaProduto.FirstOrDefault(e => e.NumeroSerie == SaidaProduto.NumeroSerie);

                if (entradaProduto != null)
                {
                    // Atualizando a EntradaProduto para Ativo = false
                    entradaProduto.Ativo = false;
                    _db.SaveChanges();
                }

                SaidaProduto.LucroVenda = SaidaProduto.PrecoVenda - entradaProduto.PrecoCusto;
                SaidaProduto.Ativado = false;
                _db.SaidaProduto.Add(SaidaProduto);
                _db.SaveChanges();

                TempData["MensagemSucesso"] = "Cadastro realizado com sucesso!";

                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Editar(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            SaidaProduto SaidaProduto = _db.SaidaProduto
                .Include(f => f.Produto)
                .FirstOrDefault(x => x.Id == id);

            var produtos = _db.Produto.OrderBy(f => f.ProdutoNome).ToList();
            ViewBag.Produtos = produtos;

            if (SaidaProduto == null)
            {
                return NotFound();
            }

            return View(SaidaProduto);
        }

        [HttpPost]
        public IActionResult Editar(SaidaProduto SaidaProduto)
        {

            var entradaProduto = _db.EntradaProduto.FirstOrDefault(e => e.NumeroSerie == SaidaProduto.NumeroSerie);

            SaidaProduto.LucroVenda = SaidaProduto.PrecoVenda - entradaProduto.PrecoCusto;

            _db.SaidaProduto.Update(SaidaProduto);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Edição realizada com sucesso!";

            return RedirectToAction("Index");

        }

        [HttpGet]
        public IActionResult Excluir(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            SaidaProduto SaidaProduto = _db.SaidaProduto.FirstOrDefault(x => x.Id == id);

            var produtos = _db.Produto.OrderBy(f => f.ProdutoNome).ToList();
            ViewBag.Produtos = produtos;

            if (SaidaProduto == null)
            {
                return NotFound();
            }

            return View(SaidaProduto);
        }

        [HttpPost]
        public IActionResult Excluir(SaidaProduto SaidaProduto)
        {
            if (SaidaProduto == null)
            {
                return NotFound();
            }
            // Buscando a EntradaProduto com o mesmo NumeroSerie
            var entradaProduto = _db.EntradaProduto.FirstOrDefault(e => e.NumeroSerie == SaidaProduto.NumeroSerie);

            if (entradaProduto != null)
            {
                // Atualizando a EntradaProduto para Ativo = false
                entradaProduto.Ativo = true;
                _db.SaveChanges();
            }
            _db.SaidaProduto.Remove(SaidaProduto);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Remoção realizada com sucesso!";

            return RedirectToAction("Index");
        }
    }
}
