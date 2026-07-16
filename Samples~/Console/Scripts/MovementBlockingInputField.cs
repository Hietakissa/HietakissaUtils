using UnityEngine;
using TMPro;

namespace HietakissaUtils
{
    public class MovementBlockingInputField : TMP_InputField
    {
        [SerializeField] bool preventVerticalArrowMovement;
        [SerializeField] bool preventHorizontalArrowMovement;

        void OnGUI()
        {
            Event evt = Event.current;
            if (IsActive() && (preventVerticalArrowMovement && EventIsVerticalArrowInput(evt) || preventHorizontalArrowMovement && EventIsHorizontalArrowInput(evt))) evt.Use();
        }

        bool EventIsVerticalArrowInput(Event evt) => evt.isKey && (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow);
        bool EventIsHorizontalArrowInput(Event evt) => evt.isKey && (evt.keyCode == KeyCode.LeftArrow || evt.keyCode == KeyCode.RightArrow);
    }
}