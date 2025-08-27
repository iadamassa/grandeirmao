using CheckAnomaliaApi.Consumers;
using CheckAnomaliaApi.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IHasAnomalieService, HasAnomalieService>();
builder.Services.AddScoped<IHasAnomalieService, HasAnomalieService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PageProcessedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")
                          ?? "amqp://admin:admin123@rabbitmq:5672/");

        cfg.Host(uri);

        cfg.ReceiveEndpoint("page-processed-queue", e =>
        {
            e.ConfigureConsumer<PageProcessedConsumer>(context);
            e.PrefetchCount = 1;
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
