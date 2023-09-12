using Microsoft.AspNetCore.Authentication.Certificate;
using System.Net.Security;
using System.Security.Authentication;
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


// 這個跟httpClientHandler真的有關
builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme).AddCertificate();


string? is_deg = builder.Configuration["isDebug"];
ArgumentNullException.ThrowIfNull(is_deg);
Console.WriteLine($"DegMode: {is_deg}");
string? httpClientNameForSRS = builder.Configuration["SRSHttpClientName"];
ArgumentNullException.ThrowIfNull(httpClientNameForSRS);
string? httpClientPortForSRS = builder.Configuration["SRSHttpClientPort"];
ArgumentNullException.ThrowIfNull(httpClientPortForSRS);
string? httpClientHostForSRS = builder.Configuration["SRSHttpClientHost"];
ArgumentNullException.ThrowIfNull(httpClientNameForSRS);


var clientCertFile = Path.Combine(builder.Environment.ContentRootPath, "testcert.pfx");
var clientCertificate = new X509Certificate2(clientCertFile, "test", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);


var serverCertFile = Path.Combine(builder.Environment.ContentRootPath, "cert.pem");
var serverCertificate = new X509Certificate2(serverCertFile);
clientCertificate.GetRawCertDataString();

// this callback is used to verify server cert 
bool ValidateServerCertificate(HttpRequestMessage requestMessage, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
{
    if (sslPolicyErrors == SslPolicyErrors.None)
        return true;

    if (chain != null && chain.ChainElements.Count > 0)
    {
        X509Certificate2 root = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
        if (root.RawData.SequenceEqual(serverCertificate.RawData))
            return true;
    }


    return false;
}



// can use Polly to send data again
builder.Services.AddHttpClient<SRSService>(
    httpClientNameForSRS, 
    client =>
    {
        string url;
        url = $"https://{httpClientHostForSRS}:{httpClientPortForSRS}";
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Test-Game-Service#dotnet");
        
    }).ConfigurePrimaryHttpMessageHandler(() => 
    {
        HttpClientHandler handler;
        handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.None,
            ClientCertificateOptions = ClientCertificateOption.Manual,
            // set how to verify server cert 
            ServerCertificateCustomValidationCallback = ValidateServerCertificate
        };
        var clientCertificates = new X509Certificate2Collection { clientCertificate };
        handler.ClientCertificates.AddRange(clientCertificates);
        return handler;
    });

// Enable CORS
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(o =>
{
    o.AddPolicy(name: myAllowSpecificOrigins, policy =>
    {
        var AllowedHosts = builder.Configuration["AllowedHosts"];
        Console.WriteLine($"CORS allowedhosts： {AllowedHosts}");

        policy.WithOrigins(AllowedHosts).AllowAnyMethod().AllowAnyHeader();
    }); 
});

var HTTPS_PORT = builder.Configuration["SRS_HTTPS_PORT"];
var HTTP_PORT = builder.Configuration["SRS_HTTP_PORT"];
builder.WebHost.UseKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(10501, listenOptions =>
    {
        listenOptions.UseHttps("./testcert.pfx",
                "test");
    });
    serverOptions.ListenAnyIP(10500);
});

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("start swagger");
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
app.UseAuthentication();
app.UseCors(myAllowSpecificOrigins);

// Add a middleware to display author's name at response headers.
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Developed-By", "MingZhe");
    await next.Invoke();
});


app.MapControllers();

app.Run();
