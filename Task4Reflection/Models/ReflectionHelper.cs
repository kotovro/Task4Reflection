using FlightsAircrafts.Models;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Linq;

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

        public static List<Type> GetAircraftsTypes(Assembly assembly)
        {
            Type[] allTypes = assembly.GetTypes();
            List<Type> airplaneTypes = new List<Type>();

            foreach (Type type in allTypes)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(AbstractAircraft)))
                    airplaneTypes.Add(type);
            }
            return airplaneTypes;
        }

        public static Type GetAirportType(Assembly assembly)
        {
            Type[] allTypes = assembly.GetTypes();
            Type airportType = null;

            foreach (Type type in allTypes)
            {
                if (type.IsClass && !type.IsAbstract && type.FullName.Contains("Airport")) 
                    airportType = type;
            }
            return airportType;
        }

        public static List<MethodInfo> GetMethods(Type type)
        {
            MethodInfo[] allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            List<MethodInfo> methods = new List<MethodInfo>();

            foreach (MethodInfo method in allMethods)
            {
                if (!method.IsSpecialName)
                    methods.Add(method);
            }
            return methods;
        }

        
        public static List<ConstructorInfo> GetConstructors(Type type)
        {
            return type.GetConstructors().ToList();
        }

        public static object? ExecuteMethod(object? instance, MethodInfo method)
        {
            return method.Invoke(instance, null);
        }

        public static object? CreateAirplaneInstance(Type type, string name, double detoriationLevel, double ceil, double verticalSpeed, double runwayLength, double landingLength, double fuelLevel)
        {
            var ctor = type.GetConstructor(new[] { typeof(string), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double) });
            return Activator.CreateInstance(type, name, detoriationLevel, ceil, verticalSpeed, runwayLength,   landingLength, fuelLevel);
        }

        public static object? CreateHelicopterInstance(Type type, string name, double detoriationLevel, double ceil, double verticalSpeed, double fuelLevel)
        {
            var ctor = type.GetConstructor(new[] { typeof(string), typeof(double), typeof(double), typeof(double), typeof(double), });
            return Activator.CreateInstance(type, name, detoriationLevel, ceil, verticalSpeed,  fuelLevel);
        }

        public object? CreateInstance(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(string), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double)});
            return Activator.CreateInstance(type, "testcp", 10, 10, 10, 10, 10, 10);
        }

    }
}
