


using System.Security.Claims;
using System.Text.Json;
using Azure.Storage.Blobs;
using InMindLab7.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "My API", 
        Version = "v1" 
    });

    // 1) Defining the BearerAuth scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",  
        In = ParameterLocation.Header,
        Description = "Enter your JWT Bearer token in the format: Bearer {your token here}"
    });

    // 2) Making sure the scheme is globally required 
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
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<UniversityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/UniversityRealm";
        options.Audience = "account";
        options.RequireHttpsMetadata = false;

        // This is the critical part:
        // Basically the way my tokens are generated are as follows 
        // {
        //  .... other fields ....
        // realm_access{
        //      roles = []
        // }
        // So in other words, dotnet cannot read the roles directly, I need to tell it how to parse the JSON document
        // After parsing the JSON document I need to register the roles of the user as "Role claims"
        // Role claims is how we do authorization
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // 1) Grab the 'realm_access' claim from the JWT
                var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                if (!string.IsNullOrEmpty(realmAccess))
                {
                    // 2) realmAccess is JSON like: {"roles":["Teacher","Student","offline_access","uma_authorization"]}
                    var doc = JsonDocument.Parse(realmAccess);

                    if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                    {
                        var appIdentity = new ClaimsIdentity();

                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            // 3) C# reads roles as "Role claims" so I have to register each one as a claim
                            appIdentity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                        }

                        // 4) Attach these new claims to the current principal (the current user)
                        context.Principal?.AddIdentity(appIdentity);
                    }
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("student"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("teacher"));
});

// Azurite Setup
string azuriteConnectionString = builder.Configuration["AzureStorage:ConnectionString"];

builder.Services.AddSingleton(new BlobServiceClient(azuriteConnectionString));




var app = builder.Build();
app.UseStaticFiles();


app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "/files"
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();