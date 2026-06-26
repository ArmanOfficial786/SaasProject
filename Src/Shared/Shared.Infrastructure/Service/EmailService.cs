//using System.Net.Mail;
//using Microsoft.Extensions.Options;
//using MimeKit;
//using RazorLight;
//using Shared.Application.Configuration;

//namespace Shared.Infrastructure.Services;

//public class EmailService : IEmailService
//{
//    private readonly MailConfig _mailConfig;
//    private readonly RazorLightEngine _engine;

//    public EmailService(IOptions<MailConfig> mailConfig)
//    {
//        _mailConfig = mailConfig.Value;
//        _engine = new RazorLightEngineBuilder()
//            .UseFileSystemProject(
//                Path.Combine(Directory.GetCurrentDirectory(), _mailConfig.TemplatePath)
//            )
//            .UseMemoryCachingProvider()
//            .Build();
//    }

//    private async Task SendEmail(MimeMessage message)
//    {
//        message.From.Add(new MailboxAddress("Mutual Fund", _mailConfig.EmailUserName));
//        using (var client = new SmtpClient())
//        {
//            try
//            {
//                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
//                client.Connect(
//                    _mailConfig.EmailHost,
//                    _mailConfig.EmailPort,
//                    SecureSocketOptions.Auto
//                );
//                client.Authenticate(_mailConfig.EmailUserName, _mailConfig.EmailPassword);
//                _ = await client.SendAsync(message);
//                client.Disconnect(true);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }
//    }

//    public async Task SendEmail(
//        string Email,
//        string msgBodyHtml,
//        string Subject,
//        string[]? attachmentsPath = null
//    )
//    {
//        var message = new MimeMessage();
//        message.To.Add(new MailboxAddress("", Email));
//        message.Subject = Subject;

//        var builder = new BodyBuilder { HtmlBody = msgBodyHtml };
//        if (attachmentsPath != null)
//        {
//            foreach (var item in attachmentsPath)
//            {
//                if (File.Exists(item))
//                    _ = builder.Attachments.Add(item);
//            }
//        }
//        message.Body = builder.ToMessageBody();
//        await SendEmail(message);
//    }

//    public async Task SendEmail<TModel>(
//        string Email,
//        string Subject,
//        string templatePath,
//        TModel model,
//        string[]? attachmentsPath = null
//    )
//    {
//        var message = new MimeMessage();
//        message.To.Add(new MailboxAddress("", Email));
//        message.Subject = Subject;

//        var builder = new BodyBuilder
//        {
//            HtmlBody = await _engine.CompileRenderAsync<TModel>(templatePath, model),
//        };
//        if (attachmentsPath != null)
//        {
//            foreach (var item in attachmentsPath)
//            {
//                if (File.Exists(item))
//                    _ = builder.Attachments.Add(item);
//            }
//        }
//        message.Body = builder.ToMessageBody();
//        await SendEmail(message);
//    }
//}
