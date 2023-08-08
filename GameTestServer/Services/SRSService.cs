using GameTestServer.Dto.SRSController;
using System;
using System.Net.Http;
using System.Text.Json;
using Wanin_Test.Dto.SRSController;
using Wanin_Test.Util;

namespace Wanin_Test.Services
{
    public class SRSService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _conf;

        public SRSService(HttpClient httpClient, IConfiguration conf)
        {
            _httpClient = httpClient;
            _conf = conf;
        }

        public async Task<GetUrlResponse?> GetUrl(GetUrlPayload data) { 
            try
            {
                
                var res = await _httpClient.PostAsJsonAsync("api/get_url", new
                {
                    data.PullerId,
                    data.PublisherId,
                    Key = _conf["SECRET_KEY"]
                }, new JsonSerializerOptions());

                var content =  res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                // Creating a SerializerOptions object to replaced original object, due CamelCaseMode is used to DeserializeFromCamelCase method.
                var options = new JsonSerializerOptions();
                GetUrlResponse? result = JsonSerializerExteions.DeserializeFromCamelCase<GetUrlResponse>(content, options);

                if (result == null)
                {
                    throw new Exception("Don't get data from srs_manager's api which is getUrl");
                }

                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        public async void DeleteUrl(string userId)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync("api/delete_stream_and_url", new { userId });
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
