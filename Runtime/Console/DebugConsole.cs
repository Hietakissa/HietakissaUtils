using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace HietakissaUtils.Console
{
    public static class DebugConsole
    {
        /*
        Events that any UI frontend can listen to
        public static event Action<LogEntry> OnLogAdded;
        public static event Action OnLogsCleared;
         */



        public static void Watch(string key, object value)
        {
            
        }

        public static void Bind(string key, Func<object> getter)
        {

        }


        public static List<Command> GetCommandsForInput(string input, int num = 5)
        {
            // Total max score 100 for a perfect match with all criteria
            const int SCORE_FOR_NAME_MATCH = 60;
            const int SCORE_FOR_PARAMETER_COUNT_MATCH = 15;
            const int SCORE_PER_PARAMETER_MATCH = 25;

            // For overloading (when executing a command based on input), we can use the following criteria to sort the commands:
            // 1. Parameter count, fewer parameters is preferred to handle cases with default parameter values

            string[] args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args.Length == 0) return new List<Command>();

            string name = args[0];
            int parameterCount = args.Length - 1;

            // Score commands based on the criteria
            List<CommandScore> commandScores = new List<CommandScore>();
            foreach (Command command in CommandLibrary.Commands)
            {
                if (!CommandLibrary.HasInstanceForCommand(command)) continue;
                int score = 0;

                // 1. Command name match with input based on levenshtein distance
                int editDistance = name.LevenshteinDistance(command.Name);
                float similarity = 1f - (float)editDistance / Math.Max(name.Length, command.Name.Length);
                score += Mathf.FloorToInt(similarity * SCORE_FOR_NAME_MATCH);

                // 2. Parameter count match
                similarity = 1f - Mathf.Abs((float)(parameterCount - command.Parameters.Length) / Math.Max(parameterCount, command.Parameters.Length));
                score += Mathf.FloorToInt(similarity * SCORE_FOR_PARAMETER_COUNT_MATCH);
                //if (parameterCount == command.Parameters.Length)
                //    score += SCORE_FOR_PARAMETER_COUNT_MATCH;

                // 3. Parameter type match
                int matches = 0;
                for (int i = 0; i < Math.Min(parameterCount, command.Parameters.Length); i++)
                {
                    string arg = args[i + 1];
                    if (!string.IsNullOrEmpty(arg) && command.CanParseArgument(arg, command.Parameters[i].ParameterType))
                    {
                        //score += SCORE_PER_PARAMETER_MATCH;
                        matches++;
                    }
                }
                similarity = (float)matches / Math.Max(parameterCount, command.Parameters.Length);
                score += Mathf.FloorToInt(similarity * SCORE_PER_PARAMETER_MATCH);

                commandScores.Add(new CommandScore(command, score));
            }

            // Sort commands by score in descending order and return the top N commands
            commandScores.Sort((a, b) => b.Score.CompareTo(a.Score));
            List<Command> bestCommands = new List<Command>();
            for (int i = 0; i < Math.Min(num, commandScores.Count); i++)
            {
                bestCommands.Add(commandScores[i].Command);
            }
            return bestCommands;
        }


        public static void ExecuteCommand(Command command, string[] args)
        {
            if (!CommandLibrary.HasInstanceForCommand(command)) return;

            if (command.IsInstanceCommand)
            {
                // validate all instances
                List<object> instances = CommandLibrary.GetInstances(command);
                instances.RemoveAll(item => item == null || (item is UnityEngine.Object obj && !obj));
                foreach (var instance in instances)
                {
                    command.Execute(args, instance);
                }
            }
            else
            {
                command.Execute(args);
            }
        }
    }

    struct CommandScore
    {
        public readonly Command Command;
        public readonly int Score;
        public CommandScore(Command command, int score)
        {
            Command = command;
            Score = score;
        }
    }
}