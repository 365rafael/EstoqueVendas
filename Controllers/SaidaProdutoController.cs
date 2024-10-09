using EstoqueVendas.Context;
using EstoqueVendas.ViewModels;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EstoqueVendas.Services;

namespace EstoqueVendas.Controllers
{
    public class SaidaProdutoController : Controller
    {
        private readonly AppDbContext _db;
        private readonly NotificacaoService _notificacaoService;


        public SaidaProdutoController(AppDbContext context, NotificacaoService notificacaoService)
        {
            _db = context;
            _notificacaoService = notificacaoService;
        }
        public async Task<IActionResult> Index()
        {
            var hoje = DateTime.Today;

            // Mês atual
            var primeiroDiaMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
            var lucroMesAtual = await _db.SaidaProduto
                .Where(s => s.DataSaida >= primeiroDiaMesAtual && s.DataSaida <= hoje)
                .SumAsync(s => s.LucroVenda);
            ViewBag.LucroMesAtual = lucroMesAtual;

            // Mês anterior
            var primeiroDiaMesAnterior = primeiroDiaMesAtual.AddMonths(-1);
            var ultimoDiaMesAnterior = primeiroDiaMesAtual.AddDays(-1);
            var lucroMesAnterior = await _db.SaidaProduto
                .Where(s => s.DataSaida >= primeiroDiaMesAnterior && s.DataSaida <= ultimoDiaMesAnterior)
                .SumAsync(s => s.LucroVenda);
            ViewBag.LucroMesAnterior = lucroMesAnterior;

            // Total de SaidaProduto.Ativado == true nos últimos 30 dias
            var dataLimite = hoje.AddDays(-30);
            var totalAtivadosUltimos30Dias = await _db.SaidaProduto
                .Where(s => s.DataSaida >= dataLimite && s.DataSaida <= hoje && s.Ativado == true)
                .CountAsync();
            ViewBag.TotalAtivadosUltimos30Dias = totalAtivadosUltimos30Dias;

            // Carregar os produtos de saída
            var data45Dias = hoje.AddDays(-45);
            var SaidaProdutos = await _db.SaidaProduto
                .Where(p => p.DataSaida >= data45Dias && p.DataSaida <= hoje)
                .Include(p => p.Produto)
                .ThenInclude(p => p.Fornecedor)
                .OrderByDescending(f => f.DataSaida)
                .ToListAsync();

            return View(SaidaProdutos);
        }



        public async Task<IActionResult> Cadastrar()
        {
            var produtos = await _db.Produto.OrderBy(f => f.ProdutoNome).ToListAsync();
            ViewBag.Produtos = produtos;
            var entradas = await _db.EntradaProduto.Where(e => e.Ativo == true).ToListAsync();
            ViewBag.Entradas = entradas;
            return View();
        }


        [HttpGet]
        public async Task<JsonResult> GetNumerosSeriePorProduto(int produtoId)
        {
            var numerosSerie = await _db.EntradaProduto
                .Where(e => e.ProdutoId == produtoId && e.Ativo == true)
                .Select(e => new { e.NumeroSerie })
                .ToListAsync();

            return Json(numerosSerie);
        }




        [HttpPost]
        public async Task<IActionResult> Cadastrar(SaidaProduto SaidaProduto)
        {
            
            if (SaidaProduto.ProdutoId < 1)
            {
                TempData["MensagemErro"] = "Produto inválido. Por favor, selecione um produto.";
                return RedirectToAction("Index");
            }

            var entradaProduto = await _db.EntradaProduto.FirstOrDefaultAsync(e => e.NumeroSerie == SaidaProduto.NumeroSerie);

            if (entradaProduto == null)
            {
                TempData["MensagemErro"] = "Número de série não encontrado!";
                return RedirectToAction("Index");
            }

            SaidaProduto.LucroVenda = SaidaProduto.PrecoVenda - entradaProduto.PrecoCusto;
            SaidaProduto.Ativado = false;
            _db.SaidaProduto.Add(SaidaProduto);

            entradaProduto.Ativo = false;
            await _db.SaveChangesAsync();

            var estoqueRestante = await _db.EntradaProduto
                .CountAsync(e => e.ProdutoId == SaidaProduto.ProdutoId && e.Ativo == true);

            if (estoqueRestante == 0)
            {
                _notificacaoService.EnviarEmailProdutoSemEstoque(SaidaProduto.ProdutoId);
                TempData["MensagemSemEstoque"] = "Vendeu a última unidade!";
            }

            TempData["MensagemSucesso"] = "Cadastro realizado com sucesso!";

            return RedirectToAction("Index");
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
        public async Task<IActionResult> Editar(SaidaProduto SaidaProduto)
        {

            var entradaProduto = await _db.EntradaProduto.FirstOrDefaultAsync(e => e.NumeroSerie == SaidaProduto.NumeroSerie);
            SaidaProduto.LucroVenda = SaidaProduto.PrecoVenda - entradaProduto.PrecoCusto;

            _db.SaidaProduto.Update(SaidaProduto);
            await _db.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Edição realizada com sucesso!";

            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Excluir(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            SaidaProduto SaidaProduto = await _db.SaidaProduto.FirstOrDefaultAsync(x => x.Id == id);

            var produtos = await _db.Produto.OrderBy(f => f.ProdutoNome).ToListAsync();
            ViewBag.Produtos = produtos;

            if (SaidaProduto == null)
            {
                return NotFound();
            }

            return View(SaidaProduto);
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(SaidaProduto SaidaProduto)
        {
            if (SaidaProduto == null)
            {
                return NotFound();
            }
            // Buscando a EntradaProduto com o mesmo NumeroSerie
            var entradaProduto = await _db.EntradaProduto.FirstOrDefaultAsync(e => e.NumeroSerie == SaidaProduto.NumeroSerie);

            if (entradaProduto != null)
            {
                // Atualizando a EntradaProduto para Ativo = false
                entradaProduto.Ativo = true;
                await _db.SaveChangesAsync();
            }
            _db.SaidaProduto.Remove(SaidaProduto);
            await _db.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Remoção realizada com sucesso!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Relatorio(DateTime dataInicial, DateTime dataFinal)
        {
            var relatorioData = await _db.SaidaProduto
                .Include(p => p.Produto)
                .Where(s => s.DataSaida >= dataInicial && s.DataSaida <= dataFinal)
                .GroupBy(s => s.Produto.ProdutoNome)
                .Select(g => new RelatorioViewModel
                {
                    ProdutoNome = g.Key,
                    QuantidadeVendida = g.Count(),
                    SomaVendas = g.Sum(s => s.PrecoVenda),
                    LucroTotal = g.Sum(s => s.LucroVenda),
                    QuantidadeAtivada = g.Where(q => q.Ativado == true).Count(),
                })
                .OrderBy(r => r.ProdutoNome)
                .ToListAsync();

            var lucroTotalPeriodo = relatorioData.Sum(r => r.LucroTotal);
            var quantidadeTotalVendida = relatorioData.Sum(r => r.QuantidadeVendida);
            var valorTotalVendas = relatorioData.Sum(r => r.SomaVendas);
            var totalAtivada = relatorioData.Sum(r => r.QuantidadeAtivada);

            ViewBag.LucroTotalPeriodo = lucroTotalPeriodo;
            ViewBag.QuantidadeTotalVendida = quantidadeTotalVendida;
            ViewBag.ValorTotalVendas = valorTotalVendas;
            ViewBag.DataInicial = dataInicial;
            ViewBag.DataFinal = dataFinal;
            ViewBag.TotalAtivada = totalAtivada;

            // Dados de vendas por fornecedor
            var fornecedorData = await _db.SaidaProduto
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
                .ToListAsync();

            ViewBag.FornecedorData = fornecedorData;

            return View("Relatorio", relatorioData);
        }
    }
}
