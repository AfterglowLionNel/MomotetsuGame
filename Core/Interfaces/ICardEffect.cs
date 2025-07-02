using System;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// �J�[�h���ʂ̃C���^�[�t�F�[�X
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// ���ʂ̎��
        /// </summary>
        string EffectType { get; }

        /// <summary>
        /// ���ʂ����s�\���`�F�b�N
        /// </summary>
        /// <param name="context">�Q�[���R���e�L�X�g</param>
        /// <returns>���s�\�ȏꍇtrue</returns>
        Task<bool> CanExecute(CardEffectContext context);

        /// <summary>
        /// ���ʂ����s
        /// </summary>
        /// <param name="context">�Q�[���R���e�L�X�g</param>
        /// <returns>���s����</returns>
        Task<EffectResult> Execute(CardEffectContext context);

        /// <summary>
        /// ���ʂ̐������擾
        /// </summary>
        /// <returns>������</returns>
        string GetDescription();
    }

    /// <summary>
    /// �J�[�h���ʎ��s���̃R���e�L�X�g
    /// </summary>
    public class CardEffectContext
    {
        /// <summary>
        /// ���݂̃Q�[�����
        /// </summary>
        public GameState GameState { get; set; } = null!;

        /// <summary>
        /// �J�[�h���g�p�����v���C���[
        /// </summary>
        public Player Player { get; set; } = null!;

        /// <summary>
        /// �g�p���ꂽ�J�[�h
        /// </summary>
        public Card Card { get; set; } = null!;

        /// <summary>
        /// �Q�[���}�l�[�W���[
        /// </summary>
        public IGameManager GameManager { get; set; } = null!;

        /// <summary>
        /// �_�C�X�T�[�r�X
        /// </summary>
        public IDiceService? DiceService { get; set; }

        /// <summary>
        /// �C�x���g�o�X
        /// </summary>
        public IEventBus? EventBus { get; set; }

        /// <summary>
        /// UI�Θb�T�[�r�X
        /// </summary>
        public IDialogService? DialogService { get; set; }

        /// <summary>
        /// �ǉ��p�����[�^
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// �J�[�h���ʂ̎��s����
    /// </summary>
    public class EffectResult
    {
        /// <summary>
        /// �����������ǂ���
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ���ʃ��b�Z�[�W
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// �A���s���\���ǂ���
        /// </summary>
        public bool AllowContinuation { get; set; }

        /// <summary>
        /// �ǉ��f�[�^
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// �T�C�R���T�[�r�X�C���^�[�t�F�[�X
    /// </summary>
    public interface IDiceService
    {
        /// <summary>
        /// �T�C�R����U��
        /// </summary>
        /// <param name="count">�T�C�R���̐�</param>
        /// <returns>�T�C�R���̌���</returns>
        DiceResult Roll(int count);

        /// <summary>
        /// ����T�C�R����U��
        /// </summary>
        /// <param name="count">�T�C�R���̐�</param>
        /// <param name="min">�ŏ��l</param>
        /// <param name="max">�ő�l</param>
        /// <returns>�T�C�R���̌���</returns>
        DiceResult RollSpecial(int count, int min, int max);
    }

    /// <summary>
    /// �C�x���g�o�X�C���^�[�t�F�[�X
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// �C�x���g�𔭍s
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : class;

        /// <summary>
        /// �C�x���g���w��
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// �C�x���g�̍w�ǂ�����
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    }

    /// <summary>
    /// �_�C�A���O�T�[�r�X�C���^�[�t�F�[�X
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// �m�F�_�C�A���O��\��
        /// </summary>
        Task<bool> ShowConfirmationAsync(string message, string title);

        /// <summary>
        /// �I���_�C�A���O��\��
        /// </summary>
        Task<T?> ShowSelectionAsync<T>(IEnumerable<T> items, string title);

        /// <summary>
        /// ���b�Z�[�W�_�C�A���O��\��
        /// </summary>
        Task ShowMessageAsync(string message, string title);

        /// <summary>
        /// �G���[�_�C�A���O��\��
        /// </summary>
        Task ShowErrorAsync(string message, string title);
    }

    /// <summary>
    /// UI�X�V�v���C�x���g�i�J�[�h�I���Ȃǁj
    /// </summary>
    public class CardSelectionRequestEvent
    {
        public Player Player { get; set; } = null!;
        public List<Card> AvailableCards { get; set; } = new List<Card>();
        public string Message { get; set; } = string.Empty;
        public Card? SelectedCard { get; set; }
        public TaskCompletionSource<Card?> CompletionSource { get; set; } = new TaskCompletionSource<Card?>();

        public async Task<Card?> WaitForCompletionAsync()
        {
            return await CompletionSource.Task;
        }
    }

    /// <summary>
    /// �v���C���[�I��v���C�x���g
    /// </summary>
    public class PlayerSelectionRequestEvent
    {
        public List<Player> AvailablePlayers { get; set; } = new List<Player>();
        public string Message { get; set; } = string.Empty;
        public Player? SelectedPlayer { get; set; }
        public TaskCompletionSource<Player?> CompletionSource { get; set; } = new TaskCompletionSource<Player?>();

        public async Task<Player?> WaitForCompletionAsync()
        {
            return await CompletionSource.Task;
        }
    }

    /// <summary>
    /// �����w���v���C�x���g
    /// </summary>
    public class PropertyPurchaseRequestEvent
    {
        public Player Player { get; set; } = null!;
        public Station Station { get; set; } = null!;
        public List<Property> AvailableProperties { get; set; } = new List<Property>();
        public List<Property> PurchasedProperties { get; set; } = new List<Property>();
        public TaskCompletionSource<List<Property>> CompletionSource { get; set; } = new TaskCompletionSource<List<Property>>();

        public async Task<List<Property>> WaitForCompletionAsync()
        {
            return await CompletionSource.Task;
        }
    }
}