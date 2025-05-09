using FlightsAircrafts.Models;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Task4Reflection.ViewModels;

namespace Task4Reflection.Models
{
    public class ReflectionHelper
    {
        public static Assembly LoadAssembly(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Указанный файл не найден.");

            return Assembly.LoadFrom(path);
        }

        public static List<string> GetAircraftsTypes(string assemblyPath)
        {
            var assembly = LoadAssembly(assemblyPath);
            return assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AbstractAircraft)))
                .Select(t => t.Name)
                .ToList();
        }

        public static List<string> GetMethods(string assemblyPath, string typeName)
        {
            var assembly = LoadAssembly(assemblyPath);
            return assembly
                .GetTypes()
                .First(t => t.Name.Equals(typeName))
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => m.ToString())
                .ToList();
        }

        public static List<ConstructorParameter> GetConstructorParameters(string assemblyPath, string typeName)
        {
            var assembly = LoadAssembly(assemblyPath);

            return assembly
                .GetTypes()
                .First(t => t.Name.Equals(typeName))
                .GetConstructors()
                .First()
                .GetParameters()
                .Select(prm => new ConstructorParameter
                    {
                        TargetType = prm.ParameterType,
                        Name = $"{prm.Name} ({prm.ParameterType.ToString().Replace("System.", "")})",
                    }
                )
                .ToList();
        }

        public static object? ExecuteMethod(object? instance, MethodInfo method)
        {
            return method.Invoke(instance, null);
        }
    }
}
