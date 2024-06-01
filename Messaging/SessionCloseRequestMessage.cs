using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class SessionCloseRequestMessage
    {
        public string type = "session-close-request";
        public string AccessToken { get; set; }
        public string SessionUuid { get; set; }

    }
}
