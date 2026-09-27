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

        // current player inventory (client authority)
        [SerializeField] ListBagView inventory;
        // shared stash (server authority)
        [SerializeField] NetworkListBag networkBag;
        // world item prefab (requires WorldItem script)
        [SerializeField] NetworkObject itemPrefab;

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
            // if the bag has server authority send a request instead of the usual flow
            if (e.Context.Bag == networkBag.Bag) {
                RequestPickItem(e.Bag.Name, (e.Slot as ListSlot).Index);
            } else {
                Result result = e.Context.Bag.Remove(e.Context.Item);
                UpdateGhost(result, e.Context);
                Bus.Publish(result);
            }
        }

        // a server rpc that performs the action on the server auth bag
        // RequireOwnership needs to be false, since clients don't own the object
        [ServerRpc(RequireOwnership = false)]
        public void RequestPickItem(string bagId, int slotIndex, NetworkConnection sender = null) {
            if (sender == null) return;
            var item = networkBag.Items.ElementAtOrDefault(slotIndex);
            if (item == null) { Debug.Log($"no item found at index {slotIndex}"); return; }
            networkBag.Items.Set(slotIndex, null);
            // sending back a response means a successful operation
            ResponsePickItem(sender, item);
        }

        // a client rpc that cleans up the ghost item and publishes a success event (for sfx, vfx)
        [TargetRpc]
        private void ResponsePickItem(NetworkConnection conn, Item item) {
            Ghost.Item = item;
            Ghost.Notify();
            Bus.Publish(new PickItemSuccess(item));
        }

        // On place, add the ghost item to the bag, potentially swapping with the one in the target slot, then publish the resulting event
        void OnPlaceItem(PlaceGhostItem e) {
            // if the bag has server authority send a request instead of the usual flow
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
            // Note: you can switch on bagId and treat different bags here
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
            if (Ghost.Empty) return;
            if (e.IsOverUi) return;
            RequestSpawnItem(Ghost.Item, e.WorldPosition);
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestSpawnItem(Item item, Vector3 worldPos, NetworkConnection sender = null) {
            // to successfuly instantiate a prefab and replicate on all clients, the prefab needs to be a NetworkObject            
            // for this particular example to work, it also needs to be a NetworkWorldItem, which does 2 things: 
            // stores item data in a SyncVar and adds a click listener
            NetworkObject instance = Instantiate(itemPrefab, worldPos, Quaternion.identity);
            var worldItem = instance.GetComponent<NetworkWorldItem>();
            worldItem.Item.Value = item;
            ServerManager.Spawn(instance);
            ResponseSpawnItem(sender);
        }

        [TargetRpc]
        void ResponseSpawnItem(NetworkConnection sender) {
            Bus.Publish(new DropWorldItemSuccess(Ghost.Item));
            Ghost.Reset();
        }

        void OnPickWorldItem(PickWorldItem e) {
            // picking a world item is server authoritative, but before sending a request
            // we check that it can fit in player inventory
            var result = inventory.Bag.CanAdd(e.WorldItem.Item);
            if (result is Fail) {
                Bus.Publish(result);
            } else {
                RequestDespawnItem(e.WorldItem.Item, e.WorldItem.GameObject);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestDespawnItem(Item item, GameObject go, NetworkConnection sender = null) {
            ServerManager.Despawn(go);
            // after despawning the game object, send a success reponse back to sender
            ResponseDespawnItem(sender, item);
        }

        [TargetRpc]
        void ResponseDespawnItem(NetworkConnection sender, Item item) {
            // at this point the item has successfully despawned (server-side) and can be added to player inventory (client-side)
            Result result = inventory.Bag.Add(item);
            Bus.Publish(result);

        }
    }

}