using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// �T�C�R����ʂ�ViewModel
    /// </summary>
    public class DiceRollViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _animationTimer;
        private readonly Random _random = new Random();

        private int _diceCount;
        private int _dice1Value = 1;
        private int _dice2Value = 1;
        private int _dice3Value = 1;
        private bool _isRolling;
        private bool _showResult;
        private string _resultText;
        private DiceResult _finalResult;
        private int _animationCounter;

        // �v���p�e�B
        public int Dice1Value
        {
            get => _dice1Value;
            set => SetProperty(ref _dice1Value, value);
        }

        public int Dice2Value
        {
            get => _dice2Value;
            set => SetProperty(ref _dice2Value, value);
        }

        public int Dice3Value
        {
            get => _dice3Value;
            set => SetProperty(ref _dice3Value, value);
        }

        public bool IsRolling
        {
            get => _isRolling;
            set => SetProperty(ref _isRolling, value);
        }

        public bool ShowResult
        {
            get => _showResult;
            set => SetProperty(ref _showResult, value);
        }

        public string ResultText
        {
            get => _resultText;
            set => SetProperty(ref _resultText, value);
        }

        public bool ShowDice1 => _diceCount >= 1;
        public bool ShowDice2 => _diceCount >= 2;
        public bool ShowDice3 => _diceCount >= 3;

        // �R�}���h
        public ICommand RollCommand { get; private set; }
        public ICommand ConfirmCommand { get; private set; }

        // �C�x���g
        public event EventHandler<DiceResult> DiceRolled;
        public event EventHandler Confirmed;

        // �R���X�g���N�^
        public DiceRollViewModel()
        {
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _animationTimer.Tick += OnAnimationTick;

            InitializeCommands();
        }

        /// <summary>
        /// ������
        /// </summary>
        public void Initialize(int diceCount, DiceResult presetResult = null)
        {
            _diceCount = Math.Max(1, Math.Min(3, diceCount));
            _finalResult = presetResult;
            ShowResult = false;
            IsRolling = false;

            OnPropertyChanged(nameof(ShowDice1));
            OnPropertyChanged(nameof(ShowDice2));
            OnPropertyChanged(nameof(ShowDice3));
        }

        private void InitializeCommands()
        {
            RollCommand = new RelayCommand(async () => await RollDiceAsync(), () => !IsRolling);
            ConfirmCommand = new RelayCommand(Confirm, () => ShowResult);
        }

        private async Task RollDiceAsync()
        {
            IsRolling = true;
            ShowResult = false;
            _animationCounter = 0;

            // �ŏI���ʂ�����i�v���Z�b�g���Ȃ��ꍇ�j
            if (_finalResult == null)
            {
                var values = new List<int>();
                for (int i = 0; i < _diceCount; i++)
                {
                    values.Add(_random.Next(1, 7));
                }
                _finalResult = new DiceResult(values);
            }

            // �A�j���[�V�����J�n
            _animationTimer.Start();

            // 2�b�ԃA�j���[�V����
            await Task.Delay(2000);

            // �A�j���[�V������~
            _animationTimer.Stop();

            // �ŏI���ʂ�\��
            if (_diceCount >= 1) Dice1Value = _finalResult.Values[0];
            if (_diceCount >= 2) Dice2Value = _finalResult.Values[1];
            if (_diceCount >= 3) Dice3Value = _finalResult.Values[2];

            IsRolling = false;
            ShowResult = true;

            // ���ʃe�L�X�g�𐶐�
            GenerateResultText();

            // �C�x���g����
            DiceRolled?.Invoke(this, _finalResult);
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            _animationCounter++;

            // �����_���ɒl��ύX�i���X�ɒx���Ȃ�j
            if (_animationCounter % Math.Max(1, _animationCounter / 10) == 0)
            {
                if (ShowDice1) Dice1Value = _random.Next(1, 7);
                if (ShowDice2) Dice2Value = _random.Next(1, 7);
                if (ShowDice3) Dice3Value = _random.Next(1, 7);
            }
        }

        private void GenerateResultText()
        {
            if (_finalResult == null) return;

            ResultText = $"���v: {_finalResult.Total}";

            if (_finalResult.IsDouble)
            {
                ResultText += " �]���ځI";
            }

            if (_finalResult.Total >= 15 && _diceCount >= 3)
            {
                ResultText += " �哖����I";
            }
        }

        private void Confirm()
        {
            Confirmed?.Invoke(this, EventArgs.Empty);
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
            CommandManager.InvalidateRequerySuggested();
            return true;
        }
    }
}