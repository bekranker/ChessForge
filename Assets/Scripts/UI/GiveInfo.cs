using UnityEngine;

public class GiveInfo : MonoBehaviour
{
    [SerializeField] private CustomButton _customButton;
    [SerializeField] private string _infoText;

    void OnEnable()
    {
        _customButton.OnHover += () => InfoPanelController.Instance.WriteText(_infoText);
        _customButton.OnExit += () => InfoPanelController.Instance.WriteDefaultText();
    }
    void OnDisable()
    {
        _customButton.OnHover -= () => InfoPanelController.Instance.WriteText(_infoText);
        _customButton.OnExit -= () => InfoPanelController.Instance.WriteDefaultText();
    }
}