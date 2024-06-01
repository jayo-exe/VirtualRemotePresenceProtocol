using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class SessionCreateRequestMessage
    {
        public string type = "session-create-request";
        public string AccessToken { get; set; }
    }
}
