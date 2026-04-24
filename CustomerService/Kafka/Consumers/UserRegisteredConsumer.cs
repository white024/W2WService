// CustomerService/Consumers/UserRegisteredConsumer.cs
using CustomerService.Data.Services.Interfaces;
using Shared.Contracts.Events.User;
using Shared.Kafka.Consumer;
using Shared.Kafka.Settings;
using Shared.Kafka.Topics;

namespace CustomerService.Kafka.Consumers;

public class UserRegisteredConsumer : KafkaConsumerBase<UserRegisteredEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UserRegisteredConsumer(
        KafkaSettings settings,
        ILogger<UserRegisteredConsumer> logger,
        IServiceScopeFactory scopeFactory)
        : base(settings, KafkaTopics.UserTopics.Registered, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task HandleAsync(
        UserRegisteredEvent message, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var companyService = scope.ServiceProvider
            .GetRequiredService<ICompanyService>();

        // Yeni kullanıcı için Company + CompanyMember oluştur
        await companyService.InitializeForNewUserAsync(
            message.UserId,
            message.Name,
            message.Surname,
            message.InviteCode);
    }
}