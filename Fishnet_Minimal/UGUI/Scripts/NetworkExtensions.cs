using FishNet.Serializing;
using GDS.Core;

namespace GDS.Examples {
    public static class NetworkExtensions {

        public static void WriteItem(this Writer writer, Item item) {
            if (item == null) {
                writer.WriteUInt8Unpacked(0);
                return;
            }

            var index = GameManager.ItemBaseRegistry.FindIndex(i => { return item.Base == i; });
            if (index == -1) throw new System.Exception($"could not find item in registry: {item.Name}");

            writer.WriteUInt8Unpacked(1);
            writer.WriteInt32(index);
            writer.WriteInt32(item.StackSize);
        }

        public static Item ReadItem(this Reader reader) {
            var flag = reader.ReadUInt8Unpacked();
            if (flag == 0) return null;

            var index = reader.ReadInt32();
            var stackSize = reader.ReadInt32();
            var itemBase = GameManager.ItemBaseRegistry[index];
            if (itemBase == null) throw new System.Exception($"item base not in registry, at index: {index}");

            var item = itemBase.CreateItem();
            item.StackSize = stackSize;
            return item;
        }
    }

}
