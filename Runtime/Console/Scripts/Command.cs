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

        public void Execute(string[] stringArgs, object instance = null)
        {
            if (IsInstanceCommand && instance == null)
            {
                UnityEngine.Debug.Log($"Command Error: Command '{Name}' requires an instance of type '{InstanceType.Name}' to execute.");
                return;
            }

            object[] processedArgs = new object[Parameters.Length];

            for (int i = 0; i < Parameters.Length; i++)
            {
                ParameterInfo p = Parameters[i];

                if (i < stringArgs.Length)
                {
                    // Parse the string input to the target parameter type
                    processedArgs[i] = ParseArgument(stringArgs[i], p.ParameterType);
                }
                else if (p.HasDefaultValue)
                {
                    // Fallback to default value if the user didn't supply it
                    processedArgs[i] = p.DefaultValue;
                }
                else
                {
                    UnityEngine.Debug.Log($"Command Error: Missing required argument '{p.Name}'");
                    return;
                }
            }

            // Invoke the method. Pass null for target if it's a static method.
            // (Assuming static console commands here for simplicity)

            if (processedArgs.Length != Parameters.Length)
            {
                UnityEngine.Debug.Log($"Command Error: Argument count mismatch for command '{Name}'. Expected {Parameters.Length}, got {processedArgs.Length}.");
                return;
            }
            method.Invoke(instance, processedArgs);
        }

        public bool CanParseArgument(string input, Type targetType) => ParseArgument(input, targetType) != null;
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