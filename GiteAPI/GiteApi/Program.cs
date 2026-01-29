using GiteApi.Data;

namespace GiteApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSingleton<DatabaseHelper>();
            builder.Services.AddOpenApi(options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<DatabaseHelper>();

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
