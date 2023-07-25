using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Wanin_Test.Core.Share;
using Wanin_Test.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Custom Share Service
builder.Services.AddSingleton<PublishListManager>();
builder.Services.AddSingleton<WebSockerHandler>();
builder.Services.AddSingleton<SRSService>();


static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
{
    return true;
}


string? test = builder.Configuration["isDebug"];
string? httpClientNameForSRS = builder.Configuration["SRSHttpClientName"];
string? httpClientPortForSRS = builder.Configuration["SRSHttpClientPort"];
string? httpClientHostForSRS = builder.Configuration["SRSHttpClientHost"];
ArgumentNullException.ThrowIfNull(httpClientNameForSRS);


// Enroll HttpClient by IHttpClientFactory
// can use Polly to send data again
builder.Services.AddHttpClient<SRSService>(
    httpClientNameForSRS, 
    client =>
    {
        client.BaseAddress = new Uri($"https://{httpClientHostForSRS}:{httpClientPortForSRS}");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Test-Game-Service#dotnet");
        
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = ValidateServerCertificate
    });



// Enable CORS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(o =>
{
    o.AddPolicy(name: myAllowSpecificOrigins, policy =>
    {
        var AllowedHosts = builder.Configuration["AllowedHosts"];
        Console.WriteLine($"CORS allowedhosts¡G {AllowedHosts}");

        policy.WithOrigins(AllowedHosts).AllowAnyMethod().AllowAnyHeader();
    });
});

var HTTPS_PORT = builder.Configuration["SRS_HTTPS_PORT"];
var HTTP_PORT = builder.Configuration["SRS_HTTP_PORT"];
builder.WebHost.UseKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(10501, listenOptions =>
    {
        listenOptions.UseHttps("../testcert.pfx",
                "test");
    });
    serverOptions.ListenAnyIP(10500);
});




builder.Services.AddApplicationInsightsTelemetry();




var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// start websocket
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(myAllowSpecificOrigins);

// Add a middleware to display author's name at response headers.
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Developed-By", "MingZhe");
    await next.Invoke();
});


app.MapControllers();

app.Run();
