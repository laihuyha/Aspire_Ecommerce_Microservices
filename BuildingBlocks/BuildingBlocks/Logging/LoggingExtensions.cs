using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Logging
{
    /// <summary>
    ///     Extension methods for structured logging.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        ///     Logs command processing start.
        /// </summary>
        public static void LogCommandProcessing<TCommand>(
            this ILogger logger,
            TCommand command)
        {
            logger.LogInformation("Processing command {CommandType}: {@Command}",
                typeof(TCommand).Name, command);
        }

        /// <summary>
        ///     Logs command processing completion.
        /// </summary>
        public static void LogCommandProcessed<TCommand>(
            this ILogger logger,
            TCommand command,
            string result,
            long elapsedMilliseconds)
        {
            logger.LogInformation("Processed command {CommandType}: {Result} in {ElapsedMilliseconds}ms",
                typeof(TCommand).Name, result, elapsedMilliseconds);
        }

        /// <summary>
        ///     Logs query processing start.
        /// </summary>
        public static void LogQueryProcessing<TQuery>(
            this ILogger logger,
            TQuery query)
        {
            logger.LogInformation("Processing query {QueryType}: {@Query}",
                typeof(TQuery).Name, query);
        }

        /// <summary>
        ///     Logs query processing completion.
        /// </summary>
        public static void LogQueryProcessed<TQuery>(
            this ILogger logger,
            TQuery query,
            string result,
            long elapsedMilliseconds)
        {
            logger.LogInformation("Processed query {QueryType}: {Result} in {ElapsedMilliseconds}ms",
                typeof(TQuery).Name, result, elapsedMilliseconds);
        }
    }
}
