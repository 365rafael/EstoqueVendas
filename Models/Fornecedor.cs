using System.ComponentModel.DataAnnotations;

namespace EstoqueVendas.Models
{
    public class Fornecedor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "O nome do fornecedor é obrigatório")]
        [StringLength(100)]
        public string FornecedorNome { get; set; }
    }
}
