using HM.DAL.Enums;

namespace HM.BLL.Extensions;

static class SupportTopicExtensions
{
    public static string GetTopicName(this SupportTopic topic)
    {
        return topic switch
        {
            SupportTopic.Other => "Інше",
            SupportTopic.AccountIssues => "Питання щодо облікового запису",
            SupportTopic.ProductQuestions => "Питання щодо товару",
            SupportTopic.PaymentQuestions => "Питання щодо оплати",
            _ => "(без теми)",
        };
    }
}
