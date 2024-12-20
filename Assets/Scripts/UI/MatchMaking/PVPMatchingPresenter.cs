using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Data;
using Fusion;
using UnityEngine;

public class PVPMatchingPresenter : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public Transform RedTeamPanel;
    public Transform BlueTeamPanel;
    
    public GameObject CharacterPanel;
    public PVPSettingPanel SettingPanel;
    
    [Networked] private NetworkButtons StartButton { get; set; }

    [Networked] public NetworkLinkedList<PlayerRef> RedTeam { get; }
    [Networked] public NetworkLinkedList<PlayerRef> BlueTeam { get; }

    private Dictionary<PlayerRef, GameObject> panels = new Dictionary<PlayerRef, GameObject>();

    [SerializeField]
    public GameObject LoadingPanel;

    //임시. Ready하면 바로 고하게 설정해뒀는데 이렇게 했더니 씬이 여러번 불려와서 에러남.
    private bool GameIsStarted = false;
    
    public override void Spawned()
    {
        Debug.Log("MatchingPresenter");
        
        if (HasStateAuthority)
        {
            PVPData data = FindObjectOfType<GameSettingData>().Data;
            SettingPanel.SetData(data);

            foreach (var player in Runner.ActivePlayers)
            {
                SetPlayerTeam(player);
            }
        }
        else
        {
            SettingPanel.SetData(RoomManager.Instance.GetGameSetting());
            SettingPanel.ChangeSetting -= RPC_ChangeData;
            SettingPanel.ChangeSetting += RPC_ChangeData;
        }
        
        foreach (var player in RedTeam)
        {
            SetPlayerPanel(player);
        }
        
        foreach (var player in BlueTeam)
        {
            SetPlayerPanel(player);
        }

        RPC_SetPanelDataRequest();
    }

    public override void FixedUpdateNetwork()
    {
        if (PlayerRegistry.CountPlayers < 1) { return; }

        bool areAllReady = true;
        foreach (KeyValuePair<PlayerRef, PlayerObject> player in PlayerRegistry.Instance.ObjectByRef)
        {
            if (!player.Value.IsReady)
            {
                areAllReady = false;
                break;
            }
        }
        if (areAllReady && !GameIsStarted)
        {
            GameIsStarted = true;
            StartCoroutine(GameStart());
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ChangeData(MapType mapType, bool crystal, bool decay)
    {
        RoomManager.Instance.SetGameSettting(new PVPData(mapType, decay, crystal));
        RPC_SetData(mapType, crystal, decay);
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetData(MapType mapType, bool crystal, bool decay)
    {
        SettingPanel.SetData(new PVPData(mapType, decay, crystal));
    }

    IEnumerator GameStart()
    {
        Rpc_SetTeam();
        RoomManager.State.Server_SetState(GameState.EGameState.Loading);
        LoadingPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        Runner.LoadScene(RoomManager.Instance.MapType.ToString());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_SetTeam()
    {
        for (int i = 0; i < RedTeam.Count; i++)
        {
            PlayerObject po = PlayerRegistry.GetPlayer(RedTeam[i]);
            po.SetLayer(LayerMask.NameToLayer("RedTeam"));
            po.TeamNumber = i;
        }

        for (int i = 0; i < BlueTeam.Count; i++)
        {
            PlayerObject po = PlayerRegistry.GetPlayer(BlueTeam[i]);
            po.SetLayer(LayerMask.NameToLayer("BlueTeam"));
            po.TeamNumber = i;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayerReady(PlayerObject caller)
    {
        caller.IsReady = !caller.IsReady;
        panels.TryGetValue(caller.Ref, out GameObject playerpanel);
        playerpanel.GetComponent<CharacterSlot>().SetReady(caller.IsReady);
    }

    public void CallReady()
    {
        PlayerObject _local = PlayerObject.Local;
        RPC_PlayerReady(_local);
    }

    public void PlayerJoined(PlayerRef player)
    {
        if(HasStateAuthority) SetPlayerTeam(player);
        SetPlayerPanel(player);
        RPC_SetPanelDataRequest();
    }

    private void SetPlayerTeam(PlayerRef player)
    {
        int redTeam = RedTeamPanel.childCount;
        int blueTeam = BlueTeamPanel.childCount;

        PlayerObject po = PlayerRegistry.GetPlayer(player);
        
        if (redTeam < blueTeam)
        {
            RedTeam.Add(player);
        }
        else
        {
            BlueTeam.Add(player);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetPanelDataRequest()
    {
        foreach (var player in PlayerRegistry.Instance.ObjectByRef)
        {
            player.Value.OnStatChanged += delegate
            {
                RPC_SetPanelData(player.Key, player.Value.Character, player.Value.Nickname);
            };
            
            Debug.Log("RPC_SetPanelDataRequest" + player.Key + " " + player.Value.Character + " " + player.Value.Nickname);
            RPC_SetPanelData(player.Key, player.Value.Character, player.Value.Nickname);
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPanelData(PlayerRef player, CharacterType character, string nickname)
    {
        Debug.Log("RPC_SetPanelData" + player + " " + character + " " + nickname);
        panels[player].GetComponent<CharacterSlot>().SetData(character, nickname);
    }

    private void SetPlayerPanel(PlayerRef player)
    {
        GameObject go = Instantiate(CharacterPanel);
        
        panels[player] = go;

        if (RedTeam.Contains(player))
        {
            go.transform.SetParent(RedTeamPanel);
        }
        else
        {
            go.transform.SetParent(BlueTeamPanel);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        Destroy(panels[player]);
        if (RedTeam.Contains(player))
        {
            RedTeam.Remove(player);
        }
        else
        {
            BlueTeam.Remove(player);
        }
    }


}
