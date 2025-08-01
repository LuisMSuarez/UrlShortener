
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using UrlShortenerApi.DataAccess;

namespace UrlShortenerApi
{
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

            builder.Services.AddScoped<IUrlShortcutRepository, CosmosDbUrlShortcutRepository>();

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
