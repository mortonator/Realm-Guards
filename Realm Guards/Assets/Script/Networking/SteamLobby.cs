using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] GameObject connectingText;

    const string HostAddressKey = "HostAddress";
    public static CSteamID LobbyID;
    RG_NetworkManager networkManager;

    protected Callback<LobbyCreated_t> cb_lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> cb_gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> cb_lobbyEnter;

    void Start()
    {
        networkManager = GetComponent<RG_NetworkManager>();

        if (!SteamManager.Initialized) return;

        cb_lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        cb_gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        cb_lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }

    public void HostLobby()
    {
        RG_NetworkManager.Instance.StopHost();
        connectingText.SetActive(true);

        networkManager.ChangeTransportToSteam();
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }

    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            connectingText.SetActive(false);
            return;
        }

        // if successful, become a host and set address in the lobby
        networkManager.StartHost();
        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(LobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
    }
    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        // simply join
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    void OnLobbyEnter(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return; //is the host
        networkManager.ChangeTransportToSteam();

        // get the address from the lobby
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        connectingText.SetActive(true);
    }
}
