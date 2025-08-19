namespace UrlShortenerApi
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Options;
    using UrlShortenerApi.DataAccess;
    using UrlShortenerApi.Services;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddOptions<DataAccess.Configuration>().Bind(builder.Configuration.GetSection(nameof(DataAccess.Configuration)));

            builder.Services.AddSingleton<CosmosClient>((serviceProvider) =>
            {
                IOptions<DataAccess.Configuration> configurationOptions = serviceProvider.GetRequiredService<IOptions<DataAccess.Configuration>>();
                DataAccess.Configuration configuration = configurationOptions.Value;

                CosmosClient client = new(
                    connectionString: configuration.AzureCosmosDB.ConnectionString
                );
                return client;
            });

            // Registering the repository as singleton for better performance, provided this is safe for CosmosDb
            builder.Services.AddSingleton<IUrlShortcutRepository, CosmosDbUrlShortcutRepository>();
            builder.Services.AddScoped<IUrlShortcutService, UrlShortcutService>();
            builder.Services.AddScoped<IUrlShortcutGenerationService, Sha256UrlShortcutGenerationService>();

            // Register IUrlShortcutService factory to enable creation of instances of UrlShortcutService and CachedUrlShortcutService based on the key provided.
            // This allows for easy switching between different implementations of IUrlShortcutService.
            // For this to work, we must also register the concrete implementations so they can be resolved by the factory.
            builder.Services.AddScoped<CachedUrlShortcutService>();
            builder.Services.AddScoped<UrlShortcutService>();
            builder.Services.AddSingleton<Func<string, IUrlShortcutService>>(serviceProvider =>
            {
                return key =>
                {
                    return key switch
                    {
                        "Base" => serviceProvider.GetService<UrlShortcutService>() ?? throw new InvalidOperationException("Service of type UrlShortcutService is not registered."),
                        "Cached" => serviceProvider.GetService<CachedUrlShortcutService>() ?? throw new InvalidOperationException("Service of type CachedUrlShortcutService is not registered."),
                        _ => throw new ArgumentException("Invalid IUrlShortcutService type")
                    };
                };
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
