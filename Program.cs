using Supabase;

// 1. ปิด FileSystemWatcher ป้องกัน Crash บน Linux
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// 2. บังคับให้ Kestrel รับส่งข้อมูลผ่านพอร์ต 8080 ของ 0.0.0.0 เพื่อให้ Render ตรวจพบ
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

// 3. เคลียร์ Configuration และโหลดแบบปิด reloadOnChange
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 4. ตั้งค่า CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 5. ลงทะเบียน Supabase Client
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

// 6. เปิดใช้งาน Middleware
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
