using System.Collections.Generic;
using GDS.Core;
using GDS.Core.UGUI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GDS.Examples {

    public class GameManager : MonoBehaviour {

        public static List<ItemBase> ItemBaseRegistry;

        [SerializeField] ItemBaseCatalogSO ItemBaseCatalog;
        [SerializeField] ListBagView inventoryView;
        [SerializeField] ListBagView stashView;

        bool visible = true;

        void Awake() {
            ItemBaseRegistry = ItemBaseCatalog.Items;
            UpdateState();
        }

        void Update() {
            if (Keyboard.current.tabKey.wasPressedThisFrame) {
                visible = !visible;
                UpdateState();
            }
        }

        void UpdateState() {
            inventoryView.gameObject.SetActive(visible);
            stashView.gameObject.SetActive(visible);
        }
    }
}