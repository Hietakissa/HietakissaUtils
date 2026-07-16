using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Debug = UnityEngine.Debug;
using UnityEngine;

namespace HietakissaUtils.Console
{
    public static class CommandLibrary
    {
        static readonly string AssemblyName = typeof(CommandAttribute).Assembly.GetName().Name;

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
            {
                instances.RemoveAll(item => item == null || (item is UnityEngine.Object obj && !obj));
                return instances;
            }
            return new List<object>();
        }

        public static void Refresh()
        {
            /*string[] ignoredAssemblies = new string[]
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

            Commands.Clear();

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
                    // Private, public, instance and static methods
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    {
                        CommandAttribute attr = method.GetCustomAttribute<CommandAttribute>();
                        if (attr != null) CreateAndRegisterCommand(method, attr, type);
                    }
                }
            }*/

            //Stopwatch sw = Stopwatch.StartNew();
            Commands.Clear();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (name == AssemblyName)
                {
                    ScanAssemblyForCommands(assembly);
                    continue;
                }

                bool referencesUs = assembly.GetReferencedAssemblies().Any(refName => refName.Name == AssemblyName);

                if (referencesUs)
                {
                    ScanAssemblyForCommands(assembly);
                }
            }
            //UnityEngine.Debug.Log($"Finished CommandLibrary refresh in {sw.ElapsedMilliseconds}ms");
        }

        static void ScanAssemblyForCommands(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                // Private, public, instance and static methods/functions
                foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    CommandAttribute attr = method.GetCustomAttribute<CommandAttribute>();
                    if (attr != null) CreateAndRegisterCommand(method, attr, type);
                }
            }
        }


        public static bool HasInstanceForCommand(Command command)
        {
            if (!command.IsInstanceCommand) return true;
            return instanceDict.TryGetValue(command.InstanceType, out List<object> instances) && instances.Count > 0;
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
            //Debug.Log($"Registered command: {command.Name}");
        }
    }
}