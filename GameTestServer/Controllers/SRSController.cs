using GameTestServer.Dto.SRSController;
using Microsoft.AspNetCore.Mvc;
using Wanin_Test.Core.Share;
using Wanin_Test.Dto.SRSController;
using Wanin_Test.Dto.Websocket;
using Wanin_Test.Services;

namespace Wanin_Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SRSController : ControllerBase
    {
        private readonly PublishListManager _pi;
        private readonly WebSockerHandler _wManager;
        private readonly SRSService _srsservice;
        private readonly IConfiguration _conf;

        public SRSController(PublishListManager pi, WebSockerHandler wm, SRSService ss, IConfiguration conf)
        {
            _srsservice = ss;
            _pi = pi;
            _wManager = wm;
            _conf = conf;
        }

        [HttpGet("test")]
        public ActionResult CanPublish()
        {
            return Ok("test");
        }

        [HttpPost("can_publish")]
        [Produces("application/json")]
        public ActionResult<CanPublishResponse> CanPublish(CanPublishPayload canPublishPayload)
        {
            string? userId = canPublishPayload.UserId;
            if (userId == null)
            {
                return BadRequest(new CanPublishResponse
                {
                    Msg = "Your request haven't userId!",
                });
            }

            // 將可以推流的 userId 放入 publishList
            _pi.AddPublishList(userId);

            // 建立一個 WebSocketSendData
            var sendData = new WebsocketSendData<UpdatePublishListData>(new UpdatePublishListData { PublishList = _pi.GetPublishList() }, "update_publishList");

            // 傳送更新後的 publishList 給 client
            _wManager.BroadCast(sendData, userId);

            return Ok(new CanPublishResponse
            {
                Msg = "OK",
            });
        }

        [HttpPost("cancel_can_publish")]
        [Produces("application/json")]
        public ActionResult<CancelCanPublishResponse> CancelCanPublish(CancelCanPublishPayload cancelCanPublishPayload)
        {
            string? userId = cancelCanPublishPayload.UserId;
            if (userId == null)
            {
                return BadRequest(new CancelCanPublishResponse
                {
                    Msg = "Your request haven't userId!",
                });
            }

            // 將可以推流的 userId 移出 publishList
            _pi.RemovePublishList(userId);

            // 建立一個 WebSocketSendData
            var sendData = new WebsocketSendData<UpdatePublishListData>(new UpdatePublishListData { PublishList = _pi.GetPublishList() }, "update_publishList");

            // 傳送更新後的 publishList 給 client
            _wManager.BroadCast(sendData, userId);

            return Ok(new CancelCanPublishResponse
            {
                Msg = "OK",
            });
        }

        [HttpGet("get_publishList")]
        public ActionResult<GetPublishListResponse> GetPublishList()
        {
            return Ok(new GetPublishListResponse
            {
                PublishList = _pi.GetPublishList(),
            });
        }

        [HttpPost("pull")]
        public async Task<ActionResult<PullResponse>> GetUrl(GetUrlPayload getUrlPayload)
        {
            if (getUrlPayload.PullerId == null)
            {
                return NotFound(new
                {
                    Msg = "Dont't get your PullerId. Please enter it."
                });
            }
            if (getUrlPayload.PublisherId == null)
            {
                return NotFound(new
                {
                    Msg = "Dont't get your PublisherId. Please enter it."
                });
            }
            if (getUrlPayload.UserId == null)
            {
                return NotFound(new
                {
                    Msg = "Dont't get your UserId. Please enter it."
                });
            }
            var data = await _srsservice.GetUrl(getUrlPayload);

            var srsManagerConnection = "https://" + _conf["SRSHttpClientHost"] + ":" + _conf["SRSHttpClientPort"];

            if (data == null)
            {
                return NotFound(new
                {
                    Msg = "Don't any url can use"
                });
            }
            else
            {
                if (data.PusherNeedToPush)
                {
                    var sendData = new WebsocketSendData<NotifyUrlData>(new NotifyUrlData { RTCUrl = data.RTCUrl, RTMPUrl = data.RTMPUrl, SRSManagerConnectionUrl = srsManagerConnection, Token = "" }, "notify_url");
                    _wManager.BroadCast(sendData, getUrlPayload.PullerId);
                }
            }

            return Ok(new PullResponse
            {
                RTCUrl = data.RTCUrl,
                RTMPUrl = data.RTMPUrl,
                SRSManagerConnectionUrl = srsManagerConnection,
                Token = ""
            });
        }
    }
}
