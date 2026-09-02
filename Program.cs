using Supabase;

// 1. สร้าง Builder แบบล้าง Config เก่าทั้งหมด (ป้องกัน FileSystemWatcher Crash)
var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// 2. โหลด Config โดยไม่ใช้ FileWatcher (reloadOnChange: false)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 3. ลงทะเบียน Services พื้นฐาน
builder.Services.AddRouting();
builder.Services.AddControllers();

// 4. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 5. Supabase Client Setup
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

var app = builder.Build();

// 6. Config Middleware & Routes
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
