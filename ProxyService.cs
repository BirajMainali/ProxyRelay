using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProxyRelay
{
    public class ProxyService : BackgroundService
    {
        private const string SourceUrl = "http://localhost:8080/";
        private const string DestinationUrl = "https://restcountries.com/v3.1/all";
        

        private readonly ILogger<ProxyService> _logger;
        private readonly HttpClient _httpClient;

        public ProxyService(ILogger<ProxyService> logger)
        {
            _logger = logger;
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(httpClientHandler);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var listener = new HttpListener();
                listener.Prefixes.Add(SourceUrl);
                listener.Start();
                _logger.LogInformation("Proxy server started. Listening on " + SourceUrl);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var context = await listener.GetContextAsync();
                    await HandleRequestAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred: " + ex.Message);
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var destinationRequest = new HttpRequestMessage();
            destinationRequest.RequestUri = new Uri(DestinationUrl + context.Request.Url?.PathAndQuery);
            destinationRequest.Method = new HttpMethod(context.Request.HttpMethod);
            destinationRequest.Content = new StreamContent(context.Request.InputStream);

            if (!string.IsNullOrEmpty(context.Request.ContentType))
            {
                destinationRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType);
            }

            foreach (var header in context.Request.Headers.AllKeys)
            {
                if (header != null) destinationRequest.Headers.TryAddWithoutValidation(header, context.Request.Headers[header]);
            }

            using (var destinationResponse = await _httpClient.SendAsync(destinationRequest))
            {
                context.Response.StatusCode = (int)destinationResponse.StatusCode;
                context.Response.ContentType = destinationResponse.Content.Headers.ContentType?.ToString();

                if (destinationResponse.Content.Headers.ContentEncoding.Contains("gzip"))
                {
                    await using var responseStream = await destinationResponse.Content.ReadAsStreamAsync();
                    await using var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress);
                    using var reader = new StreamReader(gzipStream, Encoding.UTF8);
                    var responseContent = await reader.ReadToEndAsync();
                    var responseBytes = Encoding.UTF8.GetBytes(responseContent);
                    context.Response.ContentLength64 = responseBytes.Length;
                    await context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
                else
                {
                    await using var responseStream = await destinationResponse.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var responseContent = await reader.ReadToEndAsync();
                    var responseBytes = Encoding.UTF8.GetBytes(responseContent);
                    context.Response.ContentLength64 = responseBytes.Length;
                    await context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }

            context.Response.Close();
        }
    }
}