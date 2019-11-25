using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomNetworking;
using CustomNetworking.Messages;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ThreadDispatcher))]
public class NetworkingController : MonoBehaviour
{
    [SerializeField] private string ipAdress = "192.168.1.101";
    [SerializeField] private int port = 90;
    
    public ThreadDispatcher threadDispatcher;
    
    public static NetworkingController instance;

    public List<CustomNetworkBehavior> players = new List<CustomNetworkBehavior>();

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        Networking.onOponentConnect += () => 
        {
            SceneManager.LoadScene("PlayScene");
        };
    }

    private void Update()
    {
        if (Networking.acceptedMessages.Count > 0)
        {
            BaseMessage baseMessage = Networking.acceptedMessages.Dequeue();
            if (baseMessage.messageType == MessageType.Position)
            {
                PositionMessage posMessage = (PositionMessage)baseMessage;

                players.ForEach(player =>
                {
                    player.OnNetworkUpdate(posMessage);
                });
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "StartScene")
        {

        }
    }

    #region Testing

    public void InitAsServer()
    {
        Networking.Start(ipAdress, port, NetworkingMode.Server);
    }

    public void InitAsClient()
    {
        Networking.Start(ipAdress, port, NetworkingMode.Client);
    }

    [ContextMenu("Send test message")]
    public void Test()
    {
        //
    }
    #endregion
}
