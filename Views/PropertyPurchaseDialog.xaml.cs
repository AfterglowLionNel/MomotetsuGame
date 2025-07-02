using System;
using System.Windows;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.ViewModels;

namespace MomotetsuGame.Views
{
    /// <summary>
    /// PropertyPurchaseDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyPurchaseDialog : Window
    {
        private PropertyPurchaseViewModel _viewModel;

        public Property SelectedProperty { get; private set; }
        public bool IsPurchased { get; private set; }

        public PropertyPurchaseDialog(Station station, Player player)
        {
            InitializeComponent();

            _viewModel = (PropertyPurchaseViewModel)DataContext;
            _viewModel.Initialize(station, player);

            // イベントハンドラを設定
            _viewModel.PropertyPurchased += OnPropertyPurchased;
            _viewModel.Cancelled += OnCancelled;
        }

        private void OnPropertyPurchased(object sender, Property property)
        {
            SelectedProperty = property;
            IsPurchased = true;
            DialogResult = true;
            Close();
        }

        private void OnCancelled(object sender, EventArgs e)
        {
            IsPurchased = false;
            DialogResult = false;
            Close();
        }
    }
}