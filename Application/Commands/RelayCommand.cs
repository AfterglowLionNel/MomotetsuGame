using System;
using System.Windows.Input;

namespace MomotetsuGame.Application.Commands
{
    /// <summary>
    /// パラメータなしのコマンド実装
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">実行処理</param>
        /// <param name="canExecute">実行可能判定処理</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 実行可能状態が変更された際のイベント
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute();
        }

        /// <summary>
        /// 実行可能状態の再評価を要求
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// パラメータ付きのコマンド実装
    /// </summary>
    /// <typeparam name="T">パラメータの型</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">実行処理</param>
        /// <param name="canExecute">実行可能判定処理</param>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 実行可能状態が変更された際のイベント
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
                return false;

            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        /// <summary>
        /// 実行可能状態の再評価を要求
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// 非同期コマンド実装
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<System.Threading.Tasks.Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">非同期実行処理</param>
        /// <param name="canExecute">実行可能判定処理</param>
        public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 実行可能状態が変更された際のイベント
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public async void Execute(object? parameter)
        {
            if (_isExecuting)
                return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}