namespace EstoqueVendas.Models
{
    public class SaidaProduto
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public DateTime DataSaida { get; set; }
        public string NumeroSerie { get; set; }
        public string? NomeCliente { get; set; }
        public decimal? PrecoVenda { get; set; }
        public decimal? LucroVenda { get; set; }
        public bool? Ativado { get; set; }
        public virtual Produto Produto { get; set; }
    }
}
