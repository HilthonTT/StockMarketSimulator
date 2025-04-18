using FluentValidation;

namespace Infrastructure.Events.Options;

internal sealed class MessageBrokerOptionsValidator : AbstractValidator<MessageBrokerOptions>
{
    public MessageBrokerOptionsValidator()
    {
        RuleFor(x => x.QueueName).NotEmpty();
    }
}
