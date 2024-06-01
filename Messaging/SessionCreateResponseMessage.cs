using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class SessionCreateResponseMessage
    {
        public string type = "session-create-response";
        public string Uuid { get; set; }
    }
}
