using UnityEngine;
using TMPro;

using System.Text;
using UnityEngine.UI;

namespace HietakissaUtils.Console
{
    public class CommandSuggestion : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI commandNameText;
        [SerializeField] TextMeshProUGUI commandDescriptionText;

        [SerializeField] Image backgroundImage;

        Command command;
        bool isVisible = true;
        bool isInitialized;


        void Show()
        {
            isVisible = true;
            gameObject.SetActive(isVisible);
        }

        void Hide()
        {
            isVisible = false;
            gameObject.SetActive(isVisible);
        }

        public void UpdateElement(Command command)
        {
            if (isInitialized && this.command == command) return;
            this.command = command;
            isInitialized = true;

            if (command == null)
            {
                if (isVisible) Hide();
                return;
            }
            else if (!isVisible) Show();


            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < command.Parameters.Length; i++)
            {
                var param = command.Parameters[i];
                sb.Append($"<{param.Name}>");
                if (i < command.Parameters.Length - 1)
                {
                    sb.Append(" ");
                }
            }

            commandNameText.text = command.Name + sb.ToString();
            commandDescriptionText.text = command.Description;
        }

        public void SetSelected(bool selected)
        {
            backgroundImage.enabled = selected;
        }
    }
}