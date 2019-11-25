using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomNetworking.Messages;
public abstract class CustomNetworkBehavior : MonoBehaviour
{
    public NetworkingMode id;

    public abstract void OnNetworkUpdate<T>(T posMessage) where T : PositionMessage;
}
