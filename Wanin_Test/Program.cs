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
builder.Services.AddSingleton<WebsocketManager>();


builder.Services.AddSingleton<SRSService>();

// [This solution is not regular method¡Ait skip sslcheck to avoid to happen ssl problem]
//ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
//ServicePointManager.ServerCertificateValidationCallback +=
//    new RemoteCertificateValidationCallback(ValidateServerCertificate);

static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
{
    return true;
}

string? httpClientNameForSRS = builder.Configuration["SRSHttpClientName"];
string? httpClientPortForSRS = builder.Configuration["SRSHttpClientPort"];
string? httpClientHostForSRS = builder.Configuration["SRSHttpClientHost"];
ArgumentNullException.ThrowIfNull(httpClientNameForSRS);

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
        var AllowedHosts = builder.Configuration.GetSection("AllowedHosts").Value;
        Console.WriteLine("CORS allowedhosts¡G", AllowedHosts);

        policy.WithOrigins(AllowedHosts).AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(myAllowSpecificOrigins);

app.Use(async (context, next) =>
{
    //Console.WriteLine(context.Request.Headers.Host);
    await next.Invoke();
});
// Add a middleware to display author's name at response headers.
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Developed-By", "MingZhe");
    await next.Invoke();
});

app.MapControllers();

app.Run();
