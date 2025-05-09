using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using System.Reflection;
using Task4Reflection.Models;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Avalonia.Data;

namespace Task4Reflection.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {

        private static readonly Regex _numericRegex = new(@"^\d+(\.\d+)?$", RegexOptions.Compiled);
        private static readonly Regex _detoriationLevelRegex = new(@"^(?:\d{1,2}(?:\.\d+)?|0\.(?!0+$)\d+)$", RegexOptions.Compiled);
        private readonly Window _mainWindow;
        private string _assemblyPath = string.Empty;
        private List<Type> _aircraftTypes = new();
        private Type _selectedAircraftType;
        private List<MethodInfo> _methods = new();
        private MethodInfo _selectedMethod;
        private object? _instance;
        private ConstructorInfo _constrInfo;
        private string _result;
        private Type _airportType;
        private Assembly _assembly;


        public System.Collections.ObjectModel.ObservableCollection<ConstructorParameter> AircraftConstructorParameters { get;  } = new();
        public System.Collections.ObjectModel.ObservableCollection<ConstructorParameter> AirportConstructorParameters { get; } = new();

        public bool IsMethodSelected 
        {
            get => _selectedMethod != null;
        }

        public string AssemblyPath
        {
            get => _assemblyPath;
            set => this.RaiseAndSetIfChanged(ref _assemblyPath, value);
        }

        public List<Type> AircraftTypes
        {
            get => _aircraftTypes;
            set => this.RaiseAndSetIfChanged(ref _aircraftTypes, value);
        }

        public void AddInputsForAircraftFields()
        {
            AircraftConstructorParameters.Clear();
            var constructors = ReflectionHelper.GetConstructors(SelectedType);
            if (constructors.Count == 1)
            {
                _constrInfo = constructors.ElementAt(0);
                foreach (ParameterInfo prm in _constrInfo.GetParameters())
                {
                    var item = new ConstructorParameter { TargetType = prm.ParameterType, Name = $"{prm.Name} ({prm.ParameterType.ToString().Replace("System.", "")})", };
                    AircraftConstructorParameters.Add(item);
                }

                this.RaisePropertyChanged(nameof(AircraftConstructorParameters));           
            }
        }

        public void AddInputsForAirportFields()
        {
            AirportConstructorParameters.Clear();
            var constructors = ReflectionHelper.GetConstructors(ReflectionHelper.GetAirportType(_assembly));
            if (constructors.Count == 1)
            {
                _constrInfo = constructors.ElementAt(0);
                foreach (ParameterInfo prm in _constrInfo.GetParameters())
                {
                    var item = new ConstructorParameter { TargetType = prm.ParameterType, Name = $"{prm.Name} ({prm.ParameterType.ToString().Replace("System.", "")})", };
                    AirportConstructorParameters.Add(item);
                }

                this.RaisePropertyChanged(nameof(AirportConstructorParameters));
            }
        }

        public Type SelectedType
        {
            get => _selectedAircraftType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAircraftType, value);
                AddInputsForAircraftFields();
            }
        }

        public bool IsAssemblySelected
        {
            get
            {
                AddInputsForAirportFields();
                return !string.IsNullOrEmpty(_assemblyPath);
            } 

        }

        public List<MethodInfo> Methods
        {
            get => _methods;
            set => this.RaiseAndSetIfChanged(ref _methods, value);
        }

        public MethodInfo SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedMethod, value);
                this.RaisePropertyChanged(nameof(IsMethodSelected));
            }
        }

        public string Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        public MainWindowViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public static FilePickerFileType DLL { get; } = new("Libraries")
        {
            Patterns = new[] { "*.dll" },
        };

        public async void OpenDialog()
        {
            var topLevel = TopLevel.GetTopLevel(_mainWindow);

            var file = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Text File",
                AllowMultiple = false,
                FileTypeFilter = new[] { DLL },

            });
            try
            {
                if (file is not null)
                {
                    AssemblyPath = file.First().Path.ToString().Split(new string[] { "file:///" }, StringSplitOptions.None)[1] ?? string.Empty;

                }

            }
            catch (Exception e)
            { 
                
            }
            LoadAssembly();
            this.RaisePropertyChanged(nameof(IsAssemblySelected));
        }
        public void LoadAssembly()
        {
            try
            {
                _assembly = ReflectionHelper.LoadAssembly(AssemblyPath);
                AircraftTypes = ReflectionHelper.GetAircraftsTypes(_assembly);
                Result = "Сборка загружена успешно!";
            }
            catch (Exception ex)
            {
                Result = $"Ошибка: {ex.Message}";
            }
        }

        public void CreateInstance()
        {
            _constrInfo.Invoke(AircraftConstructorParameters.Select(param => param.TryGetValue()).ToArray());
        }
        private void LoadMethods()
        {
            if (SelectedType == null) return;
            Methods = ReflectionHelper.GetMethods(SelectedType);
        }

        public void ExecuteMethod()
        {
            try
            {
                object? result = ReflectionHelper.ExecuteMethod(_instance, SelectedMethod);
                Result = $"Метод выполнен. Результат: {result ?? "void"}";
            }
            catch (Exception ex)
            {
                Result = $"Ошибка: {ex.InnerException?.Message ?? ex.Message}";
            }
        }
    }
}
