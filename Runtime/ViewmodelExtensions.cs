namespace Slimer37.Viewmodels
{
    public static class ViewmodelExtensions
    {
        public static void Add(this Viewmodel viewmodel, ViewmodelObject obj) => obj.AddToViewmodel(viewmodel);
    }
}