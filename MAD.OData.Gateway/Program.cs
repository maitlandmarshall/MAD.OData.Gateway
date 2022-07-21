using MAD.OData.Gateway;
using MAD.OData.Gateway.DynamicDbContext;
using MAD.OData.Gateway.Middlewares;
using MAD.OData.Gateway.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", true);

var dbFactory = new DynamicDbContextFactory();
var edmModel = new EdmModelFactory(dbFactory, builder.Configuration.GetConnectionString("odata")).Create();

builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection("authentication"));
builder.Services.AddTransient<AuthConfig>(cfg => cfg.GetRequiredService<IOptions<AuthConfig>>().Value);

// Add services to the container.
builder.Services.AddControllers().AddOData(opt => opt.AddRouteComponents(edmModel).EnableQueryFeatures());
builder.Services.AddSingleton(edmModel);
builder.Services.AddTransient<IApplicationModelProvider, ODataApplicationModelProvider>();
builder.Services.AddTransient<BasicAuthenticationMiddleware>();
builder.Services.AddSingleton<DynamicDbContextFactory>(dbFactory);
builder.Services.AddScoped<DbContext>(services =>
{
    var dynamicDbContextFactory = services.GetRequiredService<DynamicDbContextFactory>();
    var dbContext = dynamicDbContextFactory.CreateDbContext(builder.Configuration.GetConnectionString("odata"));

    dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));

    return dbContext;
});

var app = builder.Build();

#if DEBUG
app.UseODataRouteDebug();
#endif
app.UseODataQueryRequest();

app.UseMiddleware<BasicAuthenticationMiddleware>();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.Run();