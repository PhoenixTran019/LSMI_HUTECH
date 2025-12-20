// wire-up serilog, swagger, automapper, mediatR
using FluentValidation;
using FluentValidation.AspNetCore;
using LmsMini.Api.Swagger;
using LmsMini.Application.Auth;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using LmsMini.Infrastructure.Services;
using LmsMini.Infrastructure.Services.Classrooms;
using LmsMini.Infrastructure.Services.Project;
using LmsMini.Infrastructure.Services.Projects;
using LmsMini.Infrastructure.Services.Students;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// ======================================================================
// 1. Khởi tạo Serilog (chạy trước khi tạo `builder` để bắt log khởi tạo)
// ======================================================================
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNgrok", p => p
        .SetIsOriginAllowed(origin =>
            origin == "https://aryan-hypaesthesic-answerably.ngrok-free.dev"
            || origin.StartsWith("http://localhost:")
            || origin.StartsWith("https://localhost:")
            || origin.StartsWith("http://127.0.0.1:")
            || origin.StartsWith("https://127.0.0.1:")
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRazorDev", policy =>
    {
        policy.WithOrigins("https://localhost:7251")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// ======================================================================
// 2. Thay thế logger mặc định của Host bằng Serilog
// ======================================================================
builder.Host.UseSerilog();

// ======================================================================
// 3. Cấu hình dịch vụ chung (Service registrations)
//    3.1 Đăng ký DbContext (kết nối CSDL)
// ======================================================================
builder.Services.AddDbContext<LmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ======================================================================
// 3.2 Đăng ký Repository và các DI khác
// ======================================================================
builder.Services.AddHttpContextAccessor();


// ======================================================================
// 3.3 Đăng ký AutoMapper (quét tất cả assemblies hiện tại)
// ======================================================================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ======================================================================
// 3.4 Đăng ký MediatR (command/query handlers)
// ======================================================================
builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());

// ======================================================================
// 3.5 Đăng ký dịch vụ JWT
// ======================================================================

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IClassroomService, ClassroomService>();
builder.Services.AddScoped<IAssigmentService, AssignmentService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ProjectMajorDropdownSchemaFilter>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectClassroomService, ProjectClassroomService>();
builder.Services.AddScoped<ProjectFilterSchemaFilter>();
builder.Services.AddScoped<IProWeeklyService, ProWeeklyMeeting>();
builder.Services.AddScoped<ILecWeekReportService, LecWeekReportService>();
builder.Services.AddScoped<IDropHeplerService, DropHelperService>();
builder.Services.AddScoped<IStudentClassroomService, StudentClassroomService>();

// ======================================================================
// 3.6 Đăng ký FluentValidation
// ======================================================================
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var jwtOpts = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtOpts.Issuer,
            ValidAudience = jwtOpts.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOpts.Key)),

            ClockSkew =TimeSpan.Zero
        };
    });

// ======================================================================
// 3.7 Đăng ký Swagger/OpenAPI (cấu hình bảo mật, JWT nếu cần)
// ======================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LmsMini API", Version = "v1" });

    // Cấu hình JWT Bearer Auth trong Swagger (nếu cần)
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new()
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
            Array.Empty<string>()
        }
    });

    c.SchemaFilter<StudentDropdownSchemaFilter>();

    c.SchemaFilter<ProjectMajorDropdownSchemaFilter>();

    c.SchemaFilter<ProjectFilterSchemaFilter>();
});

// ======================================================================
// 3.8 Đăng ký Identity
//    Ghi chú: `AspNetUser` và `LmsDbContext` đã được cấu hình cho Identity.
// ======================================================================


var app = builder.Build();

// ======================================================================
// 4. Cấu hình middleware (the request pipeline)
//    4.1 Bật Serilog request logging để ghi log các request
// ======================================================================
app.UseSerilogRequestLogging();

// ======================================================================
// 4.2 Bật Swagger chỉ trong môi trường phát triển
// ======================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LmsMini API v1"));
    // ======================================================================

}
app.UseCors("AllowNgrok");
app.UseCors("AllowRazorDev");
app.UseCors("AllowAll");

// ======================================================================
// 4.3 Middleware chung: HTTPS, Authentication, Authorization, Controllers
// ======================================================================
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/uploads"
});
// ======================================================================
// 5. Chạy ứng dụng
// ======================================================================
app.Run();
