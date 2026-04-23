using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using OpdManager.Application.Services.Interfaces;
using OpdManager.Domain.Entities;
using OpdManager.Presentation.Core;

namespace OpdManager.Presentation.ViewModels
{
    public class AddEditDoctorViewModel : BaseViewModel
    {
        private readonly IDoctorService _doctorService;

        // Form fields
        private int _id;
        private string _name = string.Empty;
        private string _specialty = string.Empty;
        private string? _phoneNumber;
        private TimeSpan _startTime = new(8, 0, 0);
        private TimeSpan _endTime = new(17, 0, 0);
        private bool _isActive = true;
        private bool _isEditMode;
        private string _title = "Add Doctor";

        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value); SaveCommand.RaiseCanExecuteChanged(); }
        }
        public string Specialty
        {
            get => _specialty;
            set { SetProperty(ref _specialty, value); SaveCommand.RaiseCanExecuteChanged(); }
        }
        public string? PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }
        public TimeSpan StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }
        public TimeSpan EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        // Time slot options for ComboBox
        public ObservableCollection<TimeSpan> TimeOptions { get; } = new();

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        // Events — dialog result
        public event Action? OnSaved;
        public event Action? OnCancelled;

        public AddEditDoctorViewModel(IDoctorService doctorService)
        {
            _doctorService = doctorService;

            SaveCommand = new RelayCommand(async () => await SaveAsync(),
                                             () => !string.IsNullOrWhiteSpace(Name) &&
                                                   !string.IsNullOrWhiteSpace(Specialty));
            CancelCommand = new RelayCommand(() => OnCancelled?.Invoke());

            GenerateTimeOptions();
        }

        private void GenerateTimeOptions()
        {
            // Generate every 30 min from 00:00 to 23:30
            for (int h = 0; h < 24; h++)
                for (int m = 0; m < 60; m += 30)
                    TimeOptions.Add(new TimeSpan(h, m, 0));
        }

        public void LoadDoctor(Doctor doctor)
        {
            _isEditMode = true;
            Title = "Edit Doctor";
            _id = doctor.Id;
            Name = doctor.Name;
            Specialty = doctor.Specialty;
            PhoneNumber = doctor.PhoneNumber;
            StartTime = doctor.StartTime;
            EndTime = doctor.EndTime;
            IsActive = doctor.IsActive;
        }

        private async Task SaveAsync()
        {
            if (_isEditMode)
            {
                await _doctorService.UpdateAsync(new Doctor
                {
                    Id = _id,
                    Name = Name,
                    Specialty = Specialty,
                    PhoneNumber = PhoneNumber,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    IsActive = IsActive
                });
            }
            else
            {
                await _doctorService.AddAsync(new Doctor
                {
                    Name = Name,
                    Specialty = Specialty,
                    PhoneNumber = PhoneNumber,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    IsActive = IsActive
                });
            }

            OnSaved?.Invoke();
        }
    }
}
