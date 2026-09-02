using Supabase;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// 1. ปิด reloadOnChange เพื่อแก้ปัญหา FileSystemWatcher Crash (Status 139) บน Linux Container
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 2. ตั้งค่า CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 3. ลงทะเบียน Supabase Client (ลบ .Wait() ออก ป้องกันปัญหา Thread Deadlock ตอน Startup)
builder.Services.AddSingleton(sp =>
{
    var url = "https://axiswrwyscakeacrgrcy.supabase.co";
    var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImF4aXN3cnd5c2Nha2VhY3JncmN5Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODU2OTAzMDIsImV4cCI6MjEwMTI2NjMwMn0.sB3CrZrler-tgaYKeXM1yA-mXxmx4BWKvUwb8E54ezY";

    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };

    return new Supabase.Client(url, key, options);
});

builder.Services.AddControllers();

var app = builder.Build();

// 4. เปิดใช้งาน Middleware
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
