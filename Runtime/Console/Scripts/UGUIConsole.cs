using HietakissaUtils.Input;

using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;

namespace HietakissaUtils.Console
{
    public class UGUIConsole : MonoBehaviour
    {
        public static UGUIConsole Instance;

        [Header("References")]
        [SerializeField] GameObject consoleRoot;
        [SerializeField] TMP_InputField inputField;

        [SerializeField] Transform commandSuggestionRoot;
        [SerializeField] CommandSuggestion commandSuggestionPrefab;


        List<Command> matches = new List<Command>();
        int matchSelectionIndex;

        CommandSuggestion[] commandSuggestions = new CommandSuggestion[MAX_SUGGESTIONS];
        const int MAX_SUGGESTIONS = 5;


        // todo: maybe figure out a proper way to parse input string into separate parts, not splitting a "this is a string" into 4

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
            CommandLibrary.Refresh();
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
            matchSelectionIndex %= matches.Count;

            if (InputWrapper.GetKeyDown(KeyCode.Tab))
            {
                SetInputFieldText(matches[matchSelectionIndex].Name, true);
                matchSelectionIndex = 0;
                UpdateCommandSuggestions();
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
            //if (string.IsNullOrEmpty(input)) return;
            
            // Here you can update your UI to show the matches list
        }

        void OnInputFieldSubmit(string input)
        {
            // if we have matches, execute the selected one
            if (matches.Count > 0)
            {
                //DebugConsole.ExecuteCommand(matches[0], input);
                Debug.Log($"Execute command: {matches[matchSelectionIndex].Name}");
                Command command = matches[matchSelectionIndex];
                DebugConsole.ExecuteCommand(command, input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Skip(1)
                          .ToArray());
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