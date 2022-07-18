using MAD.OData.Gateway.DynamicDbContext;
using MAD.OData.Gateway.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", true);

var dbFactory = new DynamicDbContextFactory();
var edmModel = await new EdmModelFactory(dbFactory, builder.Configuration.GetConnectionString("odata")).Create();

// Add services to the container.
builder.Services.AddControllers().AddOData(opt => opt.AddRouteComponents(edmModel).EnableQueryFeatures(1000));
builder.Services.AddSingleton(edmModel);
builder.Services.AddTransient<IApplicationModelProvider, ODataApplicationModelProvider>();
builder.Services.AddTransient<SqlKataFactory>();
builder.Services.AddSingleton<DynamicDbContextFactory>(dbFactory);
builder.Services.AddScoped<DbContext>(services =>
{
    var dynamicDbContextFactory = services.GetRequiredService<DynamicDbContextFactory>();
    return dynamicDbContextFactory.CreateDbContext(builder.Configuration.GetConnectionString("odata"));
});

var app = builder.Build();

#if DEBUG
app.UseODataRouteDebug();
#endif

app.UseODataQueryRequest();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.Run();