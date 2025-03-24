﻿using Microsoft.Extensions.Http.Resilience;

namespace StockMarketSimulator.Api.Extensions;

public static class ResilienceHttpClientBuilderExtensions
{
    public static IHttpClientBuilder CustomRemoveAllResilienceHandlers(this IHttpClientBuilder builder)
    {
        builder.ConfigureAdditionalHttpMessageHandlers(static (handlers, _) =>
        {
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                if (handlers[i] is ResilienceHandler)
                {
                    handlers.RemoveAt(i);
                }
            }
        });

        return builder;
    }
}
