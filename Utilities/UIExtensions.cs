using UnityEngine.UIElements;

namespace Utilities
{
    public static class UIExtensions 
    {
        public static void Display(this VisualElement element, bool enabled)
        {
            element.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }
   
        public static void ChangeMenu(VisualElement newMenu, ref VisualElement currentUI)
        {
            currentUI?.Display(false);
            newMenu.Display(true);
            currentUI = newMenu;
        }

    }
}
