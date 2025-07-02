using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.Entities;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// �o�H�v�Z�T�[�r�X
    /// </summary>
    public class RouteCalculator
    {
        private readonly StationNetwork _stationNetwork;

        public RouteCalculator(StationNetwork stationNetwork)
        {
            _stationNetwork = stationNetwork ?? throw new ArgumentNullException(nameof(stationNetwork));
        }

        /// <summary>
        /// �w�肳�ꂽ�����œ��B�\�ȉw���v�Z
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
        /// �ړ��o�H���v�Z�i�����̓��B�\�w������ꍇ�͑I�����K�v�j
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
                    .Where(s => !route.Contains(s) || route.Count == 1) // �ŏ��̉w�ɂ͖߂��
                    .ToList();

                if (nextStations.Count == 0)
                {
                    // �s���~�܂�̏ꍇ
                    break;
                }
                else if (nextStations.Count == 1)
                {
                    // �I������1�̏ꍇ�͎����I�ɐi��
                    current = nextStations[0];
                    route.Add(current);
                    remainingSteps--;
                }
                else
                {
                    // ����_�i�I�����K�v�j
                    // �����ł͉��ɍŏ��̉w��I���i���ۂ̃Q�[���ł̓v���C���[���I���j
                    current = nextStations[0];
                    route.Add(current);
                    remainingSteps--;
                }
            }

            return route;
        }

        /// <summary>
        /// 2�w�Ԃ̍ŒZ�������v�Z�i�_�C�N�X�g���@�j
        /// </summary>
        public int CalculateShortestDistance(Station start, Station end)
        {
            if (start == null || end == null) return int.MaxValue;
            if (start == end) return 0;

            var distances = new Dictionary<int, int>();
            var visited = new HashSet<int>();
            var queue = new PriorityQueue<Station, int>();

            // ������
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

            return int.MaxValue; // ���B�s�\
        }

        /// <summary>
        /// �ŒZ�o�H���v�Z
        /// </summary>
        public List<Station>? CalculateShortestPath(Station start, Station end)
        {
            if (start == null || end == null) return null;
            if (start == end) return new List<Station> { start };

            var distances = new Dictionary<int, int>();
            var previous = new Dictionary<int, Station?>();
            var visited = new HashSet<int>();
            var queue = new PriorityQueue<Station, int>();

            // ������
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
                    // �o�H���\�z
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

            return null; // ���B�s�\
        }

        /// <summary>
        /// ����_�ł̑I�������擾
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
        /// �ړI�n�ւ̍œK���[�g���āiAI�p�j
        /// </summary>
        public Station? SuggestBestRoute(Station current, Station destination, int steps)
        {
            if (current == null || destination == null) return null;

            var options = current.ConnectedStations.ToList();
            if (options.Count == 0) return null;
            if (options.Count == 1) return options[0];

            // �e�I��������ړI�n�ւ̋������v�Z
            var optionScores = new Dictionary<Station, double>();

            foreach (var option in options)
            {
                var distanceFromOption = CalculateShortestDistance(option, destination);
                var reachableStations = CalculateReachableStations(option, steps - 1);

                // �X�R�A�v�Z�i�������߂��قǍ��X�R�A�j
                double score = 0;

                // ��{�X�R�A�i�����̋t���j
                if (distanceFromOption < int.MaxValue)
                {
                    score = 1000.0 / (distanceFromOption + 1);
                }

                // �ړI�n�ɓ��B�\�ȏꍇ�̓{�[�i�X
                if (reachableStations.Contains(destination))
                {
                    score += 500;
                }

                // �����w������΃{�[�i�X
                var propertyStationCount = reachableStations.Count(s => s.Type == Core.Enums.StationType.Property);
                score += propertyStationCount * 10;

                optionScores[option] = score;
            }

            // �ō��X�R�A�̑I������Ԃ�
            return optionScores.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
        }
    }

    /// <summary>
    /// ���[�g�I����
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
    /// �D��x�t���L���[�i.NET 6.0�ȍ~�ŕW�����ځj
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