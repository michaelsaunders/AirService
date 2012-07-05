using AirService.Model;

namespace AirService.Services.Notifications
{
    internal class NotificationData
    {
        private readonly byte[] _payload;
        private readonly NotificationToken _token;

        public NotificationData(NotificationToken token, byte[] payload)
        {
            this._token = token;
            this._payload = payload;
        }

        public byte[] Payload
        {
            get
            {
                return this._payload;
            }
        }

        public NotificationToken Token
        {
            get
            {
                return this._token;
            }
        }

        public bool Processed
        {
            get;
            set;
        }

        /// <summary>
        /// If this is set to true while pushing notification means
        /// the device or it's token is no longer valid (uninstalled etc)
        /// </summary>
        public bool IsTokenInvalid
        {
            get;
            set;
        }
    }
}