using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Application.Services
{
    /// <summary>
    /// 経路計算サービスのインターフェース
    /// </summary>
    public interface IRouteCalculator
    {
        /// <summary>
        /// 指定された歩数で到達可能な駅を計算
        /// </summary>
        List<Station> GetReachableStations(Station from, int steps);

        /// <summary>
        /// 2駅間の最短経路を計算
        /// </summary>
        List<Station> GetShortestRoute(Station from, Station to);

        /// <summary>
        /// 指定された歩数での移動経路を計算
        /// </summary>
        List<Station> CalculateRoute(Station from, int steps);

        /// <summary>
        /// 分岐点から選択した方向での経路を再計算
        /// </summary>
        List<Station> RecalculateRoute(Station branchPoint, Station selectedNext, int remainingSteps);

        /// <summary>
        /// 目的地までの最短距離を計算
        /// </summary>
        int GetDistanceToDestination(Station from, Station destination);
    }

    /// <summary>
    /// 経路計算サービスの実装
    /// </summary>
    public class RouteCalculator : IRouteCalculator
    {
        /// <summary>
        /// 指定された歩数で到達可能な駅を計算
        /// </summary>
        public List<Station> GetReachableStations(Station from, int steps)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (steps < 0)
                throw new ArgumentException("歩数は0以上である必要があります。", nameof(steps));

            var reachable = new HashSet<Station>();
            var visited = new HashSet<Station>();
            var queue = new Queue<(Station station, int remainingSteps)>();

            queue.Enqueue((from, steps));

            while (queue.Count > 0)
            {
                var (current, remaining) = queue.Dequeue();

                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                if (remaining == 0)
                {
                    reachable.Add(current);
                    continue;
                }

                // 隣接する駅を探索
                foreach (var next in current.ConnectedStations)
                {
                    if (!visited.Contains(next))
                    {
                        queue.Enqueue((next, remaining - 1));

                        // 通過可能な駅も記録（分岐選択用）
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
        /// 2駅間の最短経路を計算（ダイクストラ法）
        /// </summary>
        public List<Station> GetShortestRoute(Station from, Station to)
        {
            if (from == null || to == null)
                throw new ArgumentNullException();

            if (from == to)
                return new List<Station> { from };

            var distances = new Dictionary<Station, int>();
            var previous = new Dictionary<Station, Station?>();
            var unvisited = new HashSet<Station>();

            // 初期化
            distances[from] = 0;
            unvisited.Add(from);

            // BFSで探索
            var queue = new Queue<Station>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == to)
                    break;

                foreach (var neighbor in current.ConnectedStations)
                {
                    var tentativeDistance = distances[current] + 1;

                    if (!distances.ContainsKey(neighbor) || tentativeDistance < distances[neighbor])
                    {
                        distances[neighbor] = tentativeDistance;
                        previous[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // 経路を復元
            var path = new List<Station>();
            var node = to;

            while (node != null)
            {
                path.Add(node);
                if (node == from)
                    break;

                if (previous.TryGetValue(node, out var prev))
                    node = prev;
                else
                    return new List<Station>(); // 到達不可能
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// 指定された歩数での移動経路を計算
        /// </summary>
        public List<Station> CalculateRoute(Station from, int steps)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (steps <= 0)
                return new List<Station> { from };

            var route = new List<Station> { from };
            var current = from;
            var remainingSteps = steps;

            while (remainingSteps > 0)
            {
                var nextStations = current.ConnectedStations
                    .Where(s => !route.Contains(s) || route.Count == 1) // 戻ることは最初の1歩目のみ許可
                    .ToList();

                if (nextStations.Count == 0)
                {
                    // 行き止まりの場合は停止
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
                    // 分岐点の場合は一旦停止（UIで選択を待つ）
                    route.Add(null!); // 分岐点マーカー
                    break;
                }
            }

            return route;
        }

        /// <summary>
        /// 分岐点から選択した方向での経路を再計算
        /// </summary>
        public List<Station> RecalculateRoute(Station branchPoint, Station selectedNext, int remainingSteps)
        {
            if (branchPoint == null || selectedNext == null)
                throw new ArgumentNullException();

            if (!branchPoint.ConnectedStations.Contains(selectedNext))
                throw new ArgumentException("選択された駅は分岐点から接続されていません。");

            var route = new List<Station> { selectedNext };
            remainingSteps--;

            if (remainingSteps > 0)
            {
                var continuedRoute = CalculateRoute(selectedNext, remainingSteps);
                route.AddRange(continuedRoute.Skip(1)); // 最初の駅は重複するので除外
            }

            return route;
        }

        /// <summary>
        /// 目的地までの最短距離を計算
        /// </summary>
        public int GetDistanceToDestination(Station from, Station destination)
        {
            if (from == null || destination == null)
                return int.MaxValue;

            var route = GetShortestRoute(from, destination);
            return route.Count > 0 ? route.Count - 1 : int.MaxValue;
        }

        /// <summary>
        /// 経路上の特殊マスを検出
        /// </summary>
        public List<(Station station, int distance)> FindSpecialStationsOnRoute(Station from, int maxDistance)
        {
            var specialStations = new List<(Station, int)>();
            var visited = new HashSet<Station>();
            var queue = new Queue<(Station station, int distance)>();

            queue.Enqueue((from, 0));

            while (queue.Count > 0)
            {
                var (current, distance) = queue.Dequeue();

                if (visited.Contains(current) || distance > maxDistance)
                    continue;

                visited.Add(current);

                // 特殊駅かチェック
                if (current != from && current.Type != Core.Enums.StationType.Property)
                {
                    specialStations.Add((current, distance));
                }

                // 隣接駅を探索
                foreach (var next in current.ConnectedStations)
                {
                    if (!visited.Contains(next))
                    {
                        queue.Enqueue((next, distance + 1));
                    }
                }
            }

            return specialStations.OrderBy(s => s.distance).ToList();
        }

        /// <summary>
        /// 経路選択のヒントを生成
        /// </summary>
        public RouteHint GetRouteHint(Station from, Station destination, int steps)
        {
            var hint = new RouteHint();

            // 各選択肢の評価
            foreach (var next in from.ConnectedStations)
            {
                var evaluation = new RouteEvaluation
                {
                    NextStation = next,
                    DistanceToDestination = GetDistanceToDestination(next, destination)
                };

                // この方向に進んだ場合の到達可能駅を確認
                var reachableFromNext = GetReachableStations(next, steps - 1);

                // 良い駅（プラス駅、カード売り場）があるかチェック
                evaluation.HasGoodStation = reachableFromNext.Any(s =>
                    s.Type == Core.Enums.StationType.Plus ||
                    s.Type == Core.Enums.StationType.CardShop);

                // 悪い駅（マイナス駅）があるかチェック
                evaluation.HasBadStation = reachableFromNext.Any(s =>
                    s.Type == Core.Enums.StationType.Minus);

                // 目的地に到達可能かチェック
                evaluation.CanReachDestination = reachableFromNext.Contains(destination);

                hint.Evaluations.Add(evaluation);
            }

            return hint;
        }
    }

    /// <summary>
    /// 経路選択のヒント
    /// </summary>
    public class RouteHint
    {
        public List<RouteEvaluation> Evaluations { get; set; } = new List<RouteEvaluation>();
    }

    /// <summary>
    /// 経路の評価
    /// </summary>
    public class RouteEvaluation
    {
        public Station NextStation { get; set; } = null!;
        public int DistanceToDestination { get; set; }
        public bool CanReachDestination { get; set; }
        public bool HasGoodStation { get; set; }
        public bool HasBadStation { get; set; }
    }
}