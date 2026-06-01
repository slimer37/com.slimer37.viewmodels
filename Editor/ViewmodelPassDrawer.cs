using UnityEditor.Rendering.HighDefinition;

namespace Slimer37.Viewmodel.Rendering.Editor
{
#if UNITY_EDITOR
    [CustomPassDrawer(typeof(ViewmodelPass))]
    internal class ViewmodelPassEditor : CustomPassDrawer
    {
        protected override PassUIFlag commonPassUIFlags => PassUIFlag.Name;
    }
#endif
}