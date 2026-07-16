using System.Linq;
using System;

#if ENABLE_LEGACY_INPUT_MANAGER
namespace HietakissaUtils.Input
{
    using HietakissaUtils.Serialization;

    using System.Collections.Generic;

    using UnityEngine;
    // todo: separate this into its own file
    using Input = UnityEngine.Input;
    public static class ControlRebinding
    {
        static Dictionary<string, KeyCode> bindings;

        static KeyCode[] validKeycodes;
        static KeyCode keyCode;

        public static bool binding { get; private set; }
        static string bindingKeyName;

        public static event Action OnKeyRebound;

        public static void SetValidKeycodes(bool includeController = false)
        {
            bindings = new Dictionary<string, KeyCode>();
            validKeycodes = Enum.GetValues(typeof(KeyCode))
                .Cast<KeyCode>()
                .Where(k => !includeController ? (int)k < 330 : true)
                .ToArray();
        }

        public static KeyCode GetPressedKey()
        {
            if (!Input.anyKeyDown) return KeyCode.None;

            for (int i = 0; i < validKeycodes.Length; i++)
            {
                keyCode = validKeycodes[i];
                if (Input.GetKeyDown(keyCode)) return keyCode;
            }

            return KeyCode.None;
        }

        public static KeyCode GetKeyWithName(string name)
        {
            return bindings[name];
        }

        public static void StartBinding(string name)
        {
            binding = true;
            bindingKeyName = name;
        }

        public static void HandleBinding()
        {
            KeyCode key = GetPressedKey();

            if (key != KeyCode.None)
            {
                EditBinding(bindingKeyName, key);
                binding = false;
            }
        }

        public static void EditBinding(string name, KeyCode key)
        {
            bindings[name] = key;
            OnKeyRebound?.Invoke();
        }

        public static void SaveBindings()
        {
            Serializer.SaveGlobal(bindings, "ControlBindings");
        }

        public static void LoadBindings()
        {
            if (Serializer.LoadGlobal(out bindings, "ControlBindings"))
            {
                OnKeyRebound?.Invoke();
            }
        }
    }
}
#endif