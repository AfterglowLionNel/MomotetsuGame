using System;
using System.Windows.Input;

namespace MomotetsuGame.Application.Commands
{
    /// <summary>
    /// �p�����[�^�Ȃ��̃R�}���h����
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="execute">���s����</param>
        /// <param name="canExecute">���s�\���菈��</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// ���s�\��Ԃ��ύX���ꂽ�ۂ̃C�x���g
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// �R�}���h�����s�\���ǂ����𔻒�
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// �R�}���h�����s
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute();
        }

        /// <summary>
        /// ���s�\��Ԃ̍ĕ]����v��
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// �p�����[�^�t���̃R�}���h����
    /// </summary>
    /// <typeparam name="T">�p�����[�^�̌^</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Predicate<T?>? _canExecute;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="execute">���s����</param>
        /// <param name="canExecute">���s�\���菈��</param>
        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// ���s�\��Ԃ��ύX���ꂽ�ۂ̃C�x���g
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// �R�}���h�����s�\���ǂ����𔻒�
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
                return false;

            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        /// <summary>
        /// �R�}���h�����s
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        /// <summary>
        /// ���s�\��Ԃ̍ĕ]����v��
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// �񓯊��R�}���h����
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<System.Threading.Tasks.Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="execute">�񓯊����s����</param>
        /// <param name="canExecute">���s�\���菈��</param>
        public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// ���s�\��Ԃ��ύX���ꂽ�ۂ̃C�x���g
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// �R�}���h�����s�\���ǂ����𔻒�
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// �R�}���h�����s
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