using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// �����w����ʂ�ViewModel
    /// </summary>
    public class PropertyPurchaseViewModel : INotifyPropertyChanged
    {
        private Station _station;
        private Player _player;
        private Property _selectedProperty;
        private bool _canPurchase;
        private string _message;

        // �v���p�e�B
        public ObservableCollection<PropertyItemViewModel> Properties { get; set; }

        public string StationName => _station?.Name ?? "�w";
        public string PlayerName => _player?.Name ?? "�v���C���[";
        public Money PlayerMoney => _player?.CurrentMoney ?? Money.Zero;

        public Property SelectedProperty
        {
            get => _selectedProperty;
            set
            {
                if (SetProperty(ref _selectedProperty, value))
                {
                    UpdateCanPurchase();
                }
            }
        }

        public bool CanPurchase
        {
            get => _canPurchase;
            private set => SetProperty(ref _canPurchase, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        // �R�}���h
        public ICommand PurchaseCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand SelectPropertyCommand { get; private set; }

        // �C�x���g
        public event EventHandler<Property> PropertyPurchased;
        public event EventHandler Cancelled;

        // �R���X�g���N�^
        public PropertyPurchaseViewModel()
        {
            Properties = new ObservableCollection<PropertyItemViewModel>();
            InitializeCommands();
        }

        /// <summary>
        /// ������
        /// </summary>
        public void Initialize(Station station, Player player)
        {
            _station = station ?? throw new ArgumentNullException(nameof(station));
            _player = player ?? throw new ArgumentNullException(nameof(player));

            LoadProperties();
            UpdateCanPurchase();

            Message = $"{station.Name}�̕�����I��ł�������";
        }

        private void InitializeCommands()
        {
            PurchaseCommand = new RelayCommand(Purchase, () => CanPurchase);
            CancelCommand = new RelayCommand(Cancel);
            SelectPropertyCommand = new RelayCommand<PropertyItemViewModel>(SelectProperty);
        }

        private void LoadProperties()
        {
            Properties.Clear();

            if (_station?.Properties == null) return;

            foreach (var property in _station.Properties.Where(p => p.Owner == null))
            {
                var itemVm = new PropertyItemViewModel
                {
                    Property = property,
                    Name = property.Name,
                    Category = property.Category.ToString(),
                    Price = property.CurrentPrice,
                    IncomeRate = $"{property.IncomeRate:P0}",
                    ExpectedIncome = property.CalculateIncome(),
                    CategoryColor = property.GetCategoryColor(),
                    CanAfford = _player.CurrentMoney >= property.CurrentPrice
                };

                Properties.Add(itemVm);
            }

            // �w���ςݕ������\���i�I��s�j
            foreach (var property in _station.Properties.Where(p => p.Owner != null))
            {
                var itemVm = new PropertyItemViewModel
                {
                    Property = property,
                    Name = property.Name,
                    Category = property.Category.ToString(),
                    Price = property.CurrentPrice,
                    IncomeRate = $"{property.IncomeRate:P0}",
                    ExpectedIncome = property.CalculateIncome(),
                    CategoryColor = property.GetCategoryColor(),
                    CanAfford = false,
                    IsPurchased = true,
                    OwnerName = property.Owner.Name
                };

                Properties.Add(itemVm);
            }
        }

        private void UpdateCanPurchase()
        {
            if (SelectedProperty == null)
            {
                CanPurchase = false;
                Message = "������I�����Ă�������";
                return;
            }

            if (SelectedProperty.Owner != null)
            {
                CanPurchase = false;
                Message = $"���̕�����{SelectedProperty.Owner.Name}�����L���Ă��܂�";
                return;
            }

            if (_player.CurrentMoney < SelectedProperty.CurrentPrice)
            {
                CanPurchase = false;
                var shortage = SelectedProperty.CurrentPrice - _player.CurrentMoney;
                Message = $"������{shortage}�s�����Ă��܂�";
                return;
            }

            CanPurchase = true;
            Message = $"{SelectedProperty.Name}���w�����܂����H";

            // �Ɛ�\���`�F�b�N
            var ownedCount = _station.Properties.Count(p => p.Owner == _player);
            var totalCount = _station.Properties.Count;

            if (ownedCount == totalCount - 1)
            {
                Message += "\n�Ɛ�B���܂ł���1���I";
            }
        }

        private void SelectProperty(PropertyItemViewModel item)
        {
            if (item != null && !item.IsPurchased)
            {
                SelectedProperty = item.Property;

                // �I����Ԃ��X�V
                foreach (var prop in Properties)
                {
                    prop.IsSelected = prop == item;
                }
            }
        }

        private void Purchase()
        {
            if (!CanPurchase || SelectedProperty == null) return;

            PropertyPurchased?.Invoke(this, SelectedProperty);
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        // INotifyPropertyChanged����
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// �����A�C�e��ViewModel
    /// </summary>
    public class PropertyItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public Property Property { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public Money Price { get; set; }
        public string IncomeRate { get; set; }
        public Money ExpectedIncome { get; set; }
        public string CategoryColor { get; set; }
        public bool CanAfford { get; set; }
        public bool IsPurchased { get; set; }
        public string OwnerName { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public double Opacity => IsPurchased || !CanAfford ? 0.5 : 1.0;

        // INotifyPropertyChanged����
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(Opacity));
            return true;
        }
    }

    /// <summary>
    /// �p�����[�^�t��RelayCommand
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}