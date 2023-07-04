using Wanin_Test.Util;

namespace Wanin_Test.Dto.Websocket
{
    interface IWebsocketSendData
    {
        public string ConvertToJson();
    }
    public class WebsocketSendData<T>: IWebsocketSendData
    {
        public string SendMethodType { get; set; }

        public T Data { get; set; }

        public WebsocketSendData(T data, string methodType)
        {
            SendMethodType = methodType;
            Data = data;
        }

        public string ConvertToJson()
        {
            string sendDataJson = JsonSerializerExteions.SerializeWithCamelCase(new
            {
                Data,
                MethodName = SendMethodType
            });
            return sendDataJson;
        }
    }
}
