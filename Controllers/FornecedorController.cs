using EstoqueVendas.Context;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueVendas.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly AppDbContext _db;


        public FornecedorController(AppDbContext context)
        {
            _db = context;
        }
        public IActionResult Index()
        {
            IEnumerable<Fornecedor> fornecedores = _db.Fornecedor.OrderBy(f => f.FornecedorNome).ToList();
            return View(fornecedores);
        }

        #region Cadastrar

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                _db.Fornecedor.Add(fornecedor);
                _db.SaveChanges();

                TempData["MensagemSucesso"] = "Cadastro realizado com sucesso!";

                return RedirectToAction("Index");
            }

            return View();
        }

        #endregion

        #region Editar

        [HttpGet]
        public IActionResult Editar(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Fornecedor fornecedor = _db.Fornecedor.FirstOrDefault(x => x.Id == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        [HttpPost]
        public IActionResult Editar(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                _db.Fornecedor.Update(fornecedor);
                _db.SaveChanges();

                TempData["MensagemSucesso"] = "Edição realizada com sucesso!";

                return RedirectToAction("Index");
            }

            TempData["MensagemErro"] = "Preencha os campos corretamente.";

            return View(fornecedor);
        }

        #endregion

        #region Excluir

        [HttpGet]
        public IActionResult Excluir(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Fornecedor fornecedor = _db.Fornecedor.FirstOrDefault(x => x.Id == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            return View(fornecedor);
        }

        [HttpPost]
        public IActionResult Excluir(Fornecedor fornecedor)
        {
            if (fornecedor == null)
            {
                return NotFound();
            }

            _db.Fornecedor.Remove(fornecedor);
            _db.SaveChanges();

            TempData["MensagemSucesso"] = "Remoção realizada com sucesso!";

            return RedirectToAction("Index");
        }
        #endregion
    }
}
