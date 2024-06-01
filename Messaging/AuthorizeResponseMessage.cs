using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class AuthorizeResponseMessage
    {
        public string type = "authorize-response";
        public string AccessToken { get; set; }
    }
}
