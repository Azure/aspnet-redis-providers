using StackExchange.Redis;

namespace Microsoft.Web.Redis
{
    public interface IConnectionMultiplexerFactory
    {
        /// <summary>
        /// This method provides either new or already existing instance of <see cref="IConnectionMultiplexer"/>.
        /// </summary>
        /// <returns>Fully configured connection multiplexer</returns>
        IConnectionMultiplexer CreateMultiplexer();

        /// <summary>
        /// When <see cref="RedisSessionStateProvider"/> fails with <see cref="RedisConnectionException"/>
        /// then it sends to <see cref="IConnectionMultiplexerFactory"/> and attempt to cleanup a failed multiplexer.
        /// Additionally, the factory itself is responsible for providing a clean, fresh instance of IConnectionMultiplexer (it can be the same instance).
        /// </summary>
        /// <param name="connectionMultiplexer">Instance that failed with <see cref="RedisConnectionException"/></param>
        /// <returns>New or refreshed instance</returns>
        IConnectionMultiplexer RestartMultiplexer(IConnectionMultiplexer connectionMultiplexer);
    }
}