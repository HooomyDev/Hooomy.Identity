using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Mail;
using System.Threading;
using static Duende.IdentityServer.Models.IdentityResources;

namespace Hooome.Identity.Services;

public interface IEmailService
{
    Task SendPasswordEmail(string toEmail, string firstName, string surname, string password, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmail(string toEmail, string username, string resetLink, CancellationToken cancellationToken);
    Task SendPasswordChangedEmail(string toEmail, string username, CancellationToken cancellationToken);
}

public class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendPasswordResetEmail(string toEmail, string username, string resetLink, CancellationToken cancellationToken)
    {
        var smtp = configuration.GetSection("SmtpSettings");
        var host = smtp["Host"] ?? throw new InvalidOperationException("SMTP Host not configured");
        var port = int.Parse(smtp["Port"] ?? "587");
        var senderEmail = smtp["SenderEmail"] ?? throw new InvalidOperationException("SMTP SenderEmail not configured");
        var senderPassword = smtp["SenderPassword"] ?? throw new InvalidOperationException("SMTP SenderPassword not configured");
        var senderName = smtp["SenderName"] ?? "Hooome";
        var loginUrl = smtp["LoginUrl"] ?? "https://hooome.com/login";

        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
                <h2 style='color: #3dbfa3;'>Восстановление пароля</h2>
                <p>Здравствуйте, {username}!</p>
                <p>Вы запросили восстановление пароля. Перейдите по ссылке ниже, чтобы установить новый пароль:</p>
                <a href='{resetLink}' style='display: inline-block; padding: 10px 20px; background: #3dbfa3; color: white; text-decoration: none; border-radius: 5px;'>Сбросить пароль</a>
                <p>Ссылка действительна в течение 24 часов.</p>
                <p>Если вы не запрашивали восстановление пароля, просто проигнорируйте это письмо.</p>
            </div>";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Добро пожаловать в Hooome!",
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task SendPasswordChangedEmail(string toEmail, string username, CancellationToken cancellationToken)
    {
        var smtp = configuration.GetSection("SmtpSettings");
        var host = smtp["Host"] ?? throw new InvalidOperationException("SMTP Host not configured");
        var port = int.Parse(smtp["Port"] ?? "587");
        var senderEmail = smtp["SenderEmail"] ?? throw new InvalidOperationException("SMTP SenderEmail not configured");
        var senderPassword = smtp["SenderPassword"] ?? throw new InvalidOperationException("SMTP SenderPassword not configured");
        var senderName = smtp["SenderName"] ?? "Hooome";
        var loginUrl = smtp["LoginUrl"] ?? "https://hooome.com/login";

        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
                <h2 style='color: #3dbfa3;'>Пароль изменён</h2>
                <p>Здравствуйте, {username}!</p>
                <p>Ваш пароль был успешно изменён.</p>
                <p>Если это были не вы, немедленно свяжитесь с поддержкой.</p>
            </div>";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Добро пожаловать в Hooome!",
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task SendPasswordEmail(string toEmail, string firstName, string surname, string password, CancellationToken cancellationToken = default)
    {
        var smtp = configuration.GetSection("SmtpSettings");
        var host = smtp["Host"] ?? throw new InvalidOperationException("SMTP Host not configured");
        var port = int.Parse(smtp["Port"] ?? "587");
        var senderEmail = smtp["SenderEmail"] ?? throw new InvalidOperationException("SMTP SenderEmail not configured");
        var senderPassword = smtp["SenderPassword"] ?? throw new InvalidOperationException("SMTP SenderPassword not configured");
        var senderName = smtp["SenderName"] ?? "Hooome";
        var loginUrl = smtp["LoginUrl"] ?? "https://hooome.com/login";

        var body = $"""
            <div style="font-family: 'Segoe UI', Arial, sans-serif; max-width: 520px; margin: 0 auto; background: #ffffff; color: #122435; border-radius: 16px; overflow: hidden; border: 1px solid #c0c7d2;">
                <div style="background: #3dbfa3; padding: 32px 24px; text-align: center;">
                    <h1 style="margin: 0; font-size: 28px; color: #fff;">Hooome</h1>
                </div>
                <div style="padding: 32px 28px; background: #fff;">
                    <h2 style="color: #3dbfa3; margin: 0 0 12px;">Добро пожаловать в Hooome!</h2>
            
                    <p style="color: #122435;">Привет, <strong style="color:#3dbfa3">{firstName} {surname}</strong>!</p>
            
                    <div style="background: #e3f7f3; border: 1px solid #c0c7d2; border-radius: 14px; padding: 24px; margin: 28px 0; text-align: center;">
                        <p style="color: #122435; margin: 0 0 12px; font-size: 12px; opacity: 0.7;">Ваш временный пароль</p>
                        <div style="background: #fff; border-radius: 12px; padding: 16px; border: 1px solid #c0c7d2;">
                            <span style="font-size: 32px; font-weight: 700; letter-spacing: 4px; color: #3dbfa3; font-family: monospace;">{System.Net.WebUtility.HtmlEncode(password)}</span>
                        </div>
                        <p style="color: #d32f2f; margin: 16px 0 0; font-size: 13px;">⚠️ Обязательно смените пароль при первом входе</p>
                    </div>
            
                    <div style="background: #e3f7f3; border-radius: 12px; padding: 20px; margin: 24px 0; border: 1px solid #c0c7d2;">
                        <p style="margin: 0 0 12px; color: #3dbfa3; font-weight: 600;">🔑 Данные для входа</p>
                        <div style="margin-bottom: 8px;">
                            <span style="color: #122435; opacity: 0.7; width: 70px; display: inline-block;">Email:</span>
                            <span style="color: #122435;">{toEmail}</span>
                        </div>
                        <div>
                            <span style="color: #122435; opacity: 0.7; width: 70px; display: inline-block;">Пароль:</span>
                            <span style="color: #3dbfa3; font-family: monospace;">{password}</span>
                        </div>
                    </div>
            
                    <a href="{loginUrl}" style="display: block; background: #3dbfa3; color: #fff; text-align: center; text-decoration: none; padding: 14px 24px; border-radius: 40px; font-weight: 600; margin: 28px 0 20px;">
                        Войти в Hooome →
                    </a>
            
                    <hr style="border: none; border-top: 1px solid #dde3ea; margin: 24px 0 16px;">
            
                    <p style="color: #122435; opacity: 0.5; font-size: 12px; text-align: center;">
                        Это письмо создано автоматически. Если вы не ожидали приглашения, просто проигнорируйте его.
                    </p>
            
                    <p style="color: #122435; opacity: 0.4; font-size: 11px; text-align: center; margin: 16px 0 0;">
                        © 2026 Hooome. Все права защищены.
                    </p>
                </div>
            </div>
            """;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Добро пожаловать в Hooome!",
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message, cancellationToken);
    }
}