using UnityEngine.UIElements;

public interface IPanelController
{
    void Initialize(VisualElement panel, VisualElement contextLayer = null);
    void NotifyPanelOpened();
    void NotifyPanelClosed();
}