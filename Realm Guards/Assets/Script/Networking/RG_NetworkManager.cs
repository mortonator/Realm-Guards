using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class RG_NetworkManager : NetworkManager
{
    public static RG_NetworkManager Instance;
    MapHandler mapHandler;

    public static PlayerManager localPlayer;
    public static List<PlayerManager> players;
    bool useSteam = false;
    public static bool areAllPlayersReady = false;

    [SerializeField] int playerLayer, interactableLayer;

    public override void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;

        base.Awake();

        Physics.IgnoreLayerCollision(playerLayer, interactableLayer, true);
        mapHandler = GetComponent<MapHandler>();
    }

    public void ChangeTransportToSteam()
    {
        useSteam = true;
        transport = GetComponent<Mirror.FizzySteam.FizzySteamworks>();
    }

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        players = new List<PlayerManager>();
    }
    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
        players = new List<PlayerManager>();
        areAllPlayersReady = false;

        foreach (var prefab in spawnablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab);
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            bool isLeader = players.Count == 0;

            if (useSteam)
            {
                //get id from steam
                CSteamID steamId = SteamMatchmaking.GetLobbyMemberByIndex(
                    SteamLobby.LobbyID,
                    numPlayers - 1 //this player is last in the list. 
                    );

                //set their id
                PlayerManager player = Instantiate(playerPrefab).GetComponent<PlayerManager>();
                player.SetPlayerInfo(isLeader, useSteam, steamId.m_SteamID);
                NetworkServer.AddPlayerForConnection(conn, player.gameObject);
            }
            else
            {
                PlayerManager player = Instantiate(playerPrefab).GetComponent<PlayerManager>();
                player.SetPlayerInfo(isLeader);
                NetworkServer.AddPlayerForConnection(conn, player.gameObject);
            }

            Debug.Log("Player spawned");
        }
        else
        {
            Debug.Log("Cannot spawn player, server game is running");
            conn.Disconnect();
        }
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
    }
    public override void OnStopServer()
    {
        foreach (PlayerManager player in players)
        {
            if (player != null && player.gameObject != null)
                Destroy(player.gameObject);
        }
        players.Clear();
    }

    [Server] public void StartGame()
    {
        if (IsReadyToStart() == false)
            return;

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            MapHandler.currentMapNumber = 0;
            GameManager.Instance.Ser_ApplyRoleEffects();
            mapHandler.BuildMapList();
        }
        else
        {
            MapHandler.currentMapNumber -= 1;
        }

        Debug.Log("StartGame: Next Map");
        mapHandler.NextMap();
        ServerChangeScene(mapHandler.currentMapName);
    }
    [Server] public void Ser_GameOver() => ServerChangeScene(mapHandler.officeMapName);

    public bool IsReadyToStart()
    {
        if (numPlayers < 1)
            return false;

        return areAllPlayersReady;
    }
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.Log("ChangeScene; localPlayer: " + localPlayer + ". playerCon: " + localPlayer.playerCon);
        FindObjectOfType<PortalManager>().BeginSceneTransition(newSceneName);
    }
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        Debug.Log("ChangeScene; localPlayer: " + localPlayer + ". playerCon: " + localPlayer.playerCon);

        base.OnClientSceneChanged(conn);

        if (localPlayer != null)
        {
            localPlayer.playerCon.transform.position = Vector3.zero;
            localPlayer.playerCon.SetupCamera();
            localPlayer.playerCon.UnFreeze();
            localPlayer.playerCon.playerUi.loadingBarText.transform.parent.gameObject.SetActive(false);

            localPlayer.playerCon.SpawnInNewWorld();
        }
    }
}
