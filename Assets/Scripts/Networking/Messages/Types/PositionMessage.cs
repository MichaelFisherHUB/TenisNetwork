using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNetworking.Messages
{
    [System.Serializable]
    public class PositionMessage : BaseMessage
    {
        public NetworkingMode id;
        public float xPosition, yPosition, zPosition;

        public PositionMessage(NetworkingMode id, float x, float y, float z)
        {
            this.id = id;
            xPosition = x;
            yPosition = y;
            zPosition = z;
        }
    }
}
