using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace BuildingBlocks.Middlewares
{
    /// <summary>
    ///     Middleware for adding correlation ID to request context.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderName = "X-Correlation-ID";
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = Guid.NewGuid().ToString();

            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues existingCorrelationId))
            {
                correlationId = existingCorrelationId.ToString();
            }

            // Add correlation ID to response headers
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;

            // Add to logging scope for structured logging
            using (_logger.BeginScope("CorrelationId: {CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }

    /// <summary>
    ///     Extension methods for correlation ID middleware.
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        /// <summary>
        ///     Adds correlation ID middleware to the pipeline.
        /// </summary>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
