using ReservaCanchas.Infrastructure;

namespace ReservaCanchas.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.Run();
    }
}
