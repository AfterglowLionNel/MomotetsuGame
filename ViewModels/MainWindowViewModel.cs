using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // �v���C�x�[�g�t�B�[���h
        private int _currentYear = 1;
        private int _currentMonth = 4;
        private string _destination = "����";
        private string _currentMessage = "�Q�[�����J�n���Ă�������";
        private bool _canRollDice = true;
        private bool _canEndTurn = false;

        // �v���p�e�B
        public ObservableCollection<StationViewModel> Stations { get; set; }
        public ObservableCollection<PlayerViewModel> Players { get; set; }
        public ObservableCollection<PlayerInfoViewModel> PlayerInfos { get; set; }

        public int CurrentYear
        {
            get => _currentYear;
            set => SetProperty(ref _currentYear, value);
        }

        public int CurrentMonth
        {
            get => _currentMonth;
            set => SetProperty(ref _currentMonth, value);
        }

        public string Destination
        {
            get => _destination;
            set => SetProperty(ref _destination, value);
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }

        public bool CanRollDice
        {
            get => _canRollDice;
            set => SetProperty(ref _canRollDice, value);
        }

        public bool CanEndTurn
        {
            get => _canEndTurn;
            set => SetProperty(ref _canEndTurn, value);
        }

        // �R�}���h
        public ICommand RollDiceCommand { get; private set; }
        public ICommand EndTurnCommand { get; private set; }
        public ICommand SaveGameCommand { get; private set; }
        public ICommand LoadGameCommand { get; private set; }
        public ICommand ShowSettingsCommand { get; private set; }

        // �R���X�g���N�^
        public MainWindowViewModel()
        {
            InitializeCollections();
            InitializeCommands();
            InitializeSampleData();
        }

        private void InitializeCollections()
        {
            Stations = new ObservableCollection<StationViewModel>();
            Players = new ObservableCollection<PlayerViewModel>();
            PlayerInfos = new ObservableCollection<PlayerInfoViewModel>();
        }

        private void InitializeCommands()
        {
            RollDiceCommand = new RelayCommand(RollDice, () => CanRollDice);
            EndTurnCommand = new RelayCommand(EndTurn, () => CanEndTurn);
            SaveGameCommand = new RelayCommand(SaveGame);
            LoadGameCommand = new RelayCommand(LoadGame);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
        }

        private void InitializeSampleData()
        {
            // �T���v���w�f�[�^
            Stations.Add(new StationViewModel { X = 400, Y = 300, ShortName = "����", TypeColor = "#FF0000" });
            Stations.Add(new StationViewModel { X = 350, Y = 280, ShortName = "�V�h", TypeColor = "#00FF00" });
            Stations.Add(new StationViewModel { X = 450, Y = 320, ShortName = "�i��", TypeColor = "#0000FF" });
            Stations.Add(new StationViewModel { X = 300, Y = 250, ShortName = "�r��", TypeColor = "#FFFF00" });
            Stations.Add(new StationViewModel { X = 500, Y = 350, ShortName = "���l", TypeColor = "#FF00FF" });

            // �T���v���v���C���[�f�[�^
            Players.Add(new PlayerViewModel { PositionX = 400, PositionY = 300, Name = "�v���C���[1" });

            // �v���C���[���
            PlayerInfos.Add(new PlayerInfoViewModel
            {
                Name = "�v���C���[1",
                Money = "1���~",
                TotalAssets = "1���~",
                Rank = 1,
                BorderColor = "#0000FF"
            });

            PlayerInfos.Add(new PlayerInfoViewModel
            {
                Name = "COM1",
                Money = "1���~",
                TotalAssets = "1���~",
                Rank = 2,
                BorderColor = "#FF0000"
            });

            PlayerInfos.Add(new PlayerInfoViewModel
            {
                Name = "COM2",
                Money = "1���~",
                TotalAssets = "1���~",
                Rank = 3,
                BorderColor = "#00FF00"
            });

            PlayerInfos.Add(new PlayerInfoViewModel
            {
                Name = "COM3",
                Money = "1���~",
                TotalAssets = "1���~",
                Rank = 4,
                BorderColor = "#FFFF00"
            });
        }

        // �R�}���h�n���h��
        private void RollDice()
        {
            CurrentMessage = "�T�C�R����U��܂����I";
            CanRollDice = false;
            CanEndTurn = true;
        }

        private void EndTurn()
        {
            CurrentMessage = "�^�[�����I�����܂����B";
            CanRollDice = true;
            CanEndTurn = false;
            CurrentMonth++;
            if (CurrentMonth > 12)
            {
                CurrentMonth = 1;
                CurrentYear++;
            }
        }

        private void SaveGame()
        {
            CurrentMessage = "�Q�[�����Z�[�u���܂����B";
        }

        private void LoadGame()
        {
            CurrentMessage = "�Q�[�������[�h���܂����B";
        }

        private void ShowSettings()
        {
            CurrentMessage = "�ݒ��ʂ��J���܂��B";
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

    // �wViewModel
    public class StationViewModel
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string ShortName { get; set; }
        public string TypeColor { get; set; }
    }

    // �v���C���[ViewModel
    public class PlayerViewModel
    {
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string Name { get; set; }
    }

    // �v���C���[���ViewModel
    public class PlayerInfoViewModel
    {
        public string Name { get; set; }
        public string Money { get; set; }
        public string TotalAssets { get; set; }
        public int Rank { get; set; }
        public string BorderColor { get; set; }
    }

    // RelayCommand����
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
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
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}