using Scalar.AspNetCore;
using Api;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // app.UseAuthentication();
    // app.UseAuthorization();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();
}

app.Run();