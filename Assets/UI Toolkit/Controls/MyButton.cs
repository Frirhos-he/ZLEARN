using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MyButton : Button
{
    public MyButton() : base()
    {
        OnPressed = e =>
        {
            Debug.Log($"{name} button pressed");
        };
        OnReleased = e =>
        {
            Debug.Log($"{name} button released");
        };

    }

    EventCallback<PointerDownEvent> onPressed;

    EventCallback<PointerUpEvent> onReleased;

    public EventCallback<PointerDownEvent> OnPressed
    {
        get => onPressed; set
        {
            if (onPressed != null)
            {
                UnregisterCallback(onPressed, TrickleDown.TrickleDown);
            }
            onPressed = value;
            RegisterCallback(onPressed, TrickleDown.TrickleDown);
        }
    }
    public EventCallback<PointerUpEvent> OnReleased
    {
        get => onReleased; set
        {
            if (onReleased != null)
            {
                UnregisterCallback(onReleased);
            }
            onReleased = value;
            RegisterCallback(onReleased);
        }
    }

    public new class UxmlFactory : UxmlFactory<MyButton, UxmlTraits> { }
    public new class UxmlTraits : TextElement.UxmlTraits { }
}
