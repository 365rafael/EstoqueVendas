using EstoqueVendas.Configurations;
using EstoqueVendas.Context;
using EstoqueVendas.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EstoqueVendas.Services
{
    public class NotificacaoService
    {
        private readonly EmailSettings _emailSettings;
        private readonly AppDbContext _context;

        public NotificacaoService(IOptions<EmailSettings> emailSettings, AppDbContext context)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
        }

        public void EnviarEmailProdutoSemEstoque(int produtoId)
        {
            Produto produto = _context.Produto.Where(p => p.Id == produtoId).First();
            Fornecedor fornecedor = _context.Fornecedor.Where(f => f.Id == produto.FornecedorId).First();
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Sistema de Estoque", _emailSettings.EmailRemetente));
            emailMessage.To.Add(new MailboxAddress("Administrador", _emailSettings.EmailDestinatario));
            emailMessage.Subject = "Produto sem estoque";
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"O produto {produto.ProdutoNome} do fornecedor {fornecedor.FornecedorNome} está sem estoque."
            };

            using (var client = new SmtpClient())
            {
                client.Timeout = 10000; // Timeout em milissegundos (10 segundos)
                client.Connect(_emailSettings.SmtpHost, _emailSettings.SmtpPorta, false);
                client.Authenticate(_emailSettings.EmailRemetente, _emailSettings.SenhaEmail);
                client.Send(emailMessage);
                client.Disconnect(true);
            }
        }
    }
}
