using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewsSubscriptions;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Supports;
using HM.BLL.Models.Users;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace HM.BLL.Services;

public class EmailService(
    IEmailSender emailSender,
    IConfiguration configuration
    ) : IEmailService
{
    private const string RegistrationSubject = "Реєстрація успішна";
    private const string RegistrationTemplate = "<p>Ласкаво просимо до Holly-Molly!</p><br /><p>Вітаємо, Ви успішно зареєструвалися в HollyMolly. Для підтвердження вашої електронної пошти перейдіть за посиланням: <a title=\"{{link}}\" href=\"{{link}}\">{{link}}</a></p><p>___<br />З найкращими побажаннями, HollyMolly</p>";
    private const string ForgetPasswordSubject = "Відновити пароль";
    private const string ForgetPasswordTemplate = "<p>Ви надіслали запит на оновлення паролю до HollyMolly.<br />Щоб скинути поточний пароль перейдіть за посиланням:<br /><a title=\"{{link}}\" href=\"{{link}}\">{{link}}</a></p><p>Якщо це були не Ви, просто ігноруйте цей лист.</p><p>___<br />З найкращими побажаннями, HollyMolly</p>";
    private const string PasswordChangedSubject = "Пароль було змінено";
    private const string PasswordChangedTemplate = "<p>Ваш пароль до HollyMolly було змінено. Якщо ці зміни були зроблені не Вами, перейдіть, будь ласка, за посиланням нижче та встановіть новий пароль:<br /><a title=\"{{link}}\" href=\"{{link}}\">{{link}}</a></p><p>___<br />З найкращими побажаннями, HollyMolly</p>";
    private const string EmailChangedSubject = "Адресу електронної пошти було змінено";
    private const string EmailChangedTemplate = "<p>Вашу адресу електронної пошти для входу до HollyMolly було змінено. Якщо ці зміни були зроблені не Вами, будь ласка, зверніться до технічної підтримки сайту.</p><p>___<br />З найкращими побажаннями, HollyMolly</p>";
    private const string OrderCreatedSubject = "Замовлення створено";
    private const string OrderCreatedTemplate = "<p>Ваше замовлення з ідентифікатором {{orderId}} було успішно створено. Для відслідковування статусу замовлень перейдіть за посиланням та скористайтесь розділом \"Мої замовлення\"<br /><a title=\"{{link}}\" href=\"{{link}}\">{{link}}</a></p><p>___<br />З найкращими побажаннями, HollyMolly</p>";
    private const string OrderStatusUpdatedSubject = "Статус замовлення оновлено";
    private const string OrderStatusUpdatedTemplate = "<p>Статус вашого замовлення з ідентифікатором №{{orderId}} було змінено на {{newStatus}}. Для відслідковування статусу замовлень перейдіть за посиланням та скористайтесь розділом \"Мої замовлення\"<br /><a title=\"{{link}}\" href=\"{{link}}\">{{link}}</a></p></p><p>___<br />З найкращими побажаннями, HollyMolly</p>";
    private const string NewsTemplate = "{{news}}<p>____<br />Відписатися від новин можна за посиланням: <a title=\"{{link}}\" href=\"{{link}}\">{{link}}</a></p><p>___<br />З найкращими побажаннями, HollyMolly</p>";

    private readonly IEmailSender emailSender = emailSender;
    private readonly string supportEmail = Environment.GetEnvironmentVariable("Support:Email") ??
        configuration["Support:Email"] ?? "";
    public async Task<OperationResult> SendSupportEmailAsync(SupportDto supportDto, CancellationToken cancellationToken)
    {
        string subject = $"Новий запит підтримки від {supportDto.Name}";
        string emailBody = $@"
<h2>Новий запит підтримки</h2>
<p><strong>Ім'я:</strong> {supportDto.Name}</p>
<p><strong>Email:</strong> <a href=""mailto:{supportDto.Email}?subject=Re:{supportDto.Topic}"">{supportDto.Email}</a></p>
<p><strong>Тема:</strong> {supportDto.Topic.GetTopicName()}</p>
<p><strong>Опис:</strong> {supportDto.Description}</p>
<p><strong>Номер замовлення:</strong> {supportDto.OrderId}</p>";

        UserMailInfo userMailInfo = new()
        {
            Email = supportEmail
        };

        return await emailSender.SendEmailAsync(userMailInfo, subject, emailBody, cancellationToken);
    }
    public async Task<OperationResult> SendRegistrationResultEmailAsync(string email,
        ConfirmationEmailDto confirmationEmail, CancellationToken cancellationToken)
    {
        UserMailInfo userMailInfo = new()
        {
            Email = email,
        };
        string link = $"https://holly-molly.vercel.app/" +
            $"?userId={Uri.EscapeDataString(confirmationEmail.UserId)}&token={Uri.EscapeDataString(confirmationEmail.Token)}";
        string emailBody = RegistrationTemplate.Replace("{{link}}", link);

        return await emailSender.SendEmailAsync(userMailInfo,
            RegistrationSubject, emailBody, cancellationToken);
    }

    public async Task<OperationResult> SendForgetPasswordEmailAsync(string email,
        ResetPasswordTokenDto resetPassword, CancellationToken cancellationToken)
    {
        UserMailInfo userMailInfo = new()
        {
            Email = email,
        };
        string link = $"https://holly-molly.vercel.app/reset-password/" +
            $"?userId={Uri.EscapeDataString(resetPassword.UserId)}&token={Uri.EscapeDataString(resetPassword.Token)}";
        string emailBody = ForgetPasswordTemplate.Replace("{{link}}", link);

        return await emailSender.SendEmailAsync(userMailInfo, ForgetPasswordSubject,
            emailBody, cancellationToken);
    }

    public async Task<OperationResult> SendPasswordChangedEmail(string email,
        ResetPasswordTokenDto resetPassword, CancellationToken cancellationToken)
    {
        UserMailInfo userMailInfo = new()
        {
            Email = email,
        };
        string link = $"https://holly-molly.vercel.app/" +
            $"?userId={Uri.EscapeDataString(resetPassword.UserId)}&token={Uri.EscapeDataString(resetPassword.Token)}";
        string emailBody = PasswordChangedTemplate.Replace("{{link}}", link);

        return await emailSender.SendEmailAsync(userMailInfo, PasswordChangedSubject, emailBody, cancellationToken);
    }

    public async Task<OperationResult> SendEmailChangedEmailAsync(string email, CancellationToken cancellationToken)
    {
        UserMailInfo userMailInfo = new()
        {
            Email = email,
        };

        return await emailSender.SendEmailAsync(userMailInfo, EmailChangedSubject, EmailChangedTemplate, cancellationToken);
    }

    public async Task<OperationResult> SendOrderCreatedEmailAsync(OrderDto order, CancellationToken cancellationToken)
    {
        UserMailInfo userMailInfo = new()
        {
            Email = order.Customer.Email,
            FirstName = order.Customer.FirstName,
            LastName = order.Customer.LastName,
        };
        string link = $"https://holly-molly.vercel.app/";
        string emailBody = OrderCreatedTemplate
            .Replace("{{link}}", link)
            .Replace("{{orderId}}", order.Id.ToString());

        return await emailSender.SendEmailAsync(userMailInfo, OrderCreatedSubject, emailBody, cancellationToken);
    }

    public async Task<OperationResult> SendOrderStatusUpdatedEmailAsync(OrderDto order, CancellationToken cancellationToken)
    {
        UserMailInfo userMailInfo = new()
        {
            Email = order.Customer.Email,
            FirstName = order.Customer.FirstName,
            LastName = order.Customer.LastName,
        };
        string link = $"https://holly-molly.vercel.app/";
        string emailBody = OrderStatusUpdatedTemplate
            .Replace("{{link}}", link)
            .Replace("{{orderId}}", order.Id.ToString())
            .Replace("{{newStatus}}", order.Status);

        return await emailSender.SendEmailAsync(userMailInfo, OrderStatusUpdatedSubject, emailBody, cancellationToken);
    }

    public async Task<OperationResult> SendNewsEmailAsync(IEnumerable<NewsSubscriptionDto> subscriptions,
        string subject, string textHtml, CancellationToken cancellationToken)
    {
        int succeeded = 0;
        int failed = 0;
        StringBuilder sb = new();
        foreach (NewsSubscriptionDto subscription in subscriptions)
        {
            UserMailInfo userMailInfo = new() { Email = subscription.Email };
            string link = $"https://holly-molly.vercel.app/?token={Uri.EscapeDataString(subscription.RemoveToken)}";
            string emailBody = NewsTemplate
                .Replace("{{link}}", link)
                .Replace("{{news}}", textHtml);
            OperationResult result = await emailSender.SendEmailAsync(userMailInfo, subject, emailBody, cancellationToken);
            if (result.Succeeded)
            {
                succeeded++;
            }
            else
            {
                failed++;
                sb.AppendLine(result.Message);
            }
        }
        string fails = failed > 0 ? $"{failed} were not sent. Errors: " : "";
        sb.Insert(0, $"{succeeded} emails were sent {fails}");
        return new OperationResult(succeeded >= failed, sb.ToString());
    }
}
