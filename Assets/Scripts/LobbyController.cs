using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class LobbyController : MonoBehaviour
{
    public TMP_InputField lobbyCodeInputField;
    public TMP_InputField nicknameInputField;
    public TMP_Text codeText;
    public TMP_Text clientcodeText;
    public GameObject playerListContent;
    public GameObject clientplayerListContent;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private string playerName;
    public const string KEY_START_GAME = "Start";
    public event EventHandler<EventArgs> OnGameStarted;
    private async void Start() {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = nicknameInputField.text;
        Debug.Log(playerName);
    }

    private void Update() {
        if (lobbyCodeInputField.gameObject != null && lobbyCodeInputField.GetComponentInParent<Button>() != null && lobbyCodeInputField != null)
        {
            if (lobbyCodeInputField.text != "")
            {
                lobbyCodeInputField.GetComponentInParent<Button>().interactable = true;
            }
            else
            {
                lobbyCodeInputField.GetComponentInParent<Button>().interactable = false;
            }
        }

        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        try
                        {
                            TestRelay.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                        }
                        catch (LobbyServiceException e)
                        {
                            Debug.Log(e);
                        }
                    }

                    joinedLobby = null;

                    OnGameStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public async void CreateLobby()
    {
        try {
            string lobbyName = "MyLobby";
            int maxPlayers = 10;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0")}
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;

            UpdatePlayerName(nicknameInputField.text);

            PrintPlayers(joinedLobby);
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            codeText.text = lobby.LobbyCode;
            clientcodeText.text = lobby.LobbyCode;
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }

    public async void ListLobbies() {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions{
                Count = 25,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);
    
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCodeInputField.text, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("Joined Lobby with code " + lobbyCodeInputField.text);

            UpdatePlayerName(nicknameInputField.text);

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            joinedLobby = lobby;

            UpdatePlayerName(nicknameInputField.text);

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    public void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name);
        for (int i = 0; i < playerListContent.transform.childCount; i++)
        {
            Destroy(playerListContent.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < clientplayerListContent.transform.childCount; i++)
        {
            Destroy(clientplayerListContent.transform.GetChild(i).gameObject);
        }
        foreach (Player player in lobby.Players)
        {
            GameObject playerElement = Instantiate(codeText.gameObject, playerListContent.transform);
            playerElement.GetComponent<TMP_Text>().text = player.Data["PlayerName"].Value;
            playerElement.GetComponent<TMP_Text>().color = Color.black;
            playerElement.GetComponent<TMP_Text>().enableWordWrapping = false;

            GameObject clientplayerElement = Instantiate(clientcodeText.gameObject, clientplayerListContent.transform);
            clientplayerElement.GetComponent<TMP_Text>().text = player.Data["PlayerName"].Value;
            clientplayerElement.GetComponent<TMP_Text>().color = Color.black;
            clientplayerElement.GetComponent<TMP_Text>().enableWordWrapping = false;

            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            if (newPlayerName != "")
            {
                playerName = newPlayerName;
            }
            else
            {
                playerName = "[null]";
            }
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                Data = new Dictionary<string, PlayerDataObject> {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;

            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                Debug.Log("StartGame");

                string relayCode = await TestRelay.Instance.CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions {
                    Data = new Dictionary<string, DataObject> {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                    }
                });

                joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public void RefreshLobby()
    {
        UpdatePlayerName(nicknameInputField.text);
        PrintPlayers();
        clientcodeText.text = joinedLobby.LobbyCode;
        codeText.text = joinedLobby.LobbyCode;
    }

    public bool IsLobbyHost()
    {
        // Get the current player object
        Player player = GetPlayer();
        // Compare the player ID with the lobby host ID
        if (player.Id == joinedLobby.HostId)
        {
            // The player is the host
            return true;
        }
        else
        {
            // The player is not the host
            return false;
        }
    }
}
