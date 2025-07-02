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
        Fishery,      // ���Y
        Commerce,     // ����
        Industry,     // �H��
        Tourism       // �ό�
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
    /// �J�[�h���A�x
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
    /// �v���C���[�X�e�[�^�X
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
        ThreeYears,    // 3�N����
        Custom         // �J�X�^�����[�h
    }

    /// <summary>
    /// COM��Փx
    /// </summary>
    public enum ComDifficulty
    {
        Weak,          // �ア
        Normal,        // ����
        Strong         // ����
    }

    /// <summary>
    /// �A�j���[�V�������x
    /// </summary>
    public enum AnimationSpeed
    {
        Slow,          // �������
        Normal,        // ����
        Fast           // ����
    }

    /// <summary>
    /// �v���C���[�J���[
    /// </summary>
    public enum PlayerColor
    {
        Blue,          // ��
        Red,           // ��
        Green,         // ��
        Yellow,        // ��
        Purple,        // ��
        Orange,        // �I�����W
        Pink,          // �s���N
        Cyan           // �V�A��
    }
}