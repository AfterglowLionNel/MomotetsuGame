using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;
using Newtonsoft.Json.Linq;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// 駅エンティティ
    /// </summary>
    public class Station
    {
        private List<Station> _connectedStations;
        private List<Property> _properties;

        /// <summary>
        /// 駅ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 駅名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 駅名（短縮形）- UI表示用
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// 駅の位置（マップ上の座標）
        /// </summary>
        public Coordinate Location { get; set; }

        /// <summary>
        /// 駅の種別
        /// </summary>
        public StationType Type { get; set; }

        /// <summary>
        /// 地域
        /// </summary>
        public Region Region { get; set; }

        /// <summary>
        /// 駅で購入可能な物件リスト
        /// </summary>
        public List<Property> Properties
        {
            get => _properties;
            set => _properties = value ?? new List<Property>();
        }

        /// <summary>
        /// 接続されている駅のリスト
        /// </summary>
        public List<Station> ConnectedStations
        {
            get => _connectedStations;
            set => _connectedStations = value ?? new List<Station>();
        }

        /// <summary>
        /// カード売り場があるかどうか
        /// </summary>
        public bool HasCardShop { get; set; }

        /// <summary>
        /// 目的地として設定されているかどうか
        /// </summary>
        public bool IsDestination { get; set; }

        /// <summary>
        /// 分岐点かどうか（3つ以上の駅と接続）
        /// </summary>
        public bool IsBranchPoint => ConnectedStations.Count >= 3;

        /// <summary>
        /// 駅の色（UI表示用）
        /// </summary>
        public string TypeColor => GetStationTypeColor();

        /// <summary>
        /// マップ上のX座標
        /// </summary>
        public double X => Location.X;

        /// <summary>
        /// マップ上のY座標
        /// </summary>
        public double Y => Location.Y;

        /// <summary>
        /// 特殊駅用の追加パラメータ
        /// </summary>
        public Dictionary<string, object> SpecialParameters { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Station()
        {
            _connectedStations = new List<Station>();
            _properties = new List<Property>();
            SpecialParameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// 指定IDの駅を作成（ファクトリメソッド）
        /// </summary>
        public static Station Create(int id, string name, double x, double y, StationType type, Region region)
        {
            return new Station
            {
                Id = id,
                Name = name,
                ShortName = name.Length > 3 ? name.Substring(0, 3) : name,
                Location = new Coordinate(x, y),
                Type = type,
                Region = region
            };
        }

        /// <summary>
        /// 駅を接続
        /// </summary>
        public void ConnectTo(Station other)
        {
            if (other == null || other == this) return;

            if (!ConnectedStations.Contains(other))
            {
                ConnectedStations.Add(other);
            }

            if (!other.ConnectedStations.Contains(this))
            {
                other.ConnectedStations.Add(this);
            }
        }

        /// <summary>
        /// 駅の接続を解除
        /// </summary>
        public void DisconnectFrom(Station other)
        {
            if (other == null) return;

            ConnectedStations.Remove(other);
            other.ConnectedStations.Remove(this);
        }

        /// <summary>
        /// 指定駅への距離を計算
        /// </summary>
        public double DistanceTo(Station other)
        {
            if (other == null) return double.MaxValue;
            return Location.DistanceTo(other.Location);
        }

        /// <summary>
        /// 指定駅と直接接続されているか
        /// </summary>
        public bool IsConnectedTo(Station other)
        {
            return ConnectedStations.Contains(other);
        }

        /// <summary>
        /// 購入可能な物件を取得
        /// </summary>
        public List<Property> GetAvailableProperties()
        {
            return Properties.Where(p => p.Owner == null).ToList();
        }

        /// <summary>
        /// 指定プレイヤーが独占しているかチェック
        /// </summary>
        public bool IsMonopolizedBy(Player player)
        {
            if (Properties.Count == 0) return false;
            return Properties.All(p => p.Owner == player);
        }

        /// <summary>
        /// 駅種別に応じた色を取得
        /// </summary>
        private string GetStationTypeColor()
        {
            return Type switch
            {
                StationType.Property => "#4169E1",      // RoyalBlue
                StationType.CardShop => "#FF8C00",      // DarkOrange
                StationType.Plus => "#32CD32",          // LimeGreen
                StationType.Minus => "#DC143C",         // Crimson
                StationType.NiceCard => "#FFD700",      // Gold
                StationType.SuperCard => "#FF1493",     // DeepPink
                StationType.CardExchange => "#9370DB",  // MediumPurple
                StationType.Lottery => "#FF69B4",       // HotPink
                _ => "#808080"                          // Gray
            };
        }

        /// <summary>
        /// プラス駅のボーナス金額を取得
        /// </summary>
        public Money GetPlusBonus()
        {
            if (Type != StationType.Plus) return Money.Zero;

            // 基本金額（1000万円～1億円）
            var random = new Random();
            var baseAmount = random.Next(10, 101) * 1000000L;

            // 地域ボーナス
            var regionBonus = Region switch
            {
                Region.Hokkaido => 1.2,
                Region.Kanto => 1.5,
                Region.Kinki => 1.4,
                _ => 1.0
            };

            return new Money((long)(baseAmount * regionBonus));
        }

        /// <summary>
        /// マイナス駅のペナルティ金額を取得
        /// </summary>
        public Money GetMinusPenalty()
        {
            if (Type != StationType.Minus) return Money.Zero;

            // 基本金額（1000万円～5000万円）
            var random = new Random();
            var baseAmount = random.Next(10, 51) * 1000000L;

            return new Money(baseAmount);
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"{Name}駅 ({Type})";
        }
    }

    /// <summary>
    /// 駅ネットワーク管理クラス
    /// </summary>
    public class StationNetwork
    {
        private readonly List<Station> _stations;
        private readonly Dictionary<int, Station> _stationMap;

        public StationNetwork()
        {
            _stations = new List<Station>();
            _stationMap = new Dictionary<int, Station>();
        }

        /// <summary>
        /// 駅を追加
        /// </summary>
        public void AddStation(Station station)
        {
            if (!_stationMap.ContainsKey(station.Id))
            {
                _stations.Add(station);
                _stationMap[station.Id] = station;
            }
        }

        /// <summary>
        /// IDで駅を取得
        /// </summary>
        public Station? GetStation(int id)
        {
            return _stationMap.TryGetValue(id, out var station) ? station : null;
        }

        /// <summary>
        /// 全駅を取得
        /// </summary>
        public IReadOnlyList<Station> AllStations => _stations;

        /// <summary>
        /// 地域で駅を検索
        /// </summary>
        public List<Station> GetStationsByRegion(Region region)
        {
            return _stations.Where(s => s.Region == region).ToList();
        }

        /// <summary>
        /// 種別で駅を検索
        /// </summary>
        public List<Station> GetStationsByType(StationType type)
        {
            return _stations.Where(s => s.Type == type).ToList();
        }
    }
}