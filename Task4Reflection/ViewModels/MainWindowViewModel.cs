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
using DynamicData;

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
        private ConstructorInfo _aircraftConstrInfo;
        private ConstructorInfo _airportConstrInfo;
        private string _result;
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
            if (_selectedAircraftType == null)
                return;
            var constructors = ReflectionHelper.GetConstructors(SelectedAircraftType);

            _aircraftConstrInfo = constructors.FirstOrDefault();
            AircraftConstructorParameters.AddRange(
                _aircraftConstrInfo.GetParameters()
                .Select(prm => new ConstructorParameter
                    {
                        TargetType = prm.ParameterType,
                        Name = $"{prm.Name} ({prm.ParameterType.ToString().Replace("System.", "")})",
                    }
                )
            );
            this.RaisePropertyChanged(nameof(AircraftConstructorParameters));            
        }

        public void AddInputsForAirportFields()
        {
            if (_assembly == null)
                return;
            
            AirportConstructorParameters.Clear();

            var constructors = ReflectionHelper.GetConstructors(ReflectionHelper.GetAirportType(_assembly));
          
            _airportConstrInfo = constructors.FirstOrDefault();
            AirportConstructorParameters.AddRange(
                _airportConstrInfo.GetParameters()
                .Select(prm => new ConstructorParameter 
                    { 
                        TargetType = prm.ParameterType, 
                        Name = $"{prm.Name} ({prm.ParameterType.ToString().Replace("System.", "")})", 
                    }
                )
            );
            this.RaisePropertyChanged(nameof(AirportConstructorParameters));           
        }

        public Type SelectedAircraftType
        {
            get => _selectedAircraftType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAircraftType, value);
                AddInputsForAircraftFields();
            }
        }

        public bool IsAssemblySelected { get => !string.IsNullOrEmpty(_assemblyPath); }

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
                AddInputsForAirportFields();
                Result = "";
                if (AircraftTypes.Count == 0)
                    Result += "В сборке не найдены типы Aircraft\n";

                if (AirportConstructorParameters.Count == 0)
                    Result += "В сборке не найден тип Airport\n";
                if (string.IsNullOrEmpty(Result))
                    Result = "Сборка загружена успешно";
            }
            catch (Exception ex)
            {
                Result = $"Ошибка: {ex.Message}";
            }
        }

        public void CreateInstance()
        {
            _aircraftConstrInfo.Invoke(AircraftConstructorParameters.Select(param => param.TryGetValue()).ToArray());
        }
        private void LoadMethods()
        {
            if (SelectedAircraftType == null) return;
            Methods = ReflectionHelper.GetMethods(SelectedAircraftType);
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
