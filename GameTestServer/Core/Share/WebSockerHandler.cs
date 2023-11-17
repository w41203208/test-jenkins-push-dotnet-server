using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Wanin_Test.Dto.Websocket;

namespace Wanin_Test.Core.Share
{
    public class WebSockerHandler
    {
        private ConcurrentDictionary<string, WebSocket> _webscokets = new ConcurrentDictionary<string, WebSocket>();

        public WebSocket GetWebsocket(string userId)
        {
            return _webscokets.FirstOrDefault(w => w.Key == userId).Value;
        }
        public bool UpdateWebsocket(string userId, WebSocket newOne, WebSocket oldOne)
        {
            if (_webscokets.TryUpdate(userId, newOne, oldOne))
            {
                Console.WriteLine($"Socket update Successly: {userId}.");
                return true;
            }
            else
            {
                Console.WriteLine($"Socket update Fail: {userId}.");
                return false;
            }
        }

        public bool AddWebsocket(WebSocket websocket, string userId)
        {
            var success = _webscokets.TryAdd(userId, websocket);
            if (success)
            {
                Console.WriteLine($"Socket add Successly: {userId}.");
                return true;
            }
            else
            {
                Console.WriteLine($"Socket add Fail: {userId}.");
                return false;
            }
        }

        public void RemoveWebsocket(string userId)
        {
            bool success = _webscokets.TryRemove(userId, out _);
            if (success)
            {
                Console.WriteLine($"Socket remove Successly: {userId}.");
            }
            else
            {
                Console.WriteLine("Websocket: " + userId + " isn't exsit.");
            }
        }

        public void Send<T>(WebsocketSendData<T> sendData, string id)
        {
            var json = sendData.ConvertToJson();
            ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            Send(buffer, id);
        }

        public void Send(ArraySegment<byte> buffer, string id)
        {
            try
            {
                WebSocket? w;
                if (_webscokets.TryGetValue(id, out w))
                {
                    if (w.State != WebSocketState.Open)
                    {
                        w.Dispose();
                        RemoveWebsocket(id);
                    }
                    else
                    {
                        if (!w.CloseStatus.HasValue)
                        {
                            w.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None); ;
                        }
                    }
                   
                }
            }catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void BroadCast<T>(WebsocketSendData<T> sendData, string? excludeUserId = null)
        {
            var json = sendData.ConvertToJson();
            ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            BroadCast(buffer, excludeUserId);
       }

        private void BroadCast(ArraySegment<byte> buffer, string? id)
        { 
            Parallel.ForEach(_webscokets, async (KeyValuePair<string, WebSocket> websocket) =>
            {
                try
                {
                    if (websocket.Key != id)
                    {
                        Console.WriteLine($"{websocket.Key}: {websocket.Value.State}");
                        if (websocket.Value.State != WebSocketState.Open)
                        {
                            websocket.Value.Dispose();
                            RemoveWebsocket(websocket.Key);
                        }
                        else
                        {
                            if (!websocket.Value.CloseStatus.HasValue)
                            {
                                await websocket.Value.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });   
        }

    }
}
