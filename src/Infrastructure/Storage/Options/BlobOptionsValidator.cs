using FluentValidation;

namespace Infrastructure.Storage.Options;

internal sealed class BlobOptionsValidator : AbstractValidator<BlobOptions>
{
    public BlobOptionsValidator()
    {
        RuleFor(x => x.ContainerName).NotEmpty();
    }
}
