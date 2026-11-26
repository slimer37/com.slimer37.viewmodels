namespace Slimer37.Viewmodels
{
    public abstract class ViewmodelObject
    {
        internal void AddToViewmodel(Viewmodel viewmodel)
        {
            OnAddToViewmodel(viewmodel);
        }

        internal void RemoveFromViewmodel(Viewmodel viewmodel)
        {
            OnRemoveFromViewmodel(viewmodel);
        }

        protected abstract void OnAddToViewmodel(Viewmodel viewmodel);
        protected abstract void OnRemoveFromViewmodel(Viewmodel viewmodel);
    }
}