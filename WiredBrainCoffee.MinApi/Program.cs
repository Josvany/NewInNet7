using Microsoft.AspNetCore.RateLimiting;
using WiredBrainCoffee.MinApi.Services;
using WiredBrainCoffee.MinApi.Services.Interfaces;
using WiredBrainCoffee.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMenuService, MenuService>();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseRateLimiter(new RateLimiterOptions()
{
    RejectionStatusCode = 429
}.AddConcurrencyLimiter("Concurrency", opt =>
{
    opt.PermitLimit = 1;
}).AddFixedWindowLimiter("FixedWindow", opt =>
{
    opt.Window = TimeSpan.FromSeconds(2);
    opt.PermitLimit = 10;
    opt.QueueLimit = 3; // implementacion de colas para que la peticion no se pierda
    opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
})
);

app.MapGet("/unlimited", () =>
{
    return "Un limited";
});

app.MapGet("/limited", () =>
{
    return "Limited";
}).RequireRateLimiting("Concurrency");

app.MapGet("/orders", (IOrderService orderService) =>
{
    return Results.Ok(orderService.GetOrders());
});

app.MapGet("/orderById", (IOrderService orderService, int id) =>
{
    return Results.Ok(orderService.GetOrderById(id));
});

app.MapPost("/contact", (Contact contact) =>
{
    // save contact to database
});

app.MapGet("/menu", (IMenuService menuService) =>
{
    return menuService.GetMenuItems();
}); ;

app.Run();
