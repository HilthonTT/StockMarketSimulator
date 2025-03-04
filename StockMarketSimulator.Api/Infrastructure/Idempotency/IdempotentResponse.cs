﻿using System.Text.Json.Serialization;

namespace StockMarketSimulator.Api.Infrastructure.Idempotency;

internal sealed class IdempotentResponse
{
    [JsonConstructor]
    public IdempotentResponse(int statusCode, object? value)
    {
        StatusCode = statusCode;
        Value = value;
    }

    public int StatusCode { get; init; }

    public object? Value { get; init; }
}