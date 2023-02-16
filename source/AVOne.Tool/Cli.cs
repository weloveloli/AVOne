// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;
    using Spectre.Console;
    using Spectre.Console.Rendering;
    using TagLib.Ape;

    public static class Cli
    {
        static readonly string errorMarkUp = new Style(foreground: Color.Maroon).ToMarkup();
        internal static void Error(string message)
        {
            AnsiConsole.MarkupLine($"[{errorMarkUp}]{message}[/]");
        }

        internal static void Error(string message, params object[] args)
        {
            AnsiConsole.MarkupLine($"[{errorMarkUp}]{message}[/]", args);
        }

        internal static void Exception(Exception e, string message, params object[] args)
        {
            Error(message, args);
            AnsiConsole.WriteException(e);
        }

        internal static void Exception(Exception e, string message)
        {
            Error(message);
            AnsiConsole.WriteException(e);
        }

        internal static void PrintTable<T>(IEnumerable<T> items, bool addNo = false, params string[] propertyNames)
        {
            PrintTable<T>(items, propertyNames, propertyNames.Select(CreateTextByPropertyToString<T>), addNo);
        }

        internal static Func<T, Text> CreateTextByPropertyToString<T>(string propertyName)
        {
            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            return (item) => new Text(Convert.ToString(property?.GetValue(item)) ?? string.Empty);
        }

        internal static void PrintTable<T>(IEnumerable<T> items, IEnumerable<string> columNames, IEnumerable<Func<T, IRenderable>> renderRows, bool addNo = false)
        {
            var table = new Table();
            if (addNo)
            {
                table.AddColumn("NO.");
            }
            foreach (var name in columNames)
            {
                table.AddColumn(name);
            }
            var rowNumber = 1;
            foreach (var item in items)
            {
                var rows = new List<IRenderable>();
                if (addNo)
                {
                    rows.Add(new Text(Convert.ToString(rowNumber++)));
                }
                foreach (var render in renderRows)
                {
                    rows.Add(render(item));
                }

                table.AddRow(rows.ToArray());
            }
            AnsiConsole.Write(table);
        }
    }
}
