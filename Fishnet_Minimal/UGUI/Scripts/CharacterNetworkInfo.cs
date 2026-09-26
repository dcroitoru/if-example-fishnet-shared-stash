using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GDS.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterNetworkInfo : NetworkBehaviour {

    // [SerializeField] TextMeshPro playerInfoText;

    public readonly SyncList<Item> Items = new();
    List<Item> initialItems;

    public void Init(ListBag inventory) {
        initialItems = inventory.Slots.Select(s => s.Item).ToList();
        inventory.OnItemChanged += OnItemChanged;
    }

    private void OnItemChanged(ListSlot slot) {
        SetItem(slot.Index, slot.Item);
    }

    void OnDestroy() {

    }

    void Awake() {
        Items.OnChange += OnItemsChange;
    }

    public override void OnStartClient() {
        base.OnStartClient();
        GameManager.OnPlayerConnected?.Invoke(this);
        if (!IsOwner) return;
        Debug.Log($"on start client {OwnerId}");
        InitItems(initialItems);
    }

    public override void OnStopClient() {
        base.OnStopClient();
        GameManager.OnPlayerDisconnected?.Invoke(this);
    }

    private void OnItemsChange(SyncListOperation op, int index, Item oldItem, Item newItem, bool asServer) {
        if (asServer) return;
        if (op != SyncListOperation.Complete) return;
        // Debug.Log($"client: {OwnerId}, on item changed: {newItem}, op: {op}");
        // Debug.Log(string.Join(", ", Items.Collection));
        // RebuildText();
    }

    [ServerRpc]
    void InitItems(List<Item> items) {
        Items.Clear();
        Items.AddRange(items);
    }

    [ServerRpc]
    void SetItem(int index, Item item) {
        Items.Set(index, item);
    }

    // // Update is called once per frame
    // void Update() {
    //     if (!IsOwner) return;
    //     if (Keyboard.current.qKey.wasPressedThisFrame) { AddItem(); }
    //     if (Keyboard.current.wKey.wasPressedThisFrame) { RemoveItem(); }
    // }
}
