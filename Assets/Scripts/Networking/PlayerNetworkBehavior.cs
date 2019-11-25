using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomNetworking;
using CustomNetworking.Messages;

public class PlayerNetworkBehavior : CustomNetworkBehavior
{
    [SerializeField] public float speed;

    public override void OnNetworkUpdate<T>(T posMessage)
    {
        if (id == Networking.NetMode)
        {
            return;
        }
        
        transform.position = new Vector3(
            posMessage.xPosition,
            posMessage.yPosition,
            posMessage.zPosition);
    }

    private void Update()
    {
        if (id != Networking.NetMode)
        {
            return;
        }

        float inputValue = Input.GetAxisRaw("Horizontal");

        if(inputValue != 0)
        {
            transform.Translate(
                speed * inputValue * Time.deltaTime,
                0,
                0
                );

            Networking.SendMessage(new PositionMessage(
                id,
                transform.position.x,
                transform.position.y,
                transform.position.z
                ));
        }
    }

    private void Start()
    {
        if (!NetworkingController.instance.players.Contains(this))
        {
            NetworkingController.instance.players.Add(this);
        }
    }
}