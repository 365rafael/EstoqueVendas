using EstoqueVendas.Context;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueVendas.Controllers
{
    public class EntradaProdutoController : Controller
    {
        private readonly AppDbContext _db;


        public EntradaProdutoController(AppDbContext context)
        {
            _db = context;
        }
        public IActionResult Index()
        {
            IEnumerable<EntradaProduto> EntradaProdutos = _db.EntradaProduto
                .Include(p => p.Produto) // Inclui os dados do produto
                .OrderByDescending(f => f.DataEntrada)
                .ToList();
            return View(EntradaProdutos);
        }


        public IActionResult Cadastrar()
        {
            var produtos = _db.Produto.OrderBy(f => f.ProdutoNome).ToList();
            ViewBag.Produtos = produtos;
            return View();
        }


        [HttpPost]
        public IActionResult Cadastrar(EntradaProduto entradaProduto)
        {
            if (entradaProduto.ProdutoId < 1)
            {
                return Cadastrar();
            }
            if (entradaProduto != null)
            {
                entradaProduto.Ativo = true;
                _db.EntradaProduto.Add(entradaProduto);
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

            EntradaProduto EntradaProduto = _db.EntradaProduto
                .Include(f => f.Produto)
                .FirstOrDefault(x => x.Id == id);

            var produtos = _db.Produto.OrderBy(f => f.ProdutoNome).ToList();
            ViewBag.Produtos = produtos;

            if (EntradaProduto == null)
            {
                return NotFound();
            }

            return View(EntradaProduto);
        }

        [HttpPost]
        public IActionResult Editar(EntradaProduto EntradaProduto)
        {
            EntradaProduto.Ativo = true;

            _db.EntradaProduto.Update(EntradaProduto);
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

            EntradaProduto EntradaProduto = _db.EntradaProduto.FirstOrDefault(x => x.Id == id);

            var produtos = _db.Produto.OrderBy(f => f.ProdutoNome).ToList();
            ViewBag.Produtos = produtos;

            if (EntradaProduto == null)
            {
                return NotFound();
            }

            return View(EntradaProduto);
        }

        [HttpPost]
        public IActionResult Excluir(EntradaProduto EntradaProduto)
        {
            if (EntradaProduto == null)
            {
                return NotFound();
            }

            _db.EntradaProduto.Remove(EntradaProduto);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Remoção realizada com sucesso!";

            return RedirectToAction("Index");
        }
    }
}
