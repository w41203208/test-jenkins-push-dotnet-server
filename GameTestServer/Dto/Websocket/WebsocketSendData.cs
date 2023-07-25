using Wanin_Test.Util;

namespace Wanin_Test.Dto.Websocket
{
    public abstract class AWebsocketData<T>
    {
        public T Data { get; set; }
        public AWebsocketData(T data)
        {
            Data = data;
        }
        public abstract string ConvertToJson();
    }
    public class WebsocketSendData<T>: AWebsocketData<T>
    {
        public string SendMethodType { get; set; }

        public WebsocketSendData(T data, string methodType) : base(data)
        {
            SendMethodType = methodType;
        }

        public override string ConvertToJson()
        {
            string sendDataJson = JsonSerializerExteions.SerializeWithCamelCase(new
            {
                Data,
                MethodType = SendMethodType
            });
            return sendDataJson;
        }
    }
}
