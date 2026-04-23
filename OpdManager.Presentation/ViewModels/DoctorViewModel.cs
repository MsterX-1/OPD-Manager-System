using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;

namespace OpdManager.Presentation.ViewModels
{
    public class DoctorViewModel : BaseViewModel
    {
        private readonly IDoctorService _doctorService;

        private ObservableCollection<Doctor> _doctors = new();
        private Doctor? _selectedDoctor;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Doctor> Doctors
        {
            get => _doctors;
            set => SetProperty(ref _doctors, value);
        }

        public Doctor? SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                SetProperty(ref _selectedDoctor, value);
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
                ToggleActiveCommand.RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                _ = FilterAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand ToggleActiveCommand { get; }

        // Event to open dialog — ViewModel stays UI-agnostic
        public event Action<Doctor?>? OpenDoctorDialog;

        public DoctorViewModel(IDoctorService doctorService)
        {
            _doctorService = doctorService;

            AddCommand = new RelayCommand(() => OpenDoctorDialog?.Invoke(null));
            EditCommand = new RelayCommand(() => OpenDoctorDialog?.Invoke(SelectedDoctor),
                                             () => SelectedDoctor != null);
            DeleteCommand = new RelayCommand(async () => await DeleteAsync(),
                                             () => SelectedDoctor != null);
            RefreshCommand = new RelayCommand(async () => await LoadAsync());
            ToggleActiveCommand = new RelayCommand(async () => await ToggleActiveAsync(),
                                                   () => SelectedDoctor != null);
        }

        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var doctors = await _doctorService.GetAllAsync();
                Doctors = new ObservableCollection<Doctor>(doctors);
            }
            finally { IsLoading = false; }
        }

        private async Task FilterAsync()
        {
            var all = await _doctorService.GetAllAsync();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? all
                : all.Where(d => d.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                 d.Specialty.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            Doctors = new ObservableCollection<Doctor>(filtered);
        }

        private async Task DeleteAsync()
        {
            if (SelectedDoctor is null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete Dr. {SelectedDoctor.Name}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            await _doctorService.DeleteAsync(SelectedDoctor.Id);
            await LoadAsync();
        }

        private async Task ToggleActiveAsync()
        {
            if (SelectedDoctor is null) return;
            SelectedDoctor.IsActive = !SelectedDoctor.IsActive;
            await _doctorService.UpdateAsync(SelectedDoctor);
            await LoadAsync();
        }
    }
}
