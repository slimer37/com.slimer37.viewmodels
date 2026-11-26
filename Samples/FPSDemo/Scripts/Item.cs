using UnityEngine;

namespace Slimer37.Viewmodels.Samples
{
    public class Item : MonoBehaviour, IViewmodelObjectSource
    {
        private Hand _hand;

        public void Pickup(Hand hand)
        {
            if (hand.HeldItem != null) return;

            hand.HeldItem = this;

            hand.OnRemove += OnRemove;

            _hand = hand;

            gameObject.SetActive(false);
        }

        private void OnRemove(Item obj)
        {
            if (obj == this)
            {
                gameObject.SetActive(true);
                _hand.OnRemove -= OnRemove;
            }
        }

        public virtual ViewmodelObject CreateViewmodelObject() => ViewmodelGameObject.Copy(gameObject);
    }
}