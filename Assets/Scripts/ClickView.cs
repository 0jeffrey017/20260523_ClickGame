using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class ClickView : MonoBehaviour, IPointerClickHandler
{
    private MainGamePresenter _mainGamePresenter;
    
    public Action OnClick = delegate { };

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pointer Clicked");
        OnClick.Invoke();
    }
}
