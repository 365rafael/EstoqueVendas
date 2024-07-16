using EstoqueVendas.Context;
using EstoqueVendas.Models;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueVendas.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _db;
        public LoginController(AppDbContext context)
        {
            _db = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Entrar(LoginModel loginModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    LoginModel login = _db.Login.FirstOrDefault(x => x.Login == loginModel.Login);

                    if (login != null)
                    {
                        if (login.SenhaValida(loginModel.Senha))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        TempData["MensagemErro"] = "Senha não confere.";
                    }
                    TempData["MensagemErro"] = "Usuário não encontrado.";
                }

                return View("Index");
            }
            catch (Exception e)
            {

                TempData["MensagemErro"] = $"Ops, não foi possível realizar o login.";
                return RedirectToAction("Index");
            }
        }
    }
}
