using FishNet.Object;
using FishNet.Object.Synchronizing;
using GDS.Common.Scripts;
using GDS.Core;
using GDS.Core.Events;
using UnityEngine;

namespace GDS.Examples {

    public class NetworkWorldItem : NetworkBehaviour {
        public readonly SyncVar<Item> Item = new();

        WorldItem worldItem;
        IStore store;

        private void Awake() {
            worldItem = GetComponent<WorldItem>();
            store = StoreLocator.Get();
        }

        public override void OnStartClient() {
            base.OnStartClient();
            Debug.Log($"network item awake: {Item}");
            worldItem.Init(Item.Value);
            worldItem.OnClick += OnWorldItemClick;
        }

        void OnWorldItemClick(IWorldItem worldItem) {
            store.Bus.Publish(new PickWorldItem(worldItem));
        }
    }

}