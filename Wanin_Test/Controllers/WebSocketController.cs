using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using Wanin_Test.Core.Share;
using Wanin_Test.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Wanin_Test.Controllers
{
    public class WebSocketControler : ControllerBase
    {
        private readonly WebsocketManager _wManager;
        private readonly SRSService _srsService;
        private readonly PublishListManager _pi;

        public WebSocketControler(WebsocketManager wm, SRSService ss, PublishListManager pi)
        {
            _srsService = ss;
            _wManager = wm;
            _pi = pi;
        }

        [HttpGet("")]
        public async Task Get(string userId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _wManager.AddWebsocket(webSocket, userId);
                await HandleReceive(webSocket, userId);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        private async Task HandleReceive(WebSocket webSocket, string id)
        {
            var buffer = new byte[1024 * 4];
            var res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            while (!res.CloseStatus.HasValue)
            {
                var cmd = Encoding.UTF8.GetString(buffer, 0, res.Count);

                // It doesnt handle receive data ( cmd )
                res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(res.CloseStatus.Value, res.CloseStatusDescription, CancellationToken.None);
            _wManager.RemoveWebsocket(id);

            if (_pi.CheckPublisherHas(id)) {
                // Delete stream url which srs server provide by id
                _srsService.DeleteUrl(id);

                // Delete local publishList record by id
                _pi.RemovePublishList(id);
            }
            
            Console.WriteLine($"{id} close");
        }

    }
}
