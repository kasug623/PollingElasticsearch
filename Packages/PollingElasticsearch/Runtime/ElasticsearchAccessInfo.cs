namespace kasug623.Elasticsearch.Polling
{
    public sealed class AccessInfo
    {
        public string User { get; private set; }
        public string Password { get; private set; }
        public string IpAddress { get; private set; }
        public string Index { get; private set; }
        public string Url { get; private set; }
        public int PollingIntervalMiliSec { get; private set; }

        public string CurrentBeginTime { get; private set; }
        public string ShiftSpanTime { get; private set; }
        public int ReceiveQueueSize { get; private set; }

        public AccessInfo(Å@string user,
                            string password,
                            string ipAddress,
                            string index,
                            int pollingIntervalSec,
                            string currentBeginTime,
                            string shiftSpanTime,
                            int receiveQueueSize)
        {
            User = user;
            Password = password;
            IpAddress = ipAddress;
            Index = index;
            Url = "http://" + user + ":" + password + "@" + ipAddress + ":9200/" + index + "/_search";

            PollingIntervalMiliSec = pollingIntervalSec * 1000;

            CurrentBeginTime = currentBeginTime;
            ShiftSpanTime = shiftSpanTime;
            ReceiveQueueSize = receiveQueueSize;
        }

    }

}