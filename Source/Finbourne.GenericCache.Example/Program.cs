using Microsoft.Extensions.DependencyInjection;
using Finbourne.GenericCache.Memory;
using Serilog;
using Finbourne.GenericCache.Core.Interface;

namespace Finbourne.GenericCache.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = SetupDI();
            var cache = serviceProvider.GetService<IGenericCache>();
            
            await cache.SetAsync("key1", "value1");
            await cache.SetAsync("key2", "value2");
            await cache.TryGetAsync("key1", out string value1);
            Console.WriteLine($"Value of key1 is {value1}");
            await cache.SetAsync("key3", "value3");
            if(!await cache.TryGetAsync("key2", out string value1_2))
            {
                Console.WriteLine($"Value of key2 is not in cache, because it was correctly removed");
            }
            else
            {
                Console.WriteLine($"Value of key2 is {value1_2}. It should be gone, something is wrong!!!");
            }
            Console.ReadLine();
        }

        private static ServiceProvider SetupDI()
        {
            var services = new ServiceCollection();
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true); 
            });
            services.AddMemoryCache(new MemoryCacheConfig
            {
                Capacity = 2
            });
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}