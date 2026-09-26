using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GDS.Core;
using GDS.Core.UGUI;
using UnityEngine;

namespace GDS.Examples {

    public class NetworkListBag : NetworkBehaviour {
        public readonly SyncList<Item> Items = new();

        [SerializeField] ListBagView listBagView;
        public ListBag Bag => listBagView.Bag;

        // void Awake() {
        //     Items.OnChange += OnItemsChange;
        // }

        void Start() { Items.OnChange += OnItemsSyncChange; }

        void OnDestroy() { Items.OnChange -= OnItemsSyncChange; }

        private void OnItemsSyncChange(SyncListOperation op, int index, Item oldItem, Item newItem, bool asServer) {
            if (op != SyncListOperation.Set) return;
            var slot = Bag.Slots[index];
            Bag.ReplaceAt(slot, newItem);
        }


        // public override void OnStartServer() {
        //     base.OnStartServer();
        //     Debug.Log($"on start server, should populate sync list {SharedStash.Slots.CommaJoin()}");
        //     sharedStashSync.Clear();
        //     sharedStashSync.AddRange(SharedStash.Slots.Select(s => s.Item).ToList());

        // }

        // public override void OnStartClient() {
        //     base.OnStartClient();
        //     Debug.Log($"on start client, should show stash items: {sharedStashSync.CommaJoin()}");
        // }

        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"on start server, should populate sync list {Bag.Slots.CommaJoin()}");
            Items.Clear();
            Items.AddRange(Bag.Slots.Select(s => s.Item).ToList());

        }

        // public override void OnStartClient() {
        //     base.OnStartClient();
        //     Debug.Log($"on start client, should show stash items: {Items.CommaJoin()}");
        //     // if (IsServerInitialized) {
        //     //     Debug.Log($"on start server, should populate sync list {listBag.Bag.Slots.CommaJoin()}");
        //     // }
        // }

        // private void OnItemsChange(SyncListOperation op, int index, Item oldItem, Item newItem, bool asServer) {
        //     if (asServer) return;
        //     // if (op != SyncListOperation.Complete) return;
        //     Debug.Log($"client: {OwnerId}, on item changed: {newItem}, op: {op}");

        // }

        // [ServerRpc(RequireOwnership = false)]
        // void InitItems(List<Item> items) {
        //     Items.Clear();
        //     Items.AddRange(items);
        // }

        // [ServerRpc(RequireOwnership = false)]
        // public void RequestPickItem(string bagId, int slotIndex, NetworkConnection sender = null) {
        //     if (sender == null) return;
        //     var item = Items.ElementAtOrDefault(slotIndex);
        //     if (item == null) { Debug.Log($"no item found at index {slotIndex}"); return; }
        //     Items.Set(slotIndex, null);
        //     ResponsePickItem(sender, item);
        // }

        // [TargetRpc]
        // private void ResponsePickItem(NetworkConnection conn, Item item) {
        //     Ghost.Item = item;
        //     Ghost.Notify();
        //     Bus.Publish(new PickItemSuccess(item));
        // }


    }

}