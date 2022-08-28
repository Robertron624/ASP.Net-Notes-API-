var builder  = WebApplication.CreateBuilder(args);

var  app = builder.Build();


app.MapGet("/", () => "Welcome to Notes API");

await app.RunAsync();