using Finbourne.GenericCache.Core.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Finbourne.GenericCache.Memory
{
    public static class MemoryCacheExtensions
    {
        public static IServiceCollection AddMemoryCache(this IServiceCollection services, MemoryCacheConfig config)
        {
            services.AddSingleton(config);
            services.AddSingleton<IGenericCache, MemoryCacheCore>();
            return services;
        }
    }
}
