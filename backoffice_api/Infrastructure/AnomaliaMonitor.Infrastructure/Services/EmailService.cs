using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace AnomaliaMonitor.Infrastructure.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        
        // Se as configurações SMTP não estiverem definidas, apenas simula o envio
        if (string.IsNullOrEmpty(smtpSettings["Host"]))
        {
            // Em um ambiente de desenvolvimento, apenas loga o link
            Console.WriteLine($"=== EMAIL DE RECUPERAÇÃO DE SENHA ===");
            Console.WriteLine($"Para: {email}");
            Console.WriteLine($"Link de recuperação: {resetUrl}?email={email}&token={Uri.EscapeDataString(resetToken)}");
            Console.WriteLine($"=====================================");
            
            // Simula delay de envio
            await Task.Delay(1000);
            return;
        }

        var subject = "Recuperação de Senha - AnomaliaMonitor";
        var body = GeneratePasswordResetEmailBody(resetUrl, resetToken, email);

        using var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"] ?? "587"));
        client.EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
        client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);

        var message = new MailMessage
        {
            From = new MailAddress(smtpSettings["From"]!, "AnomaliaMonitor"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(email);

        await client.SendMailAsync(message);
    }

    private string GeneratePasswordResetEmailBody(string resetUrl, string resetToken, string email)
    {
        var fullResetUrl = $"{resetUrl}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Recuperação de Senha</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #1a1a2e; color: #ffffff; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 10px; padding: 30px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ width: 60px; height: 60px; background: rgba(255,255,255,0.1); border-radius: 50%; display: inline-flex; align-items: center; justify-content: center; margin-bottom: 20px; }}
        .content {{ text-align: center; }}
        .button {{ display: inline-block; background: #4facfe; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: rgba(255,255,255,0.7); }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>⚠️</div>
            <h1>Recuperação de Senha</h1>
        </div>
        
        <div class='content'>
            <p>Olá!</p>
            <p>Você solicitou a recuperação de senha para sua conta no <strong>AnomaliaMonitor</strong>.</p>
            <p>Clique no botão abaixo para definir uma nova senha:</p>
            
            <a href='{fullResetUrl}' class='button'>Redefinir Senha</a>
            
            <p><strong>Este link expira em 24 horas.</strong></p>
            <p>Se você não solicitou esta recuperação, pode ignorar este email.</p>
        </div>
        
        <div class='footer'>
            <p>Este é um email automático, não responda.</p>
            <p>AnomaliaMonitor - Sistema de Monitoramento de Anomalias</p>
        </div>
    </div>
</body>
</html>";
    }
}