using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Shorten;

namespace Modules.Stocks.Application.Shorten.Create;

public sealed record CreateShortenUrlCommand(string Url) : ICommand<ShortenUrlResponse>;
