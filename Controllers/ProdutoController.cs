using EstoqueVendas.Context;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueVendas.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly AppDbContext _db;


        public ProdutoController(AppDbContext context)
        {
            _db = context;
        }
        public IActionResult Index()
        {
            IEnumerable<Produto> produtos = _db.Produto
                .Include(p => p.Fornecedor) // Inclui os dados do fornecedor
                .OrderBy(f => f.ProdutoNome)
                .ToList();
            return View(produtos);
        }


        public IActionResult Cadastrar()
        {
            var fornecedores = _db.Fornecedor.OrderBy(f => f.FornecedorNome).ToList();
            ViewBag.Fornecedores = fornecedores;
            return View();
        }


        [HttpPost]
        public IActionResult Cadastrar(Produto Produto)
        {
            if (Produto.FornecedorId < 1 || Produto.ProdutoNome == null)
            {
                return Cadastrar();
            }
            if (Produto != null)
            {
                _db.Produto.Add(Produto);
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

            Produto Produto = _db.Produto
                .Include(f => f.Fornecedor)
                .FirstOrDefault(x => x.Id == id);

            var fornecedores = _db.Fornecedor.OrderBy(f => f.FornecedorNome).ToList();
            ViewBag.Fornecedores = fornecedores;

            if (Produto == null)
            {
                return NotFound();
            }

            return View(Produto);
        }

        [HttpPost]
        public IActionResult Editar(Produto Produto)
        {

            _db.Produto.Update(Produto);
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

            Produto Produto = _db.Produto.FirstOrDefault(x => x.Id == id);


            if (Produto == null)
            {
                return NotFound();
            }

            return View(Produto);
        }

        [HttpPost]
        public IActionResult Excluir(Produto Produto)
        {
            if (Produto == null)
            {
                return NotFound();
            }

            _db.Produto.Remove(Produto);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Remoção realizada com sucesso!";

            return RedirectToAction("Index");
        }
    }
}
