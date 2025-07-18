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

        #region Index
                
        public async Task<IActionResult> Index()
        {
            var hoje = DateTime.Today;

            ViewBag.LucroMesAtual = await CalcularLucroMesAtual(hoje);
            ViewBag.LucroMesAnterior = await CalcularLucroMesAnterior(hoje);
            ViewBag.TotalAtivadosUltimos30Dias = await CalcularTotalAtivadosUltimos30Dias(hoje);
            ViewBag.ProdutosVendidosMesAtual = await ObterProdutosVendidosMesAtual(hoje);

            var saidaProdutos = await ObterSaidasProdutosDosUltimosDias(hoje, 120);
            return View(saidaProdutos);
        }


        #endregion

        #region Cadastro

        public async Task<IActionResult> Cadastrar()
        {
            var produtos = await _db.Produto.OrderBy(f => f.ProdutoNome).ToListAsync();
            ViewBag.Produtos = produtos;
            var entradas = await _db.EntradaProduto.Where(e => e.Ativo == true).ToListAsync();
            ViewBag.Entradas = entradas;
            return View();
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

            SaidaProduto.LucroVenda = CalcularLucroVenda(SaidaProduto.PrecoVenda, (decimal)entradaProduto.PrecoCusto);

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

        #endregion

        #region Edição
              
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
            if (entradaProduto == null)
            {
                TempData["MensagemErro"] = "Número de série inválido!";
                return RedirectToAction("Index");
            }

            SaidaProduto.LucroVenda = CalcularLucroVenda(SaidaProduto.PrecoVenda, (decimal)entradaProduto.PrecoCusto);


            _db.SaidaProduto.Update(SaidaProduto);
            await _db.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Edição realizada com sucesso!";

            return RedirectToAction("Index");

        }

        #endregion


        #region Exclusão
        
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

        #endregion

        #region Relatório
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
        #endregion

        #region Utilitarios


        private async Task<decimal> CalcularLucroMesAtual(DateTime hoje)
        {
            var primeiroDia = new DateTime(hoje.Year, hoje.Month, 1);
            return (decimal)await _db.SaidaProduto
                .Where(s => s.DataSaida >= primeiroDia && s.DataSaida <= hoje)
                .SumAsync(s => s.LucroVenda);
        }

        private async Task<decimal> CalcularLucroMesAnterior(DateTime hoje)
        {
            var primeiroDiaMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
            var primeiroDiaMesAnterior = primeiroDiaMesAtual.AddMonths(-1);
            var ultimoDiaMesAnterior = primeiroDiaMesAtual.AddDays(-1);

            return (decimal)await _db.SaidaProduto
                .Where(s => s.DataSaida >= primeiroDiaMesAnterior && s.DataSaida <= ultimoDiaMesAnterior)
                .SumAsync(s => s.LucroVenda);
        }

        private async Task<int> CalcularTotalAtivadosUltimos30Dias(DateTime hoje)
        {
            var dataLimite = hoje.AddDays(-30);
            return await _db.SaidaProduto
                .Where(s => s.DataSaida >= dataLimite && s.DataSaida <= hoje && s.Ativado == true)
                .CountAsync();
        }

        private async Task<List<object>> ObterProdutosVendidosMesAtual(DateTime hoje)
        {
            var primeiroDia = new DateTime(hoje.Year, hoje.Month, 1);

            return await _db.SaidaProduto
                .Where(s => s.DataSaida >= primeiroDia && s.DataSaida <= hoje)
                .GroupBy(s => s.Produto.ProdutoNome)
                .Select(g => new
                {
                    ProdutoNome = g.Key,
                    QuantidadeVendida = g.Count()
                })
                .OrderBy(p => p.ProdutoNome)
                .ToListAsync<object>();
        }

        private async Task<List<SaidaProduto>> ObterSaidasProdutosDosUltimosDias(DateTime hoje, int dias)
        {
            var dataInicial = hoje.AddDays(-dias);
            return await _db.SaidaProduto
                .Where(p => p.DataSaida >= dataInicial && p.DataSaida <= hoje)
                .Include(p => p.Produto)
                    .ThenInclude(p => p.Fornecedor)
                .OrderByDescending(f => f.DataSaida)
                .ToListAsync();
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


        private decimal CalcularLucroVenda(decimal precoVenda, decimal precoCusto)
        {
            return precoVenda - precoCusto;
        }


        #endregion
    }
}
