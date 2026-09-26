using FishNet.Object;
using UnityEngine.InputSystem;

namespace GDS.Examples {

    public class CharacterNetworkMove : NetworkBehaviour {
        void Update() {
            if (!IsOwner) return;

            var newPos = transform.position;
            if (Keyboard.current.aKey.isPressed) { newPos.x -= 0.1f; }
            if (Keyboard.current.dKey.isPressed) { newPos.x += 0.1f; }
            transform.position = newPos;
        }
    }

}