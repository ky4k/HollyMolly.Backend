using HM.BLL.Extensions;
using HM.DAL.Enums;

namespace HM.BLL.UnitTests.Extensions;

public class SupportTopicExtensionsTests
{
    [Theory]
    [InlineData(0, "Інше")]
    [InlineData(1, "Питання щодо облікового запису")]
    [InlineData(2, "Питання щодо товару")]
    [InlineData(3, "Питання щодо оплати")]
    [InlineData(999, "(без теми)")]
    public void GetTopicName_ShouldReturnCorrectName(int topic, string expected)
    {
        string actual = ((SupportTopic)topic).GetTopicName();

        Assert.Equal(expected, actual);
    }
}
