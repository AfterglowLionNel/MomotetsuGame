using System;
using System.Windows;
using System.Windows.Media.Animation;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.ViewModels;

namespace MomotetsuGame.Views
{
    /// <summary>
    /// DiceRollDialog.xaml �̑��ݍ�p���W�b�N
    /// </summary>
    public partial class DiceRollDialog : Window
    {
        private DiceRollViewModel _viewModel;
        private Storyboard _diceRollStoryboard;

        public DiceResult Result { get; private set; }

        public DiceRollDialog(int diceCount, DiceResult presetResult = null)
        {
            InitializeComponent();

            _viewModel = (DiceRollViewModel)DataContext;
            _viewModel.Initialize(diceCount, presetResult);

            // �C�x���g�n���h����ݒ�
            _viewModel.DiceRolled += OnDiceRolled;
            _viewModel.Confirmed += OnConfirmed;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // �A�j���[�V�������擾
            _diceRollStoryboard = (Storyboard)Resources["DiceRollAnimation"];
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DiceRollViewModel.IsRolling))
            {
                if (_viewModel.IsRolling)
                {
                    StartDiceAnimation();
                }
                else
                {
                    StopDiceAnimation();
                }
            }
        }

        private void StartDiceAnimation()
        {
            // �e�T�C�R���̃A�j���[�V�������J�n
            if (_viewModel.ShowDice1)
            {
                var dice1Animation = _diceRollStoryboard.Clone();
                Storyboard.SetTarget(dice1Animation, Dice1);
                dice1Animation.Begin();
            }

            if (_viewModel.ShowDice2)
            {
                var dice2Animation = _diceRollStoryboard.Clone();
                Storyboard.SetTarget(dice2Animation, Dice2);
                dice2Animation.Begin();
            }

            if (_viewModel.ShowDice3)
            {
                var dice3Animation = _diceRollStoryboard.Clone();
                Storyboard.SetTarget(dice3Animation, Dice3);
                dice3Animation.Begin();
            }
        }

        private void StopDiceAnimation()
        {
            // �A�j���[�V�������~
            if (Dice1 != null) Dice1.RenderTransform = new System.Windows.Media.RotateTransform(0);
            if (Dice2 != null) Dice2.RenderTransform = new System.Windows.Media.RotateTransform(0);
            if (Dice3 != null) Dice3.RenderTransform = new System.Windows.Media.RotateTransform(0);
        }

        private void OnDiceRolled(object sender, DiceResult result)
        {
            Result = result;
        }

        private void OnConfirmed(object sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}