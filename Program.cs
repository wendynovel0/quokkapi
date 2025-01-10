using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Importa el espacio de nombres necesario para Swagger
using System.Text;
using Quokka.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));

// Registrar MongoDB en el contenedor de dependencias
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>();
    var client = new MongoClient(settings.ConnectionString);
    return client;
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("Quokka"); // Especifica tu nombre de base de datos aquí
});

builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<JwtService>();

// Configuración de la autenticación JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Solo en desarrollo, en producción usa HTTPS
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"], // Usando builder.Configuration
            ValidAudience = builder.Configuration["JwtSettings:Audience"], // Usando builder.Configuration
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Agregar eventos para verificar el token en cada solicitud
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Error de autenticación: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var token = context.Request.Headers["Authorization"];
                Console.WriteLine($"Token recibido: {token}");
                return Task.CompletedTask;
            }
        };
    });

// Añadir controladores y configurar autorización global
builder.Services.AddControllers(options =>
{
    // Esto asegura que todas las rutas requieran autenticación por defecto
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger con autenticación JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Configuración de middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quokka API v1");
    c.RoutePrefix = "swagger";  // Deja la ruta como estaba para acceder a Swagger UI en /swagger
});

// Usa autenticación y autorización
app.UseAuthentication(); // Asegúrate de usar Authentication antes de Authorization
app.UseAuthorization(); // Autoriza solo a los usuarios autenticados

app.MapControllers();

app.Run();
