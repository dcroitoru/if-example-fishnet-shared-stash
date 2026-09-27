using FishNet.Object;
using UnityEngine.InputSystem;

namespace GDS.Examples {

    public class CharacterNetworkMove : NetworkBehaviour {
        float speed = 0.06f;
        void Update() {
            if (!IsOwner) return;

            var newPos = transform.position;
            if (Keyboard.current.aKey.isPressed) { newPos.x -= speed; }
            if (Keyboard.current.dKey.isPressed) { newPos.x += speed; }
            if (Keyboard.current.wKey.isPressed) { newPos.z += speed; }
            if (Keyboard.current.sKey.isPressed) { newPos.z -= speed; }
            transform.position = newPos;
        }
    }

}