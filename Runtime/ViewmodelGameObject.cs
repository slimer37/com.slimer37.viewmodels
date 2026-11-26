using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Slimer37.Viewmodels
{
    public class ViewmodelGameObject : ViewmodelObject, IDisposable
    {
        private readonly GameObject _displayObject;

        public ViewmodelGameObject(GameObject displayObject)
        {
            _displayObject = displayObject;

            _displayObject.SetActive(false);
        }

        protected override void OnAddToViewmodel(Viewmodel viewmodel)
        {
            viewmodel.MatchViewmodelLayers(_displayObject);

            _displayObject.SetActive(true);

            _displayObject.transform.SetParent(viewmodel.transform);
            _displayObject.transform.SetLocalPositionAndRotation(Vector3.forward * 1f, Quaternion.identity);
        }

        protected override void OnRemoveFromViewmodel(Viewmodel viewmodel)
        {
            _displayObject.SetActive(false);
        }

        public static ViewmodelObject Copy(GameObject templateObject)
        {
            GameObject displayObject = Object.Instantiate(templateObject);

            return new ViewmodelGameObject(displayObject);
        }

        public void Dispose()
        {
            Object.Destroy(_displayObject);
        }
    }
}