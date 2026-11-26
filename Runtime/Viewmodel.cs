using System.Collections.Generic;
using UnityEngine;

namespace Slimer37.Viewmodels
{
    public class Viewmodel : MonoBehaviour
    {
        [SerializeField, Layer] private int _renderLayer;

        private readonly Dictionary<IViewmodelObjectSource, ViewmodelObject> _viewmodelObjects = new();

        internal int GetViewmodelLayer() => _renderLayer;

        public void AddObject(IViewmodelObjectSource source)
        {
            ViewmodelObject obj = source.CreateViewmodelObject();

            _viewmodelObjects.Add(source, obj);

            obj.AddToViewmodel(this);
        }

        public void RemoveObject(IViewmodelObjectSource target)
        {
            if (!_viewmodelObjects.TryGetValue(target, out ViewmodelObject obj))
            {
                throw new KeyNotFoundException();
            }
            
            obj.RemoveFromViewmodel(this);
        }

        public void MatchViewmodelLayers(GameObject gameObject)
        {
            int layer = GetViewmodelLayer();

            foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }
    }
}