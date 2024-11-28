
using Microsoft.Extensions.Configuration;

namespace Configure
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.Add<MemorySource>((s) => { });
            // Add services to the container.
            builder.Services.Configure<TestOption>(builder.Configuration.GetSection(nameof(TestOption)));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton(s => builder.Configuration.Sources.OfType<MemorySource>().First());

            builder.Services.AddHostedService<HostService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
