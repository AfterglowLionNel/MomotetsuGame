namespace MomotetsuGame.Core.Enums
{
    /// <summary>
    /// 駅の種類
    /// </summary>
    public enum StationType
    {
        Property,      // 物件駅
        CardShop,      // カード売り場
        Plus,          // プラス駅
        Minus,         // マイナス駅
        NiceCard,      // ナイスカード駅
        SuperCard,     // スーパーカード駅
        CardExchange,  // カード交換駅
        Lottery        // 宝くじ売り場
    }

    /// <summary>
    /// 物件カテゴリ
    /// </summary>
    public enum PropertyCategory
    {
        Agriculture,   // 農林
        Fishery,      // 水産
        Commerce,     // 商業
        Industry,     // 工業
        Tourism       // 観光
    }

    /// <summary>
    /// カードタイプ
    /// </summary>
    public enum CardType
    {
        Movement,      // 移動系
        Convenience,   // 便利系
        Attack,        // 攻撃系
        Defense,       // 防御系
        Special        // 特殊系
    }

    /// <summary>
    /// カードレア度
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
    /// プレイヤーステータス
    /// </summary>
    public enum PlayerStatus
    {
        Normal,        // 通常
        SuperLucky,    // 絶好調
        Unlucky,       // 絶不調
        Cow,           // 牛歩状態
        Sealed         // 封印状態
    }

    /// <summary>
    /// ゲームフェーズ
    /// </summary>
    public enum GamePhase
    {
        Setup,         // セットアップ
        TurnStart,     // ターン開始
        Action,        // 行動選択
        Movement,      // 移動
        Arrival,       // 到着処理
        Event,         // イベント
        TurnEnd        // ターン終了
    }

    /// <summary>
    /// 地域
    /// </summary>
    public enum Region
    {
        Hokkaido,      // 北海道
        Tohoku,        // 東北
        Kanto,         // 関東
        Chubu,         // 中部
        Kinki,         // 近畿
        Chugoku,       // 中国
        Shikoku,       // 四国
        Kyushu         // 九州
    }

    /// <summary>
    /// ゲームモード
    /// </summary>
    public enum GameMode
    {
        Normal,        // 通常モード
        ThreeYears,    // 3年決戦
        Custom         // カスタムモード
    }

    /// <summary>
    /// COM難易度
    /// </summary>
    public enum ComDifficulty
    {
        Weak,          // 弱い
        Normal,        // 普通
        Strong         // 強い
    }

    /// <summary>
    /// アニメーション速度
    /// </summary>
    public enum AnimationSpeed
    {
        Slow,          // ゆっくり
        Normal,        // 普通
        Fast           // 速い
    }

    /// <summary>
    /// プレイヤーカラー
    /// </summary>
    public enum PlayerColor
    {
        Blue,          // 青
        Red,           // 赤
        Green,         // 緑
        Yellow,        // 黄
        Purple,        // 紫
        Orange,        // オレンジ
        Pink,          // ピンク
        Cyan           // シアン
    }
}