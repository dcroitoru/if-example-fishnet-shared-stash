using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object.Synchronizing;
using GDS.Core;
using GDS.Core.UGUI;
using GDS.Examples;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour {

    public static List<ItemBase> ItemBaseRegistry;

    public static Action<CharacterNetworkInfo> OnPlayerConnected;
    public static Action<CharacterNetworkInfo> OnPlayerDisconnected;
    public List<CharacterNetworkInfo> players;


    [SerializeField] ItemBaseCatalogSO ItemBaseCatalog;
    [SerializeField] TextMeshProUGUI textField;
    [SerializeField] ListBagView inventoryView;
    [SerializeField] ListBagView stashView;

    bool visible = true;

    private void OnEnable() {
        OnPlayerConnected += OnClientConnectedHandler;
        OnPlayerDisconnected += OnClientDisconnectedHandler;
    }

    private void OnDisable() {
        OnPlayerConnected -= OnClientConnectedHandler;
        OnPlayerDisconnected -= OnClientDisconnectedHandler;
    }

    void Awake() {
        ItemBaseRegistry = ItemBaseCatalog.Items;
        UpdateState();
    }

    void Start() {
        var store = StoreLocator.Get<Fishnet_Minimal_Store>();
        // stashView.Init(store.SharedStash);
    }

    private void OnClientConnectedHandler(CharacterNetworkInfo equipment) {
        Debug.Log($"client connected {equipment}");
        players.Add(equipment);
        equipment.Items.OnChange += OnEquipmentChange;
        if (equipment.OwnerId == InstanceFinder.ClientManager.Connection.ClientId) {
            equipment.Init(inventoryView.Bag);
        }
    }

    private void OnClientDisconnectedHandler(CharacterNetworkInfo equipment) {
        Debug.Log($"client disconnected {equipment}");
        Debug.Log($"should remove {equipment.OwnerId}, index: {players.FindIndex(p => p.OwnerId == equipment.OwnerId)}");
        if (equipment.OwnerId == InstanceFinder.ClientManager.Connection.ClientId) {
            players.Clear();
            // should also unsub from each player
        } else {
            players.Remove(equipment);
            equipment.Items.OnChange -= OnEquipmentChange;

        }
        Refresh();
    }

    private void OnEquipmentChange(SyncListOperation op, int index, Item oldItem, Item newItem, bool asServer) {
        if (op != SyncListOperation.Complete) return;
        Refresh();
    }

    private void Refresh() {
        int myClientId = InstanceFinder.ClientManager.Connection.ClientId;
        var sortedPlayers = players.OrderBy(a => a.OwnerId);
        textField.text = string.Join("\n",
            sortedPlayers.Select(player => {
                string suffix = player.OwnerId == myClientId ? " (you)" : "";
                var clientName = $"{player.OwnerId}{suffix}";
                var items = player.Items.CommaJoin();
                return $"Client: {clientName}\nItems: {items}\n";
            })
        );
    }

    void Update() {
        if (Keyboard.current.tabKey.wasPressedThisFrame) {
            visible = !visible;
            UpdateState();
        }
    }

    void UpdateState() {
        inventoryView.gameObject.SetActive(visible);
    }
}