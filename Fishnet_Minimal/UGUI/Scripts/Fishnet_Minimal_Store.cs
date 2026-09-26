using System;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GDS.Core;
using GDS.Core.Events;
using UnityEngine;

namespace GDS.Examples {


    public class Fishnet_Minimal_Store : NetworkStore {

        [SerializeField] NetworkListBag networkBag;

        // public ListBag SharedStash = new() { Size = 20 };

        // TODO: sync stash on new client connect
        // readonly SyncList<Item> sharedStashSync = new();



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

        // private void OnSharedStashSyncChange(SyncListOperation op, int index, Item oldItem, Item newItem, bool asServer) {
        //     if (op != SyncListOperation.Set) return;
        //     var slot = SharedStash.Slots[index];
        //     SharedStash.ReplaceAt(slot, newItem);
        // }

        void Awake() { StoreLocator.Register(this); }

        // void Start() { sharedStashSync.OnChange += OnSharedStashSyncChange; }

        // void OnDestroy() { sharedStashSync.OnChange -= OnSharedStashSyncChange; }


        // Register event handlers for pick and place
        // The events are published by the DragAndDrop System
        void OnEnable() {
            Bus.On<PickItem>(OnPickItem);
            Bus.On<PlaceGhostItem>(OnPlaceItem);
        }

        void OnDisable() {
            Bus.Off<PickItem>(OnPickItem);
            Bus.Off<PlaceGhostItem>(OnPlaceItem);
        }

        // On pick, remove the item from the item from the bag, update the ghost and publish the resulting event
        void OnPickItem(PickItem e) {
            if (e.Context.Bag == networkBag.Bag) {
                RequestPickItem(e.Bag.Name, (e.Slot as ListSlot).Index);
            } else {
                Result result = e.Context.Bag.Remove(e.Context.Item);
                UpdateGhost(result, e.Context);
                Bus.Publish(result);
            }
        }

        // On place, add the ghost item to the bag, potentially swapping with the one in the target slot, then publish the resulting event
        void OnPlaceItem(PlaceGhostItem e) {
            if (e.Context.Bag == networkBag.Bag) {
                RequestPlaceItem(e.Context.Bag.Name, (e.Context.Slot as ListSlot).Index, Ghost.Item);
            } else {
                Result result = e.Context.Bag.AddAt(e.Context.Slot, Ghost.Item);
                UpdateGhost(result, e.Context);
                Bus.Publish(result);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPickItem(string bagId, int slotIndex, NetworkConnection sender = null) {
            if (sender == null) return;
            var item = networkBag.Items.ElementAtOrDefault(slotIndex);
            if (item == null) { Debug.Log($"no item found at index {slotIndex}"); return; }
            networkBag.Items.Set(slotIndex, null);
            ResponsePickItem(sender, item);
        }

        [TargetRpc]
        private void ResponsePickItem(NetworkConnection conn, Item item) {
            Ghost.Item = item;
            Ghost.Notify();
            Bus.Publish(new PickItemSuccess(item));
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPlaceItem(string bagId, int slotIndex, Item item, NetworkConnection sender = null) {
            if (sender == null) return;
            // Note: you can treat different bags here
            var replaced = networkBag.Items[slotIndex];
            networkBag.Items.Set(slotIndex, item);
            ResponsePlaceItem(sender, replaced);
        }

        [TargetRpc]
        private void ResponsePlaceItem(NetworkConnection conn, Item replaced) {
            Ghost.Bag = null;
            Ghost.Slot = null;
            Ghost.Item = replaced;
            Ghost.Notify();
            Bus.Publish(new PlaceItemSuccess(null, null));

        }
    }

}