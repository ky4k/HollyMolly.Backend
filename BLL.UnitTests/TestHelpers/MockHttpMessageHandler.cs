using System.Net;
using System.Net.Http.Json;

namespace HM.BLL.UnitTests.TestHelpers;

public class MockHttpMessageHandler(
    HttpStatusCode statusCode,
    object? responseContent = null
    ) : HttpMessageHandler
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
    public object? ResponseContent { get; set; } = responseContent;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HttpResponseMessage
        {
            StatusCode = StatusCode,
            Content = JsonContent.Create(ResponseContent)
        });
    }
}
