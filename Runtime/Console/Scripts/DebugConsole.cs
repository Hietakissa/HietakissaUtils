using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
            input = input.ToLower();
            CommandArgs commandArgs = ParseArgs(input);
            string[] args = commandArgs.Args;
            
            //string[] args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            //Debug.Log($"GetCommandsForInput: {input} => {args.Length} args");
            //if (args.Length == 0) return new List<Command>();

            //string name = args[0];
            //string name = args.Length == 0 ? input : args[0];
            string name = commandArgs.Name;
            int parameterCount = args.Length;

            // Score commands based on the criteria
            List<CommandScore> commandScores = new List<CommandScore>();
            foreach (Command command in CommandLibrary.Commands)
            {
                //Debug.Log($"Has instance for command {command.Name}: {CommandLibrary.HasInstanceForCommand(command)}");
                if (!CommandLibrary.HasInstanceForCommand(command)) continue;
                int score = 0;

                //Debug.Log($"Evaluating score for command '{command.Name}' with args: [{args.Join(", ")}]");
                // 1. Command name match with input based on levenshtein distance
                int editDistance = name.LevenshteinDistance(command.Name.ToLower());
                float similarity = 1f - (float)editDistance / Math.Max(name.Length, command.Name.Length);
                score += Mathf.FloorToInt(similarity * SCORE_FOR_NAME_MATCH);
                //Debug.Log($"Name score: {Mathf.FloorToInt(similarity * SCORE_FOR_NAME_MATCH)}, similarity: {similarity}, edit distance: {editDistance}, max length: {Math.Max(name.Length, command.Name.Length)}");

                // Only score based on parameters if the input actually has any
                if (args.Length >= 1 && command.Parameters.Length >= 1)
                {
                    // 2. Parameter count match
                    similarity = 1f - Mathf.Abs((float)(parameterCount - command.Parameters.Length) / Math.Max(parameterCount, command.Parameters.Length));
                    score += Mathf.FloorToInt(similarity * SCORE_FOR_PARAMETER_COUNT_MATCH);
                    //Debug.Log($"Parameter count score: {Mathf.FloorToInt(similarity * SCORE_FOR_PARAMETER_COUNT_MATCH)}, similarity: {similarity}, had {parameterCount} vs expected {command.Parameters.Length}");

                    // 3. Parameter type match
                    int matches = 0;
                    for (int i = 0; i < Math.Min(parameterCount, command.Parameters.Length); i++)
                    {
                        string arg = args[i];
                        //Debug.Log($"arg '{arg}' can be parsed to type {command.Parameters[i].ParameterType}: {command.TryParseArgumentOfType(arg, command.Parameters[i].ParameterType, out object v)}");
                        if (!string.IsNullOrEmpty(arg) && command.TryParseArgumentOfType(arg, command.Parameters[i].ParameterType, out object value))
                        {
                            matches++;
                        }
                    }
                    similarity = (float)matches / Math.Max(parameterCount, command.Parameters.Length);
                    score += Mathf.FloorToInt(similarity * SCORE_PER_PARAMETER_MATCH);
                    //Debug.Log($"Parameter type score: {Mathf.FloorToInt(similarity * SCORE_PER_PARAMETER_MATCH)}, similarity: {similarity}, types match for {matches}/{Math.Max(parameterCount, command.Parameters.Length)} parameters");
                }
                else if (args.Length == command.Parameters.Length) score += SCORE_FOR_PARAMETER_COUNT_MATCH;

                //Debug.Log($"Total score: {score}");
                commandScores.Add(new CommandScore(command, score));
            }

            // Sort commands by score in descending order and return the top N commands
            commandScores.Sort((a, b) => b.Score.CompareTo(a.Score));
            List<Command> bestCommands = new List<Command>();
            for (int i = 0; i < Math.Min(num, commandScores.Count); i++)
            {
                //Debug.Log($"Adding command {commandScores[i].Command.Name} with score {commandScores[i].Score}");
                bestCommands.Add(commandScores[i].Command);
            }
            return bestCommands;
        }


        public static void ExecuteCommand(Command command, params string[] args)
        {
            if (!CommandLibrary.HasInstanceForCommand(command)) return;

            if (command.IsInstanceCommand)
            {
                List<object> instances = CommandLibrary.GetInstances(command);
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

        public static void ExecuteCommand(string commandName, params string[] args)
        {
            // 1. Get all commands that match the name exactly
            // 2. Commands must also have at least as many arguments as the input
            // 3. All unspecified parameters must have a default value

            List<Command> validCommands = new List<Command>();
            foreach (Command command in CommandLibrary.Commands)
            {
                if (command.Name.ToLower() == commandName.ToLower() && command.Parameters.Length >= args.Length) validCommands.Add(command);
            }
            // Sort commands in ascending order based on parameter length, commands with fewer parameters should be prioritized, since they are more strict due to default values
            validCommands.Sort((a, b) => a.Parameters.Length.CompareTo(b.Parameters.Length));

            for (int i = 0; i < validCommands.Count; i++)
            {
                Command command = validCommands[i];
                if (command.IsInstanceCommand)
                {
                    List<object> instances = CommandLibrary.GetInstances(command);
                    bool atLeastOneSuccess = false;
                    foreach (var instance in instances)
                    {
                        if (command.TryExecute(args, instance))
                        {
                            atLeastOneSuccess = true;
                        }
                    }

                    if (atLeastOneSuccess) return; // Found and executed a valid instance command
                }
                else if (command.TryExecute(args)) return; // Executed a valid static command
            }
        }



        public static CommandArgs ParseArgs(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new CommandArgs(string.Empty, Array.Empty<string>());
            }

            var tokens = new List<string>();
            var currentToken = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"')
                {
                    // Toggle the inQuotes flag, but don't add the quote character to the token
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    // If we hit whitespace outside of quotes, the current token is finished
                    if (currentToken.Length > 0)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                    }
                }
                else
                {
                    // Add the character to the current token
                    currentToken.Append(c);
                }
            }

            // Add the last token if there is one
            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }

            // Assign the first token to Name, and the rest to Args
            return new CommandArgs(tokens.FirstOrDefault() ?? string.Empty, tokens.Skip(1).ToArray());
        }
    }

    public class CommandArgs
    {
        public readonly string Name;
        public readonly string[] Args;

        public CommandArgs(string name, string[] args)
        {
            Name = name;
            Args = args;
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