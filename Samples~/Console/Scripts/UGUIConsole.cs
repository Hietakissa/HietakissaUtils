using HietakissaUtils.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace HietakissaUtils.Console
{
    public class UGUIConsole : MonoBehaviour
    {
        public static UGUIConsole Instance;

        [SerializeField] bool refreshCommandLibraryOnAwake = true;

        [Header("References")]
        [SerializeField] GameObject consoleRoot;
        [SerializeField] TMP_InputField inputField;

        [SerializeField] Transform commandSuggestionRoot;
        [SerializeField] CommandSuggestion commandSuggestionPrefab;


        List<Command> matches = new List<Command>();
        int matchSelectionIndex;

        CommandSuggestion[] commandSuggestions = new CommandSuggestion[MAX_SUGGESTIONS];
        const int MAX_SUGGESTIONS = 5;


        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of UGUIConsole detected. Destroying the old one.");
                Destroy(Instance.gameObject);
            }
            Instance = this;


            for (int i = 0; i < MAX_SUGGESTIONS; i++)
            {
                var suggestion = Instantiate(commandSuggestionPrefab, commandSuggestionRoot);
                suggestion.UpdateElement(null);
                commandSuggestions[i] = suggestion;
            }

            QOL.QOL.RefreshLayoutGroupsImmediateAndRecursive(gameObject);
            if (refreshCommandLibraryOnAwake)  CommandLibrary.Refresh();
            Close();
        }

        void Update()
        {
            if (matches.Count == 0) return;

            int indexBefore = matchSelectionIndex;
            if (InputWrapper.GetKeyDown(KeyCode.UpArrow))
            {
                matchSelectionIndex++;
            }
            else if (InputWrapper.GetKeyDown(KeyCode.DownArrow))
            {
                matchSelectionIndex--;
            }

            if (matchSelectionIndex < 0) matchSelectionIndex = matches.Count - 1;
            else matchSelectionIndex %= matches.Count;

            if (InputWrapper.GetKeyDown(KeyCode.Tab))
            {
                AutofillCommand();
            }
            
            if (indexBefore != matchSelectionIndex)
            {
                // selection changed
                UpdateCommandSuggestions();
            }
        }


        void Open_Internal()
        {
            consoleRoot.SetActive(true);

            SetInputFieldText(string.Empty, true);
            inputField.ActivateInputField();
            UpdateCommandSuggestions();
        }

        void Close_Internal()
        {
            consoleRoot.SetActive(false);
        }


        #region UI management
        void AutofillCommand()
        {
            CommandArgs commandArgs = DebugConsole.ParseArgs(inputField.text);
            // figure out how many typed out arguments match the parameters of the selected command and append those

            StringBuilder sb = new StringBuilder();
            Command selectedCommand = matches[matchSelectionIndex];

            sb.Append(selectedCommand.Name);
            int maxArgs = Mathf.Min(commandArgs.Args.Length, selectedCommand.Parameters.Length);
            for (int i = 0; i < maxArgs; i++)
            {
                // Loop until the selected command doesn't have any more
                if (i < selectedCommand.Parameters.Length)
                {
                    if (selectedCommand.TryParseArgumentOfType(commandArgs.Args[i], selectedCommand.Parameters[i].ParameterType, out _))
                        sb.Append($" {commandArgs.Args[i]}");
                    else break; // Break the instant we find an invalid arg
                }
            }

            SetInputFieldText(sb.ToString(), true);
            matchSelectionIndex = 0;
            UpdateCommandSuggestions();
        }

        void SetInputFieldText(string text, bool notify = false)
        {
            //if (notify) inputField.text = text;
            //else inputField.SetTextWithoutNotify(text);
            inputField.text = text;
            OnInputFieldValueChanged(text);
            inputField.caretPosition = text.Length;
        }

        void UpdateCommandSuggestions()
        {
            //Debug.Log($"updating suggestions");

            for (int i = 0; i < commandSuggestions.Length; i++)
            {
                //Debug.Log($"i: {i}, matches: {matches.Count}, selected: {i == matchSelectionIndex}");

                CommandSuggestion suggestion = commandSuggestions[i];
                if (i >= matches.Count)
                {
                    suggestion.UpdateElement(null);
                }
                else
                {
                    suggestion.UpdateElement(matches[i]);
                    suggestion.SetSelected(i == matchSelectionIndex);
                }
            }
            QOL.QOL.RefreshLayoutGroupsImmediateAndRecursive(gameObject);
        }
        #endregion

        #region UI Events
        void OnInputFieldValueChanged(string input)
        {
            matches = DebugConsole.GetCommandsForInput(input, MAX_SUGGESTIONS);
            matchSelectionIndex = Mathf.Min(matchSelectionIndex, matches.Count);
            UpdateCommandSuggestions();

            commandSuggestionRoot.gameObject.SetActive(matches.Count != 0);
        }

        void OnInputFieldSubmit(string input)
        {
            // if we have matches, execute the selected one
            if (matches.Count > 0)
            {
                //DebugConsole.ExecuteCommand(matches[0], input);
                //Command command = matches[matchSelectionIndex];
                /*DebugConsole.ExecuteCommand(command, input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Skip(1)
                          .ToArray());*/
                CommandArgs args = DebugConsole.ParseArgs(input);
                Debug.Log($"Execute command: {args.Name} with args: '[{args.Args.Join(", ")}]'");
                DebugConsole.ExecuteCommand(args.Name, args.Args);
            }
            else
            {
                Debug.Log($"No command matches the input: {input}");
            }

            SetInputFieldText(string.Empty, true);
            inputField.ActivateInputField();
        }
        #endregion

        void OnEnable()
        {
            inputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            inputField.onSubmit.AddListener(OnInputFieldSubmit);
        }

        void OnDisable()
        {
            inputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
            inputField.onSubmit.RemoveListener(OnInputFieldSubmit);
        }

        public static void Open() => Instance?.Open_Internal();
        public static void Close() => Instance?.Close_Internal();
    }
}