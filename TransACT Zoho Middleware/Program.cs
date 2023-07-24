using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Extensions;
using KontentAiModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Kontent.ai
builder.Services.AddSingleton<ITypeProvider, CustomTypeProvider>();
builder.Services.AddDeliveryClient(builder.Configuration);

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
