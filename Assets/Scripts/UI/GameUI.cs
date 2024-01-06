using UnityEngine;
using TMPro;
using System;

public enum CameraAngle
{
    Menu=0,
    WhiteTeam=1,
    BlackTeam=2,
    Length = 3
}
public class GameUI : MonoBehaviour
{
    public Server server;
    public Client client;
    public Action<bool> SetLocalGame;
    public static GameUI Instance { get; private set; }
    [SerializeField] private Animator MenuAnimator;
    [SerializeField] private TMP_InputField addresseInput;
    [SerializeField] private GameObject[] cameraAngle;
    private void Awake()
    {
        Instance = this;
        RegisterEvents();
    }
    // Cameras control :
    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngle.Length; i++)
        {
            cameraAngle[i].SetActive(false);
        }
        cameraAngle[(int)index].SetActive(true);
    }
    public void OnLocalGameButton()
    {
        MenuAnimator.SetTrigger("InGameMenu");
        SetLocalGame?.Invoke(true);
        server.Init(8006);
        client.Init("127.0.0.1", 8006);
    }
    public void OnOnlineGameButton()
    {
        MenuAnimator.SetTrigger("OnlineMenu");
        SetLocalGame?.Invoke(false);
    }
    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8006);
        client.Init("127.0.0.1", 8006);
        MenuAnimator.SetTrigger("HostMenu");
    }
    public void OnOnlineConnectButton()
    {
        SetLocalGame?.Invoke(false);
        client.Init(addresseInput.text, 8006);
    }
    public void OnOnlineBackButton()
    {
        MenuAnimator.SetTrigger("StartMenu");
    }
    public void OnHostBackMenu()
    {
        server.Shutdown();
        client.Shutdown();
        MenuAnimator.SetTrigger("OnlineMenu");
    }
    public void OnLeaveFromGameMenu()
    {
        ChangeCamera(CameraAngle.Menu);
        MenuAnimator.SetTrigger("StartMenu");
    }
    #region GameEvents listeners 
    private void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient;
    }
    private void UnregisterEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient;
    }
    private void OnStartGameClient(NetMessage msg)
    {
        MenuAnimator.SetTrigger("InGameMenu");
    }
    #endregion
}
