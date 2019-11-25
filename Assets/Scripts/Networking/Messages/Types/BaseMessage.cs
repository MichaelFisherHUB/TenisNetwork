using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNetworking.Messages
{
    [System.Serializable]
    public class BaseMessage
    {
        public MessageType messageType;
    }
}