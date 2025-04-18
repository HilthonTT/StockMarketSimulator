using Application.Abstractions.Messaging;
using Modules.Stocks.Contracts.Shorten;

namespace Modules.Stocks.Application.Shorten.Get;

public sealed record GetShortenUrlQuery(string ShortCode) : IQuery<ShortenUrlResponse>;
