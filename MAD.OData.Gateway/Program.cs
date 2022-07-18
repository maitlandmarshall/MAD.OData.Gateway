using MAD.OData.Gateway.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;

var builder = WebApplication.CreateBuilder(args);
var edmModel = await new EdmModelFactory(builder.Configuration.GetConnectionString("odata")).Create();

// Add services to the container.
builder.Services.AddControllers().AddOData(opt => opt.AddRouteComponents(edmModel).Filter().Select());
builder.Services.AddSingleton<MatcherPolicy, ODataMatcherPolicy>();
builder.Services.AddSingleton(edmModel);
builder.Services.AddTransient<IApplicationModelProvider, ODataApplicationModelProvider>();
builder.Services.AddTransient<SqlKataFactory>();

var app = builder.Build();

#if DEBUG
app.UseODataRouteDebug();
#endif

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.Run();