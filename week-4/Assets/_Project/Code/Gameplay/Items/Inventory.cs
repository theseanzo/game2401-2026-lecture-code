using System;
using System.Collections.Generic;
using _Project.Code.Core.Patterns;

namespace _Project.Code.Gameplay.Items
{
    public class Inventory : Singleton<Inventory>
    {
        private readonly HashSet<Item> _items = new();

        public IReadOnlyCollection<Item> Items => _items;

        public event Action<Item> OnItemAdded;

        public void Add(Item item)
        {
            if (item == null || !_items.Add(item))
            {
                return;
            }
            OnItemAdded?.Invoke(item);
        }

        public bool Has(Item item)
        {
            return item != null && _items.Contains(item);
        }
    }
}
