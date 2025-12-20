using System.Net.Http.Headers;
using LmsMini.Domain.Models;
using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting; // Cần cho ConfigureKestrel
using Microsoft.AspNetCore.Http.Features; // Cần cho FormOptions
using Microsoft.EntityFrameworkCore;
using LmsMini.Application.DTOs.Classroom;
using Microsoft.Extensions.DependencyInjection;
using LmsMini.WebApp.Services; // Nơi chứa InMemoryTicketStore
using Microsoft.Extensions.Caching.Memory; // Cần cho MemoryCache

var builder = WebApplication.CreateBuilder(args);

// === 1. FIX LỖI HTTP 400 (Cookie/Header quá lớn) - CẤP ĐỘ SERVER ===
// Tăng giới hạn Header lên 128KB để server không từ chối request có cookie lớn
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestHeadersTotalSize = 128 * 1024;
});

// --- 2. CẤU HÌNH BASE URL ---
var apiBaseFromConfig = builder.Configuration["ApiBaseUrl"];
var ngrokEnv = Environment.GetEnvironmentVariable("NGROK_URL");
var apiBase = !string.IsNullOrWhiteSpace(apiBaseFromConfig) ? apiBaseFromConfig :
        !string.IsNullOrWhiteSpace(ngrokEnv) ? ngrokEnv :
        "https://localhost:7112/"; // Fallback URL

if (!apiBase.EndsWith("/")) apiBase += "/";

// --- 3. ĐĂNG KÝ SERVICES ---

builder.Services.AddRazorPages();

// === FIX LỖI HTTP 400 KHI UPLOAD FILE ===
// Tăng giới hạn upload file (Multipart Body) lên 200MB
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200L * 1024 * 1024;
});

// === FIX LỖI COOKIE QUÁ LỚN - CẤP ĐỘ ỨNG DỤNG ===
// Sử dụng MemoryCache để lưu thông tin User (Claims) trên RAM Server thay vì nhét hết vào Cookie
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITicketStore, InMemoryTicketStore>();

// Cấu hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.LoginPath = "/Account/Login";
      options.AccessDeniedPath = "/Account/AccessDenied";
      options.ExpireTimeSpan = TimeSpan.FromHours(8);
      options.SlidingExpiration = true;

      // Đổi tên Cookie để tránh xung đột với cookie cũ bị lỗi
      options.Cookie.Name = "LmsApp_Session_V3";

      // Kích hoạt Session Store (Lưu session vào RAM)
      var serviceProvider = builder.Services.BuildServiceProvider();
      options.SessionStore = serviceProvider.GetRequiredService<ITicketStore>();
  });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthenticatedHttpMessageHandler>();

// --- 4. ĐĂNG KÝ HTTP CLIENTS VÀ API CLIENTS ---

builder.Services.AddHttpClient("LmsApi", client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(60);
})
.AddHttpMessageHandler<AuthenticatedHttpMessageHandler>();

// Đăng ký các Interface ApiClient
builder.Services.AddScoped<IStudentApiClient, StudentApiClient>();
builder.Services.AddScoped<IClassroomApiClient, ClassroomApiClient>();
builder.Services.AddScoped<IProjectApiClient, ProjectApiClient>();
builder.Services.AddScoped<ICourseApiClient, CourseApiClient>();
builder.Services.AddScoped<IAssignmentApiClient, AssignmentApiClient>();
builder.Services.AddScoped<IProjectClassroomApiClient, ProjectClassroomApiClient>();
builder.Services.AddScoped<ISubmissionApiClient, SubmissionApiClient>();


// --- 5. BUILD VÀ PIPELINE ---
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Redirect root logic
app.MapGet("/", context =>
{
    // Nếu đã đăng nhập, chuyển vào Dashboard tương ứng (tuỳ chọn), nếu chưa thì về Login
    if (context.User.Identity?.IsAuthenticated == true)
    {
        if (context.User.IsInRole("Student"))
            context.Response.Redirect("/Student/Classrooms");
        else
            context.Response.Redirect("/Account/Login"); // Hoặc dashboard khác
    }
    else
    {
        context.Response.Redirect("/Account/Login");
    }
    return Task.CompletedTask;
});

app.Run();