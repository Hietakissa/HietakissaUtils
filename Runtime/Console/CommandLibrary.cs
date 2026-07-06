using System.Collections.Generic;
using System.Reflection;
using System;

namespace HietakissaUtils.Console
{
    public static class CommandLibrary
    {
        public static HashSet<Command> Commands { get; private set; }
        static Dictionary<Type, List<object>> instanceDict;
        

        static CommandLibrary()
        {
            instanceDict = new Dictionary<Type, List<object>>();
            Commands = new HashSet<Command>();
        }


        public static void RegisterInstance(object instance)
        {
            Type instanceType = instance.GetType();
            if (!instanceDict.ContainsKey(instanceType))
            {
                instanceDict[instanceType] = new List<object>();
            }
            instanceDict[instanceType].Add(instance);
        }

        public static List<object> GetInstances(Command command)
        {
            if (instanceDict.TryGetValue(command.InstanceType, out List<object> instances))
                return instances;
            return new List<object>();
        }

        public static void Refresh()
        {
            string[] ignoredAssemblies = new string[]
            {
                "Unity",
                "System",
                "ReportGeneratorMerged",
                "unity",
                "mscorlib",
                "Mono.",
                "nunit",
                "Newtonsoft",
                "log4net",
                "Bee.B",
                "ExCSS.",
                "Microsoft.",
                "JetBrains.",
                "hk",
            };

            /// todo: nicer filtering
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                bool shouldIgnore = false;
                foreach (string ignoreAssembly in ignoredAssemblies)
                {
                    if (assembly.GetName().Name.StartsWith(ignoreAssembly))
                    {
                        shouldIgnore = true;
                        break;
                    }
                }
                if (shouldIgnore) continue;



                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        CommandAttribute attr = method.GetCustomAttribute<CommandAttribute>();
                        if (attr != null) CreateAndRegisterCommand(method, attr, type);
                    }
                }
            }

            //ParameterInfo test;
            //test.
        }

        public static bool HasInstanceForCommand(Command command)
        {
            return !command.IsInstanceCommand || (instanceDict.TryGetValue(command.InstanceType, out List<object> instances) && instances.Count > 0);
        }

        static void CreateAndRegisterCommand(MethodInfo method, CommandAttribute attr, Type enclosingType)
        {
            // Gather all appropriate data
            string name = string.IsNullOrEmpty(attr.Name) ? method.Name : attr.Name;
            string description = string.IsNullOrEmpty(attr.Description) ? string.Empty : attr.Description;
            
            // Create a new Command instance based on the data
            Command command = new Command(name, description, method, enclosingType);
            
            // Register the command
            Commands.Add(command);
        }
    }
}