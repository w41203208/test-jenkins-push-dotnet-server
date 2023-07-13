using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Wanin_Test.Dto.Websocket;

namespace Wanin_Test.Core.Share
{
    public class WebsocketManager
    {
        private ConcurrentDictionary<string, WebSocket> _webscokets = new ConcurrentDictionary<string, WebSocket>();

        public void AddWebsocket(WebSocket websocket, string userId)
        {
            var success = _webscokets.TryAdd(userId, websocket);
            if (success)
            {
                Console.WriteLine($"Socket add Successly: {userId}.");
            }
            else
            {
                Console.WriteLine($"Socket add Fail: {userId}.");
            }
        }

        public void RemoveWebsocket(string userId)
        {
            bool success = _webscokets.TryRemove(userId, out _);
            if (success)
            {
                if (_webscokets.TryGetValue(userId, out _))
                {
                    Console.WriteLine("Dont Remove " + userId + " websocket.");
                }
            }
            else
            {
                Console.WriteLine("Websocket: " + userId + " isn't exsit.");
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
                if (websocket.Key != id)
                {
                    await websocket.Value.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            });
        }

    }
}
