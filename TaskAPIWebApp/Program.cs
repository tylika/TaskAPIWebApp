using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaskAPIWebApp;
using Microsoft.OpenApi.Models; // Потрібно для Swagger

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<TaskManagementApiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Початок налаштувань Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskSystem API",
        Version = "v1",
        Description = "API для системи управління завданнями TaskSystem"
        
    });

    
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // --- Підключення Swagger Middleware ---
    app.UseSwagger(); // Генерує swagger.json
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskSystem API v1");
        // c.RoutePrefix = string.Empty; // Якщо Swagger UI має бути на кореневому шляху
    });
    // --- Кінець Swagger Middleware ---
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Розкоментуйте, якщо використовуєте HTTPS
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/index.html");
    await System.Threading.Tasks.Task.CompletedTask;
});

app.Run();