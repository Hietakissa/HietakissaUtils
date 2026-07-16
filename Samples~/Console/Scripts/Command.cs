using System.ComponentModel;
using System.Reflection;
using System;

using UnityEngine.Scripting;

namespace HietakissaUtils.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : PreserveAttribute // Derive from Preserve to make sure the methods are never stripped by Code Stripping
    {
        public readonly string Name;
        public readonly string Description;

        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public CommandAttribute(string description)
        {
            Description = description;
        }

        public CommandAttribute() { }
    }


    public class Command
    {
        public readonly string Name;
        public readonly string Description;

        public readonly ParameterInfo[] Parameters;

        public readonly Type InstanceType;
        public bool IsInstanceCommand => InstanceType != null;
        public bool HasDescription => !string.IsNullOrEmpty(Description);

        MethodInfo method;

        public Command(string name, string description, MethodInfo method, Type instanceType = null)
        {
            Name = name;
            Description = description;

            this.method = method;
            Parameters = method.GetParameters();

            InstanceType = method.IsStatic ? null : instanceType;
        }

        public bool TryExecute(string[] stringArgs, object instance = null)
        {
            if (IsInstanceCommand && instance == null)
            {
                UnityEngine.Debug.Log($"Command Error: Command '{Name}' requires an instance of type '{InstanceType.Name}' to execute.");
                return false;
            }

            if (TryProcessArgs(stringArgs, out object[] processedArgs))
            {
                method.Invoke(instance, processedArgs);
            }
            else
            {
                UnityEngine.Debug.Log($"Command Error: Invalid args '[{stringArgs.Join(", ")}]' for command '{Name}'");
                return false;
            }
            return true;
        }
        public void Execute(string[] stringArgs, object instance = null)
        {
            if (IsInstanceCommand && instance == null)
            {
                UnityEngine.Debug.Log($"Command Error: Command '{Name}' requires an instance of type '{InstanceType.Name}' to execute.");
                return;
            }

            if (TryProcessArgs(stringArgs, out object[] processedArgs))
                method.Invoke(instance, processedArgs);
            else
                UnityEngine.Debug.Log($"Command Error: Invalid args '[{stringArgs.Join(", ")}]' for command '{Name}'");
        }

        public bool TryProcessArgs(string[] stringArgs, out object[] processedArgs)
        {
            processedArgs = new object[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                ParameterInfo p = Parameters[i];

                if (i < stringArgs.Length)
                {
                    // Parse the string input to the target parameter type
                    if (TryParseArgumentOfType(stringArgs[i], p.ParameterType, out object value))
                        processedArgs[i] = value;
                    else return false;
                }
                else if (p.HasDefaultValue)
                {
                    // Fallback to default value if the user didn't supply it
                    processedArgs[i] = p.DefaultValue;
                }
                else return false;
            }
            return true;
        }

        public bool TryParseArgumentOfType(string input, Type targetType, out object value)
        {
            value = ParseArgument(input, targetType);
            return value != null;
        }
        object ParseArgument(string input, Type targetType)
        {
            // Smoothly converts strings to int, float, bool, enums, etc.
            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter != null && converter.CanConvertFrom(typeof(string)) && converter.IsValid(input))
            {
                return converter.ConvertFromString(input);
            }
            return null;
        }
    }
}