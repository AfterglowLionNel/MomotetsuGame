using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.Entities;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// 経路計算サービス
    /// </summary>
    public class RouteCalculator
    {
        private readonly StationNetwork _stationNetwork;

        public RouteCalculator(StationNetwork stationNetwork)
        {
            _stationNetwork = stationNetwork ?? throw new ArgumentNullException(nameof(stationNetwork));
        }

        /// <summary>
        /// 指定された歩数で到達可能な駅を計算
        /// </summary>
        public List<Station> CalculateReachableStations(Station start, int steps)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (steps <= 0) return new List<Station> { start };

            var visited = new HashSet<int>();
            var reachable = new HashSet<Station>();
            var queue = new Queue<(Station station, int remainingSteps)>();

            queue.Enqueue((start, steps));

            while (queue.Count > 0)
            {
                var (current, remaining) = queue.Dequeue();

                if (remaining == 0)
                {
                    reachable.Add(current);
                    continue;
                }

                if (visited.Contains(current.Id)) continue;
                visited.Add(current.Id);

                foreach (var next in current.ConnectedStations)
                {
                    if (!visited.Contains(next.Id))
                    {
                        queue.Enqueue((next, remaining - 1));
                        if (remaining == 1)
                        {
                            reachable.Add(next);
                        }
                    }
                }
            }

            return reachable.ToList();
        }

        /// <summary>
        /// 移動経路を計算（複数の到達可能駅がある場合は選択が必要）
        /// </summary>
        public List<Station> CalculateRoute(Station start, int steps)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (steps <= 0) return new List<Station> { start };

            var route = new List<Station> { start };
            var current = start;
            var remainingSteps = steps;

            while (remainingSteps > 0)
            {
                var nextStations = current.ConnectedStations
                    .Where(s => !route.Contains(s) || route.Count == 1) // 最初の駅には戻れる
                    .ToList();

                if (nextStations.Count == 0)
                {
                    // 行き止まりの場合
                    break;
                }
                else if (nextStations.Count == 1)
                {
                    // 選択肢が1つの場合は自動的に進む
                    current = nextStations[0];
                    route.Add(current);
                    remainingSteps--;
                }
                else
                {
                    // 分岐点（選択が必要）
                    // ここでは仮に最初の駅を選択（実際のゲームではプレイヤーが選択）
                    current = nextStations[0];
                    route.Add(current);
                    remainingSteps--;
                }
            }

            return route;
        }

        /// <summary>
        /// 2駅間の最短距離を計算（ダイクストラ法）
        /// </summary>
        public int CalculateShortestDistance(Station start, Station end)
        {
            if (start == null || end == null) return int.MaxValue;
            if (start == end) return 0;

            var distances = new Dictionary<int, int>();
            var visited = new HashSet<int>();
            var queue = new PriorityQueue<Station, int>();

            // 初期化
            foreach (var station in _stationNetwork.AllStations)
            {
                distances[station.Id] = int.MaxValue;
            }
            distances[start.Id] = 0;
            queue.Enqueue(start, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (visited.Contains(current.Id)) continue;
                visited.Add(current.Id);

                if (current.Id == end.Id)
                {
                    return distances[end.Id];
                }

                foreach (var neighbor in current.ConnectedStations)
                {
                    if (!visited.Contains(neighbor.Id))
                    {
                        var newDistance = distances[current.Id] + 1;
                        if (newDistance < distances[neighbor.Id])
                        {
                            distances[neighbor.Id] = newDistance;
                            queue.Enqueue(neighbor, newDistance);
                        }
                    }
                }
            }

            return int.MaxValue; // 到達不可能
        }

        /// <summary>
        /// 最短経路を計算
        /// </summary>
        public List<Station>? CalculateShortestPath(Station start, Station end)
        {
            if (start == null || end == null) return null;
            if (start == end) return new List<Station> { start };

            var distances = new Dictionary<int, int>();
            var previous = new Dictionary<int, Station?>();
            var visited = new HashSet<int>();
            var queue = new PriorityQueue<Station, int>();

            // 初期化
            foreach (var station in _stationNetwork.AllStations)
            {
                distances[station.Id] = int.MaxValue;
                previous[station.Id] = null;
            }
            distances[start.Id] = 0;
            queue.Enqueue(start, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (visited.Contains(current.Id)) continue;
                visited.Add(current.Id);

                if (current.Id == end.Id)
                {
                    // 経路を構築
                    var path = new List<Station>();
                    var node = end;
                    while (node != null)
                    {
                        path.Insert(0, node);
                        node = previous[node.Id];
                    }
                    return path;
                }

                foreach (var neighbor in current.ConnectedStations)
                {
                    if (!visited.Contains(neighbor.Id))
                    {
                        var newDistance = distances[current.Id] + 1;
                        if (newDistance < distances[neighbor.Id])
                        {
                            distances[neighbor.Id] = newDistance;
                            previous[neighbor.Id] = current;
                            queue.Enqueue(neighbor, newDistance);
                        }
                    }
                }
            }

            return null; // 到達不可能
        }

        /// <summary>
        /// 分岐点での選択肢を取得
        /// </summary>
        public List<RouteOption> GetRouteOptions(Station branchPoint, int remainingSteps)
        {
            if (branchPoint == null) throw new ArgumentNullException(nameof(branchPoint));

            var options = new List<RouteOption>();

            foreach (var nextStation in branchPoint.ConnectedStations)
            {
                var reachableFromNext = CalculateReachableStations(nextStation, remainingSteps - 1);

                options.Add(new RouteOption
                {
                    NextStation = nextStation,
                    ReachableStations = reachableFromNext,
                    HasPropertyStations = reachableFromNext.Any(s => s.Type == Core.Enums.StationType.Property),
                    HasPlusStations = reachableFromNext.Any(s => s.Type == Core.Enums.StationType.Plus),
                    HasCardShops = reachableFromNext.Any(s => s.HasCardShop)
                });
            }

            return options;
        }

        /// <summary>
        /// 目的地への最適ルートを提案（AI用）
        /// </summary>
        public Station? SuggestBestRoute(Station current, Station destination, int steps)
        {
            if (current == null || destination == null) return null;

            var options = current.ConnectedStations.ToList();
            if (options.Count == 0) return null;
            if (options.Count == 1) return options[0];

            // 各選択肢から目的地への距離を計算
            var optionScores = new Dictionary<Station, double>();

            foreach (var option in options)
            {
                var distanceFromOption = CalculateShortestDistance(option, destination);
                var reachableStations = CalculateReachableStations(option, steps - 1);

                // スコア計算（距離が近いほど高スコア）
                double score = 0;

                // 基本スコア（距離の逆数）
                if (distanceFromOption < int.MaxValue)
                {
                    score = 1000.0 / (distanceFromOption + 1);
                }

                // 目的地に到達可能な場合はボーナス
                if (reachableStations.Contains(destination))
                {
                    score += 500;
                }

                // 物件駅があればボーナス
                var propertyStationCount = reachableStations.Count(s => s.Type == Core.Enums.StationType.Property);
                score += propertyStationCount * 10;

                optionScores[option] = score;
            }

            // 最高スコアの選択肢を返す
            return optionScores.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
        }
    }

    /// <summary>
    /// ルート選択肢
    /// </summary>
    public class RouteOption
    {
        public Station NextStation { get; set; } = null!;
        public List<Station> ReachableStations { get; set; } = new List<Station>();
        public bool HasPropertyStations { get; set; }
        public bool HasPlusStations { get; set; }
        public bool HasCardShops { get; set; }
    }

    /// <summary>
    /// 優先度付きキュー（.NET 6.0以降で標準搭載）
    /// </summary>
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly SortedDictionary<TPriority, Queue<TElement>> _items = new();

        public int Count { get; private set; }

        public void Enqueue(TElement element, TPriority priority)
        {
            if (!_items.TryGetValue(priority, out var queue))
            {
                queue = new Queue<TElement>();
                _items[priority] = queue;
            }
            queue.Enqueue(element);
            Count++;
        }

        public TElement Dequeue()
        {
            if (Count == 0) throw new InvalidOperationException("Queue is empty");

            var firstPair = _items.First();
            var queue = firstPair.Value;
            var element = queue.Dequeue();

            if (queue.Count == 0)
            {
                _items.Remove(firstPair.Key);
            }

            Count--;
            return element;
        }
    }
}