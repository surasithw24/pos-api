using Supabase;

// 1. ปิด FileSystemWatcher ในระดับ Environment ป้องกันการ Crash บน Linux Docker
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// 2. เคลียร์ Configuration และโหลดแบบปิด reloadOnChange
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 3. ตั้งค่า CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 4. ลงทะเบียน Supabase Client
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

// 5. เปิดใช้งาน Middleware
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
