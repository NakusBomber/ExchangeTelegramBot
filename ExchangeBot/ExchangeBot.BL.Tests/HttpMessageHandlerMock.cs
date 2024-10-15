using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeBot.BL.Tests;

public class HttpMessageHandlerMock(HttpStatusCode code, string json) : HttpMessageHandler
{
    private readonly string json = json;
    private readonly HttpStatusCode code = code;

    sealed protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken
        )
    {
        var response = new HttpResponseMessage()
        {
            StatusCode = code,
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        return Task.FromResult(response);
    }
}
