using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using Wanin_Test.Core.Share;
using Wanin_Test.Services;

namespace Wanin_Test.Controllers
{
    public class WebSocketController : ControllerBase
    {
        private readonly WebSockerHandler _wHandler;
        private readonly SRSService _srsService;
        private readonly PublishListManager _pi;
        private readonly int _receivePayloadBufferSize;



        public WebSocketController(WebSockerHandler wh, SRSService ss, PublishListManager pi)
        {
            _srsService = ss;
            _wHandler = wh;
            _pi = pi;

            _receivePayloadBufferSize = 1024;
        }

        [HttpGet("/ws")]
        public async Task Get([FromQuery]string userId)
        {
            
            try
            {
                var currentWebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (HttpContext.WebSockets.IsWebSocketRequest)
                {
                    bool addSuccess = _wHandler.AddWebsocket(currentWebSocket, userId);
                    if (addSuccess)
                    {
                        await HandleReceive(currentWebSocket, userId);
                        Console.WriteLine($"Is Closed {currentWebSocket.State}");
                        Console.WriteLine($"---------------------------------------");
                    }
                    else
                    {
                        // When another websocket of same userId
                        var oldWebsocket = _wHandler.GetWebsocket(userId);
                        if (oldWebsocket != null)
                        {
                            // close old websocket
                            if (oldWebsocket.State != WebSocketState.Closed)
                            {
                                await oldWebsocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                            }

                            // change websocket record data from old websocket to new websocket
                            if (currentWebSocket.State == WebSocketState.Open)
                            {
                                bool aupdate2Success = _wHandler.UpdateWebsocket(userId, currentWebSocket, oldWebsocket);
                                // if updating fail will close current websocket connection.
                                if (aupdate2Success)
                                {
                                    await HandleReceive(currentWebSocket, userId);
                                    Console.WriteLine($"Is Closed {currentWebSocket.State}");
                                    Console.WriteLine($"---------------------------------------");
                                }
                                else
                                {
                                    if (currentWebSocket.State != WebSocketState.Closed)
                                    {
                                        await currentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                                        _wHandler.RemoveWebsocket(userId);
                                    }
                                }
                            }
                        }
                        
                    }
                }
                else
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Happened in Get");
                Console.WriteLine(ex.Message);
            }
        }
        private async Task HandleReceive(WebSocket webSocket, string id)
        {
            try
            {
                WebSocketReceiveResult result;
                byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize * 10];
                
                while (webSocket.State == WebSocketState.Open)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                    
                    if (result.CloseStatus.HasValue)
                    {
                        Console.WriteLine(result.CloseStatus);
                        Console.WriteLine(webSocket.State);
                        if(webSocket.State == WebSocketState.CloseReceived)
                        {
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                            Console.WriteLine(webSocket.State);
                            _wHandler.RemoveWebsocket(id);
                            if (_pi.CheckPublisherHas(id))
                            {
                                // Delete local publishList record by id
                                _pi.RemovePublishList(id);
                            }
                            Console.WriteLine($"{id} close");
                        }
                    }
                    else
                    {
                        var cmd = Encoding.UTF8.GetString(receivePayloadBuffer, 0, result.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Happened in sending data and closed or closed receieved Error: ");
                Console.WriteLine(ex.Message);
            }
        }

    }
}
