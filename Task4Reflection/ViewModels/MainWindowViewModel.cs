using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Task4Reflection.Models;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Text.RegularExpressions;
using DynamicData;

namespace Task4Reflection.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {

        private static readonly Regex _numericRegex = new(@"^\d+(\.\d+)?$", RegexOptions.Compiled);
        private static readonly Regex _detoriationLevelRegex = new(@"^(?:\d{1,2}(?:\.\d+)?|0\.(?!0+$)\d+)$", RegexOptions.Compiled);
        private readonly Window _mainWindow;
        private string _assemblyPath = string.Empty;
        private List<string> _aircraftTypes = new();
        private string _selectedAircraftType;
        private List<string> _methods = new();
        private string _selectedMethod;
        private string _result;


        public System.Collections.ObjectModel.ObservableCollection<ConstructorParameter> AircraftConstructorParameters { get;  } = new();
        public System.Collections.ObjectModel.ObservableCollection<ConstructorParameter> AirportConstructorParameters { get; } = new();

        public bool IsMethodSelected 
        {
            get => !string.IsNullOrEmpty(_selectedMethod);
        }

        public string AssemblyPath
        {
            get => _assemblyPath;
            set => this.RaiseAndSetIfChanged(ref _assemblyPath, value);
        }

        public List<string> AircraftTypes
        {
            get => _aircraftTypes;
            set => this.RaiseAndSetIfChanged(ref _aircraftTypes, value);
        }

        public void AddInputsForAircraftFields()
        {
            AircraftConstructorParameters.Clear();
            if (string.IsNullOrEmpty(_selectedAircraftType))
                return;

            AircraftConstructorParameters.AddRange(ReflectionHelper.GetConstructorParameters(_assemblyPath, SelectedAircraftType));
            this.RaisePropertyChanged(nameof(AircraftConstructorParameters));            
        }

        public void AddInputsForAirportFields()
        {
            if (string.IsNullOrEmpty(_assemblyPath))
                return;
            
            AirportConstructorParameters.Clear();

            AirportConstructorParameters.AddRange(ReflectionHelper.GetConstructorParameters(_assemblyPath, "Airport"));
            this.RaisePropertyChanged(nameof(AirportConstructorParameters));           
        }

        public string SelectedAircraftType
        {
            get => _selectedAircraftType;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedAircraftType, value);
                AddInputsForAircraftFields();
                LoadMethods();

            }
        }

        public bool IsAssemblySelected { get => !string.IsNullOrEmpty(_assemblyPath); }

        public List<string> Methods
        {
            get => _methods;
            set => this.RaiseAndSetIfChanged(ref _methods, value);
        }

        public string SelectedMethod
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
                AircraftTypes = ReflectionHelper.GetAircraftsTypes(_assemblyPath);
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


        private void LoadMethods()
        {
            if (SelectedAircraftType == null) return;
            Methods = ReflectionHelper.GetMethods(_assemblyPath, SelectedAircraftType);
        }

        public void ExecuteMethod()
        {
            try
            {
                var airportParams = AirportConstructorParameters.Select(p => p.TryGetValue()).ToArray();
                var aircraftParams = AircraftConstructorParameters.Select(p => p.TryGetValue()).ToArray();
                object? result = ReflectionHelper.ExecuteMethod(_assemblyPath, _selectedAircraftType, _selectedMethod, aircraftParams, airportParams);
                Result = $"Метод выполнен. Результат: {result ?? "void"}";
            }
            catch (Exception ex)
            {
                Result = $"Ошибка: {ex.InnerException?.Message ?? ex.Message}";
            }
        }
    }
}
