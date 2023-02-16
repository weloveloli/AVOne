// Copyright (c) 2023 Weloveloli Contributors. All rights reserved.
// See License in the project root for license information.

namespace AVOne.Tool
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Spectre.Console;
    using Spectre.Console.Rendering;

    internal static class Cli
    {
        static readonly Style StyleError = new Style(foreground: Color.Red3);
        static readonly Style StyleInfo = new Style(foreground: Color.DeepSkyBlue1);
        static readonly Style StyleSuccess = new Style(foreground: Color.Green);

        internal static void Print(string message, object value)
        {
            Info(message + "{0}", value);
        }

        internal static void Info(string message, params object[] args)
        {
            AnsiConsole.Write(new Text(string.Format(message, args), StyleInfo));
            Console.WriteLine();
        }
        internal static void Error(string message, params object[] args)
        {
            AnsiConsole.Write(new Text(string.Format(message, args), StyleError));
            Console.WriteLine();
        }
        internal static void Success(string message, params object[] args)
        {
            AnsiConsole.Write(new Text(string.Format(message, args), StyleSuccess));
            Console.WriteLine();
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

        internal static Func<T, IRenderable> Text<T>(string propertyName)
        {
            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            return (item) => new Text(Convert.ToString(property?.GetValue(item)) ?? string.Empty);
        }

        internal static (string, Func<T, IRenderable>) TextDef<T>(string propertyName)
        {
            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            return (propertyName,(item) => new Text(Convert.ToString(property?.GetValue(item)) ?? string.Empty));
        }

        internal static (string, Func<T, IRenderable>) TextPathDef<T>(string propertyName)
        {
            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            return (propertyName, (item) => new TextPath(Convert.ToString(property?.GetValue(item)) ?? string.Empty));
        }

        internal static Func<T, IRenderable> Table<T, X>(Func<T, IEnumerable<X>> func, bool addNo, params string[] propertyNames)
        {
            return (o) =>
            {
                return GetTable(func(o), addNo, propertyNames);
            };
        }

        internal static Func<T, IRenderable> CreateTextByPropertyPath<T>(string propertyPath)
        {
            return (o) =>
            {
                var type = typeof(T);
                object? value = o;
                PropertyInfo? property = null;
                bool array = false;
                while (propertyPath.Contains('.') && value != null)
                {
                    var properyName = propertyPath.Substring(0, propertyPath.IndexOf('.'));
                    propertyPath = propertyPath.Substring(propertyPath.IndexOf('.') + 1);
                    if (properyName.EndsWith("[]"))
                    {
                        properyName = properyName.Substring(0, properyName.IndexOf("[]"));
                        array = true;
                    }
                    else
                    {
                        array = false;
                    }

                    property = type.GetProperty(properyName);
                    // get property value
                    value = property?.GetValue(value);
                    type = property?.PropertyType;
                }
                if (value is not null && type is not null)
                {
                    if (!array)
                    {
                        property = type.GetProperty(propertyPath);
                        value = property?.GetValue(value);
                    }
                    else
                    {
                        if (value is IEnumerable<object> values)
                        {
                            value = values.Select(e =>
                            {
                                property = e.GetType().GetProperty(propertyPath);
                                return property?.GetValue(e);
                            });
                        }
                        else
                        {
                            value = null;
                        }
                    }

                }

                // if value is null, continue
                if (value == null)
                {
                    return new Text(string.Empty);
                }
                else if (value is string str)
                {
                    return new Text(str);
                }
                // if value is IEnumerable, add to result
                else if (value is IEnumerable<object> items)
                {
                    return new Text(string.Join('\n', items.Select(e => Convert.ToString(e) ?? string.Empty)));
                }
                return new Text(Convert.ToString(value) ?? string.Empty);
            };
        }

        internal static void PrintTable<T>(IEnumerable<T> items, bool addNo = false, params string[] propertyPaths)
        {
            var table = GetTable<T>(items, addNo, propertyPaths);
            AnsiConsole.Write(table);
        }
        internal static void PrintTable<T>(IEnumerable<T> items, bool addNo = false, params (string, Func<T, IRenderable>)[] columNamesDef)
        {
            var table = GetTable<T>(items, columNamesDef.Select(e => e.Item1), columNamesDef.Select(e => e.Item2), addNo);
            AnsiConsole.Write(table);
        }
        internal static void PrintTable<T>(IEnumerable<T> items, IEnumerable<string> columNames, IEnumerable<Func<T, IRenderable>> renderRows, bool addNo = false)
        {
            var table = GetTable<T>(items, columNames, renderRows, addNo);
            AnsiConsole.Write(table);
        }
        internal static Table GetTable<T>(IEnumerable<T> items, bool addNo = false, params string[] propertyPaths) => GetTable<T>(items, propertyPaths, propertyPaths.Select(CreateTextByPropertyPath<T>), addNo);
        internal static Table GetTable<T>(IEnumerable<T> items, IEnumerable<string> columNames, IEnumerable<Func<T, IRenderable>> renderRows, bool addNo = false)
        {
            var table = new Table();
            if (addNo)
            {
                table.AddColumn("NO.");
            }
            foreach (var name in columNames)
            {
                table.AddColumn(Markup.Escape(name));
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

            return table;
        }
    }
}
