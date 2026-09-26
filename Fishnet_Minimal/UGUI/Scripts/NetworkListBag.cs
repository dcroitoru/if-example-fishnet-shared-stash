using System.Linq;
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

        void Start() { Items.OnChange += OnItemsSyncChange; }

        void OnDestroy() { Items.OnChange -= OnItemsSyncChange; }

        private void OnItemsSyncChange(SyncListOperation op, int index, Item oldItem, Item newItem, bool asServer) {
            if (op != SyncListOperation.Set) return;
            var slot = Bag.Slots[index];
            Bag.ReplaceAt(slot, newItem);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"on start server, should populate sync list {Bag.Slots.CommaJoin()}");
            Items.Clear();
            Items.AddRange(Bag.Slots.Select(s => s.Item).ToList());

        }

        public override void OnStartClient() {
            base.OnStartClient();
            if (IsServerInitialized) return;
            Debug.Log($"on start client, should sync bag: {Items.CommaJoin()}");
            for (var i = 0; i < Items.Count; i++) Bag.Slots[i].Item = Items[i];
            Bag.NotifyReset();
        }

    }

}