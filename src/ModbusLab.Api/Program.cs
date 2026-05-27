using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;
using ModbusLab.Api.Audit;
using ModbusLab.Api.Auth;
using ModbusLab.Api.BackgroundServices;
using ModbusLab.Api.Devices;
using ModbusLab.Api.Endpoints;
using ModbusLab.Api.Realtime;
using ModbusLab.Application.Devices;
using ModbusLab.Application.Modbus;
using ModbusLab.Application.Testing;
using ModbusLab.Infrastructure;
using ModbusLab.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});
builder.Services.AddSignalR();
builder.Services.AddHostedService<RandomRegisterSimulationWorker>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            remoteIp,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtSecret = builder.Configuration["Jwt:Secret"];

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience) ||
    string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT settings are not configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) &&
                    path.StartsWithSegments("/hubs/modbus"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.RequireViewer, policy =>
        policy.RequireRole(AuthRoles.Viewer, AuthRoles.Engineer, AuthRoles.Admin));
    options.AddPolicy(AuthPolicies.RequireEngineer, policy =>
        policy.RequireRole(AuthRoles.Engineer, AuthRoles.Admin));
    options.AddPolicy(AuthPolicies.RequireAdmin, policy =>
        policy.RequireRole(AuthRoles.Admin));
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<DeviceManagementService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<DeviceQueryService>();
builder.Services.AddScoped<ModbusRegisterService>();
builder.Services.AddScoped<TestProfileService>();
builder.Services.AddScoped<TestExecutionService>();

var app = builder.Build();

await DatabaseSeeder.SeedAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapUserManagementEndpoints();
app.MapAuditLogEndpoints();
app.MapDeviceEndpoints();
app.MapDeviceManagementEndpoints();
app.MapModbusEndpoints();
app.MapTestProfileEndpoints();
app.MapTestRunEndpoints();

app.MapHub<ModbusHub>("/hubs/modbus")
    .RequireAuthorization();

app.Run();
