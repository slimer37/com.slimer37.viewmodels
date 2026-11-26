using System;

namespace Slimer37.Viewmodels.Samples
{
    public class Hand
    {
        public event Action<Item> OnPickup;
        public event Action<Item> OnRemove;

        private Item _heldItem;

        public Item HeldItem
        {
            get => _heldItem;
            set
            {
                if (!value)
                {
                    OnRemove?.Invoke(_heldItem);
                }

                _heldItem = value;

                if (_heldItem)
                {
                    OnPickup?.Invoke(_heldItem);
                }
            }
        }
    }
}