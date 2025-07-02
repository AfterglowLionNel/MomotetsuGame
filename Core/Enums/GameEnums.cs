namespace MomotetsuGame.Core.Enums
{
    /// <summary>
    /// �w�̎��
    /// </summary>
    public enum StationType
    {
        Property,      // �����w
        CardShop,      // �J�[�h�����
        Plus,          // �v���X�w
        Minus,         // �}�C�i�X�w
        NiceCard,      // �i�C�X�J�[�h�w
        SuperCard,     // �X�[�p�[�J�[�h�w
        CardExchange,  // �J�[�h�����w
        Lottery        // �󂭂������
    }

    /// <summary>
    /// �����J�e�S��
    /// </summary>
    public enum PropertyCategory
    {
        Agriculture,   // �_��
        Fishery,       // ���Y
        Commerce,      // ����
        Industry,      // �H��
        Tourism        // �ό�
    }

    /// <summary>
    /// �J�[�h�^�C�v
    /// </summary>
    public enum CardType
    {
        Movement,      // �ړ��n
        Convenience,   // �֗��n
        Attack,        // �U���n
        Defense,       // �h��n
        Special        // ����n
    }

    /// <summary>
    /// �J�[�h���A���e�B
    /// </summary>
    public enum CardRarity
    {
        C,    // Common
        B,    // Basic
        A,    // Advanced
        S,    // Super
        SS    // Super Special
    }

    /// <summary>
    /// �v���C���[���
    /// </summary>
    public enum PlayerStatus
    {
        Normal,        // �ʏ�
        SuperLucky,    // ��D��
        Unlucky,       // ��s��
        Cow,           // �������
        Sealed         // ������
    }

    /// <summary>
    /// �Q�[���t�F�[�Y
    /// </summary>
    public enum GamePhase
    {
        Setup,         // �Z�b�g�A�b�v
        TurnStart,     // �^�[���J�n
        Action,        // �s���I��
        Movement,      // �ړ�
        Arrival,       // ��������
        Event,         // �C�x���g
        TurnEnd        // �^�[���I��
    }

    /// <summary>
    /// �n��
    /// </summary>
    public enum Region
    {
        Hokkaido,      // �k�C��
        Tohoku,        // ���k
        Kanto,         // �֓�
        Chubu,         // ����
        Kinki,         // �ߋE
        Chugoku,       // ����
        Shikoku,       // �l��
        Kyushu         // ��B
    }

    /// <summary>
    /// �Q�[�����[�h
    /// </summary>
    public enum GameMode
    {
        Normal,        // �ʏ탂�[�h
        ShortBattle,   // 3�N����
        Custom         // �J�X�^��
    }

    /// <summary>
    /// �v���C���[�J���[
    /// </summary>
    public enum PlayerColor
    {
        Blue,
        Red,
        Yellow,
        Green
    }

    /// <summary>
    /// �{���r�[�^�C�v
    /// </summary>
    public enum BonbyType
    {
        None,
        Mini,          // �~�j�{���r�[
        Normal,        // �{���r�[
        King,          // �L���O�{���r�[
        Pokon,         // �|�R��
        Bonbiras       // �{���r���X��
    }

    /// <summary>
    /// �A�N�V�����^�C�v
    /// </summary>
    public enum ActionType
    {
        RollDice,      // �T�C�R����U��
        UseCard,       // �J�[�h���g��
        CheckStatus,   // �X�e�[�^�X�m�F
        Settings       // �ݒ�
    }

    /// <summary>
    /// AI�헪
    /// </summary>
    public enum AIStrategy
    {
        Passive,       // ���ɓI
        Balanced,      // �o�����X�^
        Aggressive     // �U���I
    }
}