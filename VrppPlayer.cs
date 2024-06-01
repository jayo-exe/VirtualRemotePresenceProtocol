using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol
{
    //represents a User's relevant information while in a Session
    class VrppPlayer
    {
        public string Uuid { get; set; }
        public VrppUser User { get; set; }
        public List<string> BoneNames { get; set; }
        public List<string> BlendshapeNames { get; set; }
        public Dictionary<string, string> AnimationParameters { get; set; }

        public Dictionary<string,Common.VrppTransformPosition> BonePositions { get; set; }
        public Dictionary<string, Common.VrppTransformRotation> BoneRotations { get; set; }
        public Dictionary<string, float> BlendshapeValues { get; set; }
        public Dictionary<string, float> AnimationParameterValues { get; set; }

    }
}
