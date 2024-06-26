using EstoqueVendas.Context;
using EstoqueVendas.ViewModels;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueVendas.ViewModels;

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
            var hoje = DateTime.Today;

            // Mês atual
            var primeiroDiaMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
            var lucroMesAtual = _db.SaidaProduto
                .Where(s => s.DataSaida >= primeiroDiaMesAtual && s.DataSaida <= hoje)
                .Sum(s => s.LucroVenda);
            ViewBag.LucroMesAtual = lucroMesAtual;

            // Mês anterior
            var primeiroDiaMesAnterior = primeiroDiaMesAtual.AddMonths(-1);
            var ultimoDiaMesAnterior = primeiroDiaMesAtual.AddDays(-1);
            var lucroMesAnterior = _db.SaidaProduto
                .Where(s => s.DataSaida >= primeiroDiaMesAnterior && s.DataSaida <= ultimoDiaMesAnterior)
                .Sum(s => s.LucroVenda);
            ViewBag.LucroMesAnterior = lucroMesAnterior;

            // Calcular total de SaidaProduto.Ativado == true dos últimos 30 dias
            var dataLimite = hoje.AddDays(-30);
            var totalAtivadosUltimos30Dias = _db.SaidaProduto
                .Where(s => s.DataSaida >= dataLimite && s.DataSaida <= hoje && s.Ativado == true)
                .Count();
            ViewBag.TotalAtivadosUltimos30Dias = totalAtivadosUltimos30Dias;

            // Carregar os produtos de saída
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

        [HttpPost]
        public IActionResult Relatorio(DateTime dataInicial, DateTime dataFinal)
        {
            var relatorioData = _db.SaidaProduto
                .Include(p => p.Produto)
                .Where(s => s.DataSaida >= dataInicial && s.DataSaida <= dataFinal)
                .GroupBy(s => s.Produto.ProdutoNome)
                .Select(g => new RelatorioViewModel
                {
                    ProdutoNome = g.Key,
                    QuantidadeVendida = g.Count(),
                    SomaVendas = g.Sum(s => s.PrecoVenda),
                    LucroTotal = g.Sum(s => s.LucroVenda)
                })
                .OrderBy(r => r.ProdutoNome)
                .ToList();

            var lucroTotalPeriodo = relatorioData.Sum(r => r.LucroTotal);
            ViewBag.LucroTotalPeriodo = lucroTotalPeriodo;

            // Dados de vendas por fornecedor
            var fornecedorData = _db.SaidaProduto
                .Include(p => p.Produto.Fornecedor)
                .Where(s => s.DataSaida >= dataInicial && s.DataSaida <= dataFinal)
                .GroupBy(s => s.Produto.Fornecedor.FornecedorNome)
                .Select(g => new FornecedorViewModel
                {
                    FornecedorNome = g.Key,
                    QuantidadeVendida = g.Count(),
                    SomaVendas = g.Sum(s => s.PrecoVenda),
                    LucroTotal = g.Sum(s => s.LucroVenda)
                })
                .OrderBy(f => f.FornecedorNome)
                .ToList();

            ViewBag.FornecedorData = fornecedorData;

            return View("Relatorio", relatorioData);
        }
    }
}
