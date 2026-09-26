using FishNet.Object;
using FishNet.Object.Synchronizing;
using GDS.Common.Scripts;
using GDS.Core;
using UnityEngine;

namespace GDS.Examples {

    public class NetworkWorldItem : NetworkBehaviour {
        public readonly SyncVar<Item> Item = new();

        public override void OnStartClient() {
            base.OnStartClient();
            Debug.Log($"network item awake: {Item}");
            GetComponent<WorldItem>().Init(Item.Value);
        }
    }

}