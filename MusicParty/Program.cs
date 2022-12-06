using MusicParty;
using MusicParty.Hub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton(new NeteaseApi(
    builder.Configuration["NeteaseApiUrl"],
    builder.Configuration["NeteaseId"],
    builder.Configuration["NeteasePassword"]
));
builder.Services.AddCors(op =>
{
    op.AddPolicy("cors", p =>
        p.WithOrigins("http://43.138.39.172").AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors("cors");

app.UseAuthorization();

app.MapControllers();

app.MapHub<MusicHub>("/music");

app.Run();