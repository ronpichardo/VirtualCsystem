namespace VirtualControlBeta.Config
{
    public class ConfigSettings
    {
        private string _roomId;
        private string _roomName;
        private string _mtrIp;
        private string _shureIp;

        public string RoomId
        {
            get
            {
                return _roomId;
            }
            set
            {
                _roomId = value;
            }
        }
        public string RoomName
        {
            get
            {
                return _roomName;
            }
            set
            {
                _roomName = value;
            }
        }
        public string MtrIp
        {
            get
            {
                return _mtrIp;
            }
            set
            {
                _mtrIp = value;
            }
        }
        public string ShureIp
        {
            get
            {
                return _shureIp;
            }
            set
            {
                _shureIp = value;
            }
        }
    }
}
