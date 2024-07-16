using System.ComponentModel.DataAnnotations;

namespace EstoqueVendas.Models
{
    public class LoginModel
    {
        [Key]
        [Required(ErrorMessage = "Digite o login")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Digite a senha")]
        public string Senha { get; set; }

        public bool SenhaValida(string senha)
        {
            return Senha == senha;
        }
    }
}
