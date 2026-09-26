using UnityEngine;
using FishNet.Object;
using GDS.Core.Events;
using GDS.Core;

namespace GDS.Examples {

     [DisallowMultipleComponent, DefaultExecutionOrder(-1)]
     public class NetworkStore : NetworkBehaviour, IStore {
          EventBus bus = new();
          GhostContext ghost = new();
          public EventBus Bus => bus;
          public GhostContext Ghost => ghost;

          protected virtual void UpdateGhost(Result result, IItemContext context) {
               var item = result switch {
                    PickItemSuccess x => x.Item,
                    PlaceItemSuccess x => x.Replaced,
                    _ => null
               };
               Ghost.SetValue(context, item);
          }
     }

}