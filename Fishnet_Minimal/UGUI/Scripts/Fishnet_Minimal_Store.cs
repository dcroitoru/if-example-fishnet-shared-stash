using System;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using GDS.Core;
using GDS.Core.Events;
using GDS.Core.UGUI;
using UnityEngine;

namespace GDS.Examples {


    public class Fishnet_Minimal_Store : NetworkStore {

        [SerializeField] ListBagView inventory;
        [SerializeField] NetworkListBag networkBag;

        [SerializeField] private NetworkObject itemPrefab;


        void Awake() { StoreLocator.Register(this); }

        // Register event handlers for pick and place
        // The events are published by the DragAndDrop System
        void OnEnable() {
            Bus.On<PickItem>(OnPickItem);
            Bus.On<PlaceGhostItem>(OnPlaceItem);
            Bus.On<DropGhostItem>(OnDropItem);
            Bus.On<PickWorldItem>(OnPickWorldItem);
        }

        void OnDisable() {
            Bus.Off<PickItem>(OnPickItem);
            Bus.Off<PlaceGhostItem>(OnPlaceItem);
            Bus.Off<DropGhostItem>(OnDropItem);
            Bus.Off<PickWorldItem>(OnPickWorldItem);
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

        void OnDropItem(DropGhostItem e) {
            Debug.Log("1. OnDropItem");
            if (Ghost.Empty) return;
            if (e.IsOverUi) return;

            RequestSpawnItem(Ghost.Item, e.WorldPosition);
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestSpawnItem(Item item, Vector3 worldPos, NetworkConnection sender = null) {
            Debug.Log($"2. RequestSpawnItem on server, sender={sender?.ClientId}");

            NetworkObject instance = Instantiate(itemPrefab, worldPos, Quaternion.identity);
            var worldItem = instance.GetComponent<NetworkWorldItem>();
            worldItem.Item.Value = item;
            ServerManager.Spawn(instance);
            // worldItem.OnClick += OnWorldItemClick;

            // if (sender == null) return;
            // BroadcastSpawnItem(item, worldPos);
            ResponseSpawnItem(sender);
        }

        [ObserversRpc]
        void BroadcastSpawnItem(Item item, Vector3 worldPos) {
            Debug.Log($"3. BroadcastSpawnItem on client {NetworkManager.ClientManager.Connection.ClientId}, item={item}");

            // Debug.Log($"should spawn item {item}");
            Bus.Publish(new SpawnWorldItem(item, worldPos));
        }

        [TargetRpc]
        void ResponseSpawnItem(NetworkConnection sender) {
            Ghost.Reset();
        }

        void OnPickWorldItem(PickWorldItem e) {
            Debug.Log($"should pick world item {e.WorldItem}");
            RequestDespawnItem(e.WorldItem.Item, e.WorldItem.GameObject);
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestDespawnItem(Item item, GameObject go, NetworkConnection sender = null) {
            ServerManager.Despawn(go);
            ResponseDespawnItem(sender, item);
        }

        [TargetRpc]
        void ResponseDespawnItem(NetworkConnection sender, Item item) {
            Result result = inventory.Bag.Add(item);
            Bus.Publish(result);

        }
    }

}