
using AOP.Mediator;
using AOP.Mediator.Behaviors;
using AOP.Mediator.Commands;
using AOP.Services;
using System.Reflection;

namespace AOP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<BarService>();
            builder.Services.AddSingleton<IBarService>(provider =>
            {
                var barService = provider.GetRequiredService<BarService>();
                return new BarValidator(barService);
            });


            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddOpenBehavior(typeof(ValidateBehavior<,>));
            });
            builder.Services.AddSingleton<IValidator<FooCommand>, FooCommandValidator>();
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
