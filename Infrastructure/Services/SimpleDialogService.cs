using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Infrastructure.Services
{
    /// <summary>
    /// WPFダイアログサービスの実装
    /// </summary>
    public class SimpleDialogService : IDialogService
    {
        public Task<bool> ShowConfirmationAsync(string message, string title = "確認")
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        public Task ShowInformationAsync(string message, string title = "情報")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string message, string title = "エラー")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return Task.CompletedTask;
        }

        public async Task<T?> ShowSelectionAsync<T>(string message, T[] options, string title = "選択")
        {
            // カスタム選択ダイアログが必要な場合は、専用のWindowを作成
            // 現時点では簡易実装として、最初のオプションを返す

            // 実際の実装では、SelectionDialogWindowを作成して表示
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
            // ViewModelに基づいて適切なダイアログを表示
            // 実際の実装では、ViewModelの型に応じて適切なWindowを選択

            if (viewModel == null)
                return default(TResult);

            Window? dialog = viewModel switch
            {
                // 各ViewModelに対応するダイアログを作成
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

            // ダイアログの結果を取得（実装は各ダイアログに依存）
            if (result == true && viewModel is IDialogViewModel<TResult> dialogViewModel)
            {
                return await Task.FromResult(dialogViewModel.Result);
            }

            return default(TResult);
        }
    }

    /// <summary>
    /// 汎用選択ダイアログ（仮実装）
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

            // 実際の実装では、適切なUIを構築
            // 現時点では簡易実装
        }
    }

    /// <summary>
    /// ダイアログViewModel用インターフェース
    /// </summary>
    public interface IDialogViewModel<TResult>
    {
        TResult Result { get; }
    }
}