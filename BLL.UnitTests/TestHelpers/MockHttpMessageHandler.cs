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

public class MockMultipleHttpMessageHandler(int number) : HttpMessageHandler
{
    private int requestCounter = 0;
    public HttpStatusCode[] StatusCodes { get; set; } = new HttpStatusCode[number];
    public object?[] ResponseContent { get; set; } = new object?[number];
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await Task.FromResult(new HttpResponseMessage
        {
            StatusCode = StatusCodes[requestCounter],
            Content = JsonContent.Create(ResponseContent[requestCounter])
        });
        requestCounter++;
        return response;
    }
}
