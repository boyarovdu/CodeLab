using System.Net.Http.Headers;
using System.Text.Json;

using Distributed.Replication.Kafka.Tests.Utils;
using Distributed.Replication.Kafka.TestWebClient.Models;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public partial class KafkaWebClientTest
{
    private HttpClient _httpClient;
    
    protected async Task<HttpResponseMessage> ConsumeAsync(string port)
    {
        return await _httpClient.GetAsync($"http://localhost:{port}/consume");
    }

    protected async Task<HttpResponseMessage> ProduceAsync(string port, string topic, string message)
    {
        return await _httpClient.PostAsync(
            $"http://localhost:{port}/produce?topic={topic}",
            new StringContent(
                JsonSerializer.Serialize(new ProduceRequest { Message = message }),
                mediaType: new MediaTypeHeaderValue("application/json")
            ));
    }

    protected async Task<HttpResponseMessage> SubscribeAsync(string port, string topic)
    {
        return await _httpClient.PostAsync(
            $"http://localhost:{port}/consume?topic={topic}",
            null);
    }
    
    protected static async Task ServiceHealthy(string producerPort,
        int timeoutMs = (1 * 30 * 1000),
        string host = "localhost",
        string endpointRelativePath = "health")
    {
        using var httpClient = new HttpClient();

        var healthUri = new Uri($"http://{host}:{producerPort}/{endpointRelativePath}");

        await TestUtil.WaitUntilAsync(timeoutMs, async () =>
            (await httpClient.GetAsync(healthUri)).IsSuccessStatusCode);
    }

    private void InitHttpClient()
    {
        _httpClient = new HttpClient();
    }
    
    private void DisposeHttpClient()
    {
        _httpClient.Dispose();
    }
}