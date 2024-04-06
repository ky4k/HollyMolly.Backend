var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddPolicy(name: "FreeCorsPolicy", cfg =>
{
    cfg.AllowAnyHeader();
    cfg.AllowAnyMethod();
    cfg.WithOrigins("*");
}));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FreeCorsPolicy");
app.UseHttpsRedirection();

app.MapGet("/api/test", () => new { Succeded = true });

app.UseAuthorization();

app.MapControllers();

app.Run();
