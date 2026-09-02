using Supabase;

var builder = WebApplication.CreateBuilder(args);

// 1. ตั้งค่า CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 2. ลงทะเบียน Supabase Client ใน DI Container
// ใช้วิธี Initialize ตอนที่เริ่มเรียกใช้งานตัวแรก เพื่อไม่ให้บล็อกการ Start ของ Web API
builder.Services.AddSingleton(sp =>
{
    var url = "https://axiswrwyscakeacrgrcy.supabase.co";
    var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImF4aXN3cnd5c2Nha2VhY3JncmN5Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODU2OTAzMDIsImV4cCI6MjEwMTI2NjMwMn0.sB3CrZrler-tgaYKeXM1yA-mXxmx4BWKvUwb8E54ezY";

    var options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };

    var client = new Supabase.Client(url, key, options);
    client.InitializeAsync().Wait(); // ดำเนินการกดล็อกเซสชันเบื้องต้น
    return client;
});

builder.Services.AddControllers();

var app = builder.Build();

// 3. เปิดใช้งาน Middleware ตามลำดับที่ถูกต้อง
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();