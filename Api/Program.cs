using Api.Services;
using Microsoft.EntityFrameworkCore;
using Api.Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using DAL;
using DAL.Entities;
using Api.Mapper;
using Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var authSection = builder.Configuration.GetSection(AuthConfig.Position);
var authConfig = authSection.Get<AuthConfig>();

builder.Services.Configure<AuthConfig>(authSection);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s=>
{
    s.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "������� ����� ������������",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    s.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,

                },
                Scheme =  "oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header, 
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<DAL.DataContext>(options=>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"), sql => { });
});

builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<PostService>();

builder.Services.AddTransient<AttachService>();

builder.Services.AddScoped<LikeService>();

builder.Services.AddScoped<LinkGeneratorService>();

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authConfig.Issuer,
        ValidateAudience = true,
        ValidAudience = authConfig.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = authConfig.SymmetricSecurityKey(),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("ValidAccessToken", p =>
    {
        p.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        p.RequireAuthenticatedUser();
    });
});

var app = builder.Build();

using (var serviceScope = ((IApplicationBuilder)app).ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
{
    if (serviceScope != null)
    {
        var context = serviceScope.ServiceProvider.GetRequiredService<DAL.DataContext>();
        context.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization();

app.UseTokenValidator();

app.UseGlobalErrorWrapper();

app.MapControllers();
app.Run();
