using System.Globalization;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement;
using NpDirectory.Api;
using NpDirectory.Application;
using NpDirectory.Application.Repositories;
using NpDirectory.Application.Requests;
using NpDirectory.Application.Services;
using NpDirectory.Infrastructure;
using NpDirectory.Infrastructure.Repositories;
using NpDirectory.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ValidateModelAttribute>();
});

builder.Services.AddFeatureManagement();

builder.Services.AddScoped<INaturalPersonsService, NaturalPersonsService>();
builder.Services.AddScoped<IRelationService, RelationService>();
builder.Services.AddSingleton<IFileService, FileSystemFileService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<INaturalPersonsRepository, NaturalPersonsRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IPhoneNumbersRepository, PhoneNumbersRepository>();
builder.Services.AddScoped<IRelationsRepository, RelationsRepository>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateNaturalPersonRequestValidator>();

builder.Services.AddScoped<ValidateModelAttribute>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<Migrator>();

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration).WriteTo.Console();
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<IStringLocalizer>(sp => {
    var factory = sp.GetRequiredService<IStringLocalizerFactory>();
    return factory.Create("Resources", typeof(Program).Assembly.FullName);
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { 
        new CultureInfo("en-US"),
        new CultureInfo("ka-GE")
    };
   
    options.DefaultRequestCulture = new RequestCulture("ka-GE");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRequestLocalization();

var featureManager = app.Services.GetRequiredService<IFeatureManager>();

if (await featureManager.IsEnabledAsync("RunMigrationsAutomatically"))
{
    var migrator = app.Services.GetRequiredService<Migrator>();
    migrator.Migrate();
}

if (await featureManager.IsEnabledAsync("EnableSeeder"))
{
    var seeder = app.Services.GetRequiredService<Migrator>();
    seeder.Seed();
}

if(await featureManager.IsEnabledAsync("EnableSwagger"))
{
    app.UseSwagger();
    
    app.UseSwaggerUI(c =>
    {
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

app.Run();

public partial class Program { }