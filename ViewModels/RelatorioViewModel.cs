namespace EstoqueVendas.ViewModels
{
    public class RelatorioViewModel
    {
        public string ProdutoNome { get; set; }
        public int QuantidadeVendida { get; set; }
        public decimal? SomaVendas { get; set; }
        public decimal? LucroTotal { get; set; }
    }

}
