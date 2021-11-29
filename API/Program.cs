using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder ( args );

builder.Services.AddApplicationServices ( builder.Configuration );
builder.Services.AddControllers ();
builder.Services.AddCors ();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen ();

// ---- Configure the HTTP request pipeline. ---- //
var app = builder.Build ();

if ( app.Environment.IsDevelopment () )
{
  //app.UseDeveloperExceptionPage();
  app.UseMiddleware<ExceptionMiddleware> (); // custom middleware
  app.UseSwagger ();
  app.UseSwaggerUI ();
}

//app.UseHttpsRedirection();
//app.UseRouting();

app.UseCors ( x => x.AllowAnyHeader ()
      .AllowAnyMethod ()
      .AllowCredentials () // allow cookies to be passed to client domain
      .WithOrigins ( "http://localhost:3000" ) );

app.UseAuthorization ();

app.MapControllers ();
//app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope ();
var services = scope.ServiceProvider;
try
{
  var context = services.GetRequiredService<StoreContext> ();
  await context.Database.MigrateAsync ();
  DbInitializer.Initialize ( context );
}
catch ( Exception ex )
{
  var logger = services.GetRequiredService<ILogger<Program>> ();
  logger.LogError ( ex, "An error occurred during migration" );
}

await app.RunAsync ();
