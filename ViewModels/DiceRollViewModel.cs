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
    /// サイコロ画面のViewModel
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

        // プロパティ
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

        // コマンド
        public ICommand RollCommand { get; private set; }
        public ICommand ConfirmCommand { get; private set; }

        // イベント
        public event EventHandler<DiceResult> DiceRolled;
        public event EventHandler Confirmed;

        // コンストラクタ
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
        /// 初期化
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

            // 最終結果を決定（プリセットがない場合）
            if (_finalResult == null)
            {
                var values = new List<int>();
                for (int i = 0; i < _diceCount; i++)
                {
                    values.Add(_random.Next(1, 7));
                }
                _finalResult = new DiceResult(values);
            }

            // アニメーション開始
            _animationTimer.Start();

            // 2秒間アニメーション
            await Task.Delay(2000);

            // アニメーション停止
            _animationTimer.Stop();

            // 最終結果を表示
            if (_diceCount >= 1) Dice1Value = _finalResult.Values[0];
            if (_diceCount >= 2) Dice2Value = _finalResult.Values[1];
            if (_diceCount >= 3) Dice3Value = _finalResult.Values[2];

            IsRolling = false;
            ShowResult = true;

            // 結果テキストを生成
            GenerateResultText();

            // イベント発火
            DiceRolled?.Invoke(this, _finalResult);
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            _animationCounter++;

            // ランダムに値を変更（徐々に遅くなる）
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

            ResultText = $"合計: {_finalResult.Total}";

            if (_finalResult.IsDouble)
            {
                ResultText += " ゾロ目！";
            }

            if (_finalResult.Total >= 15 && _diceCount >= 3)
            {
                ResultText += " 大当たり！";
            }
        }

        private void Confirm()
        {
            Confirmed?.Invoke(this, EventArgs.Empty);
        }

        // INotifyPropertyChanged実装
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