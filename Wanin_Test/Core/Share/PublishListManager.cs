using System.Collections.Concurrent;

namespace Wanin_Test.Core.Share
{
    public class PublishListManager
    {
        //private static object lockObject = new object();
        private readonly ConcurrentDictionary<string, bool> _publishList;

        public PublishListManager()
        {
            _publishList = new ConcurrentDictionary<string, bool>();
        }

        public void AddPublishList(string userId)
        {
            _publishList.TryAdd(userId, true);
        }

        public void RemovePublishList(string userId)
        {
            _publishList.TryRemove(userId, out _);
        }

        public bool CheckPublisherHas(string userId)
        {
            if(_publishList.TryGetValue(userId, out _) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public List<string> GetPublishList()
        {
            //lock (lockObject)
            //{
                List<string> publishList = new List<string>();
                foreach (var publish in _publishList.ToList())
                {
                    if (publish.Value == true)
                    {
                        publishList.Add(publish.Key);
                    }
                }
                return publishList;
            //}
        }
    }
}
