using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Infrastructure.Services
{
    /// <summary>
    /// WPF�_�C�A���O�T�[�r�X�̎���
    /// </summary>
    public class SimpleDialogService : IDialogService
    {
        public Task<bool> ShowConfirmationAsync(string message, string title = "�m�F")
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        public Task ShowInformationAsync(string message, string title = "���")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string message, string title = "�G���[")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return Task.CompletedTask;
        }

        public async Task<T?> ShowSelectionAsync<T>(string message, T[] options, string title = "�I��")
        {
            // �J�X�^���I���_�C�A���O���K�v�ȏꍇ�́A��p��Window���쐬
            // �����_�ł͊ȈՎ����Ƃ��āA�ŏ��̃I�v�V������Ԃ�

            // ���ۂ̎����ł́ASelectionDialogWindow���쐬���ĕ\��
            var window = new SelectionDialog<T>
            {
                Title = title,
                Message = message,
                Options = options.ToList()
            };

            if (Application.Current.MainWindow != null)
            {
                window.Owner = Application.Current.MainWindow;
            }

            var result = window.ShowDialog();

            if (result == true && window.SelectedItem != null)
            {
                return await Task.FromResult(window.SelectedItem);
            }

            return default(T);
        }

        public async Task<TResult?> ShowDialogAsync<TResult>(object viewModel)
        {
            // ViewModel�Ɋ�Â��ēK�؂ȃ_�C�A���O��\��
            // ���ۂ̎����ł́AViewModel�̌^�ɉ����ēK�؂�Window��I��

            if (viewModel == null)
                return default(TResult);

            Window? dialog = viewModel switch
            {
                // �eViewModel�ɑΉ�����_�C�A���O���쐬
                // PropertyPurchaseViewModel => new PropertyPurchaseDialog(),
                // CardSelectionViewModel => new CardSelectionDialog(),
                _ => null
            };

            if (dialog == null)
            {
                throw new NotSupportedException($"ViewModel type {viewModel.GetType().Name} is not supported.");
            }

            dialog.DataContext = viewModel;

            if (Application.Current.MainWindow != null)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            var result = dialog.ShowDialog();

            // �_�C�A���O�̌��ʂ��擾�i�����͊e�_�C�A���O�Ɉˑ��j
            if (result == true && viewModel is IDialogViewModel<TResult> dialogViewModel)
            {
                return await Task.FromResult(dialogViewModel.Result);
            }

            return default(TResult);
        }
    }

    /// <summary>
    /// �ėp�I���_�C�A���O�i�������j
    /// </summary>
    public class SelectionDialog<T> : Window
    {
        public string Message { get; set; } = string.Empty;
        public List<T> Options { get; set; } = new List<T>();
        public T? SelectedItem { get; set; }

        public SelectionDialog()
        {
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // ���ۂ̎����ł́A�K�؂�UI���\�z
            // �����_�ł͊ȈՎ���
        }
    }

    /// <summary>
    /// �_�C�A���OViewModel�p�C���^�[�t�F�[�X
    /// </summary>
    public interface IDialogViewModel<TResult>
    {
        TResult Result { get; }
    }
}