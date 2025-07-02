using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// 駅エンティティ
    /// </summary>
    public class Station
    {
        /// <summary>
        /// 駅ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 駅名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 駅の場所（座標）
        /// </summary>
        public Coordinate Location { get; set; }

        /// <summary>
        /// 駅の種類
        /// </summary>
        public StationType Type { get; set; }

        /// <summary>
        /// 地域
        /// </summary>
        public Region Region { get; set; }

        /// <summary>
        /// この駅の物件リスト
        /// </summary>
        public List<Property> Properties { get; set; }

        /// <summary>
        /// 接続されている駅のリスト
        /// </summary>
        public List<Station> ConnectedStations { get; set; }

        /// <summary>
        /// カード売り場があるか
        /// </summary>
        public bool HasCardShop { get; set; }

        /// <summary>
        /// 目的地かどうか
        /// </summary>
        public bool IsDestination { get; set; }

        /// <summary>
        /// 分岐点かどうか
        /// </summary>
        public bool IsBranchPoint => ConnectedStations.Count > 2;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Station()
        {
            Properties = new List<Property>();
            ConnectedStations = new List<Station>();
        }

        /// <summary>
        /// 駅を作成（ファクトリメソッド）
        /// </summary>
        public static Station Create(int id, string name, double x, double y, StationType type, Region region)
        {
            return new Station
            {
                Id = id,
                Name = name,
                Location = new Coordinate(x, y),
                Type = type,
                Region = region
            };
        }

        /// <summary>
        /// 物件駅を作成
        /// </summary>
        public static Station CreatePropertyStation(int id, string name, double x, double y, Region region)
        {
            return Create(id, name, x, y, StationType.Property, region);
        }

        /// <summary>
        /// 別の駅までの距離を計算
        /// </summary>
        public double DistanceTo(Station other)
        {
            if (other == null) return double.MaxValue;
            return Location.DistanceTo(other.Location);
        }

        /// <summary>
        /// 駅が特定のプレイヤーに独占されているかチェック
        /// </summary>
        public bool IsMonopolizedBy(Player player)
        {
            if (Properties.Count == 0) return false;
            return Properties.All(p => p.Owner == player);
        }

        /// <summary>
        /// 購入可能な物件数を取得
        /// </summary>
        public int GetAvailablePropertyCount()
        {
            return Properties.Count(p => p.Owner == null);
        }

        /// <summary>
        /// 特定プレイヤーが所有する物件数を取得
        /// </summary>
        public int GetPropertyCountOwnedBy(Player player)
        {
            return Properties.Count(p => p.Owner == player);
        }

        /// <summary>
        /// 駅の総物件価値を取得
        /// </summary>
        public Money GetTotalPropertyValue()
        {
            return Properties.Aggregate(Money.Zero, (total, prop) => total + prop.CurrentPrice);
        }

        /// <summary>
        /// 駅の色を取得（UI表示用）
        /// </summary>
        public string GetStationColor()
        {
            return Type switch
            {
                StationType.Property => "#4169E1",     // RoyalBlue
                StationType.CardShop => "#FF6347",     // Tomato
                StationType.Plus => "#32CD32",         // LimeGreen
                StationType.Minus => "#DC143C",        // Crimson
                StationType.NiceCard => "#FFD700",     // Gold
                StationType.SuperCard => "#FF1493",    // DeepPink
                StationType.CardExchange => "#9370DB", // MediumPurple
                StationType.Lottery => "#FFA500",      // Orange
                _ => "#808080"                         // Gray
            };
        }

        /// <summary>
        /// 短縮名を取得（地図表示用）
        /// </summary>
        public string GetShortName()
        {
            // 長い駅名は短縮
            if (Name.Length > 3)
            {
                // 「駅」を除去
                var shortName = Name.Replace("駅", "");

                // それでも長い場合は最初の3文字
                if (shortName.Length > 3)
                {
                    return shortName.Substring(0, 3);
                }

                return shortName;
            }

            return Name;
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Type}) - 物件数: {Properties.Count}";
        }
    }

    /// <summary>
    /// 駅ネットワーク（全駅を管理）
    /// </summary>
    public class StationNetwork
    {
        private readonly Dictionary<int, Station> _stations;
        private readonly List<(int from, int to)> _connections;

        public StationNetwork()
        {
            _stations = new Dictionary<int, Station>();
            _connections = new List<(int, int)>();
        }

        /// <summary>
        /// 駅を追加
        /// </summary>
        public void AddStation(Station station)
        {
            _stations[station.Id] = station;
        }

        /// <summary>
        /// 駅間の接続を追加
        /// </summary>
        public void AddConnection(int fromId, int toId)
        {
            if (_stations.TryGetValue(fromId, out var fromStation) &&
                _stations.TryGetValue(toId, out var toStation))
            {
                if (!fromStation.ConnectedStations.Contains(toStation))
                {
                    fromStation.ConnectedStations.Add(toStation);
                    toStation.ConnectedStations.Add(fromStation);
                    _connections.Add((fromId, toId));
                }
            }
        }

        /// <summary>
        /// IDから駅を取得
        /// </summary>
        public Station? GetStation(int id)
        {
            return _stations.TryGetValue(id, out var station) ? station : null;
        }

        /// <summary>
        /// 全駅を取得
        /// </summary>
        public List<Station> GetAllStations()
        {
            return _stations.Values.ToList();
        }

        /// <summary>
        /// 特定タイプの駅を取得
        /// </summary>
        public List<Station> GetStationsByType(StationType type)
        {
            return _stations.Values.Where(s => s.Type == type).ToList();
        }

        /// <summary>
        /// 特定地域の駅を取得
        /// </summary>
        public List<Station> GetStationsByRegion(Region region)
        {
            return _stations.Values.Where(s => s.Region == region).ToList();
        }

        /// <summary>
        /// 物件駅をランダムに取得
        /// </summary>
        public Station? GetRandomPropertyStation(Random? random = null)
        {
            random ??= new Random();
            var propertyStations = GetStationsByType(StationType.Property);

            if (propertyStations.Count == 0)
                return null;

            return propertyStations[random.Next(propertyStations.Count)];
        }
    }
}