using ManagerUser.Application.Common.Models;

namespace ManagerUser.Worker.Services;

public sealed class OutboxMessageProcessor
{
    private readonly ILogger<OutboxMessageProcessor> _logger;

    public OutboxMessageProcessor(ILogger<OutboxMessageProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        // Đây là nơi thay bằng thao tác thật trong hệ thống thực tế:
        // publish RabbitMQ/Kafka, gửi email, gọi API ngoài, cập nhật search index...
        _logger.LogInformation(
            "Sample E - Outbox: publishing message {MessageId}. Type={MessageType}, Retry={RetryCount}/{MaxRetryCount}, Payload={Payload}.",
            message.Id,
            message.Type,
            message.RetryCount,
            message.MaxRetryCount,
            message.Payload);

        await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

        // Message type này dùng để demo retry/error.
        // Khi seed DB có Type='SampleE.AlwaysFail', worker sẽ mark failed và retry theo RetryDelaySeconds.
        if (string.Equals(message.Type, "SampleE.AlwaysFail", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Sample E simulated publish failure.");
        }
    }
}
