using System;
using System.Collections.Generic;

namespace Data
{
    public class SaveData
    {
        public List<Guid> Characters = new();
        public List<ItemSaveData> Items = new();
    }

    public class ItemSaveData
    {
        public Guid Guid;
        public int Amount = 1;
        public bool IsEquipped = false;
        public bool IsSelected = false;
    }
}