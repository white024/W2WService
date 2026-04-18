namespace Shared.Kafka.Settings;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string SecurityProtocol { get; set; } = "Plaintext";
    public string SaslUsername { get; set; } = string.Empty;
    public string SaslPassword { get; set; } = string.Empty;

    /// <summary>Producer retry sayısı.</summary>
    public int RetryCount { get; set; } = 3;

    public int LingerMs { get; set; } = 5;
    public bool EnableIdempotence { get; set; } = true;

    /// <summary>
    /// Consumer için session timeout.
    /// null bırakılırsa Confluent default (45000ms) kullanılır.
    /// </summary>
    public int? SessionTimeoutMs { get; set; }

    /// <summary>Producer retry backoff (ms). null → Confluent default.</summary>
    public int? RetryBackoffMs { get; set; }
}
