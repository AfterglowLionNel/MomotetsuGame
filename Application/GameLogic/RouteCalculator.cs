using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Application.Services
{
    /// <summary>
    /// �o�H�v�Z�T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface IRouteCalculator
    {
        /// <summary>
        /// �w�肳�ꂽ�����œ��B�\�ȉw���v�Z
        /// </summary>
        List<Station> GetReachableStations(Station from, int steps);

        /// <summary>
        /// 2�w�Ԃ̍ŒZ�o�H���v�Z
        /// </summary>
        List<Station> GetShortestRoute(Station from, Station to);

        /// <summary>
        /// �w�肳�ꂽ�����ł̈ړ��o�H���v�Z
        /// </summary>
        List<Station> CalculateRoute(Station from, int steps);

        /// <summary>
        /// ����_����I�����������ł̌o�H���Čv�Z
        /// </summary>
        List<Station> RecalculateRoute(Station branchPoint, Station selectedNext, int remainingSteps);

        /// <summary>
        /// �ړI�n�܂ł̍ŒZ�������v�Z
        /// </summary>
        int GetDistanceToDestination(Station from, Station destination);
    }

    /// <summary>
    /// �o�H�v�Z�T�[�r�X�̎���
    /// </summary>
    public class RouteCalculator : IRouteCalculator
    {
        /// <summary>
        /// �w�肳�ꂽ�����œ��B�\�ȉw���v�Z
        /// </summary>
        public List<Station> GetReachableStations(Station from, int steps)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (steps < 0)
                throw new ArgumentException("������0�ȏ�ł���K�v������܂��B", nameof(steps));

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

                // �אڂ���w��T��
                foreach (var next in current.ConnectedStations)
                {
                    if (!visited.Contains(next))
                    {
                        queue.Enqueue((next, remaining - 1));

                        // �ʉ߉\�ȉw���L�^�i����I��p�j
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
        /// 2�w�Ԃ̍ŒZ�o�H���v�Z�i�_�C�N�X�g���@�j
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

            // ������
            distances[from] = 0;
            unvisited.Add(from);

            // BFS�ŒT��
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

            // �o�H�𕜌�
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
                    return new List<Station>(); // ���B�s�\
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// �w�肳�ꂽ�����ł̈ړ��o�H���v�Z
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
                    .Where(s => !route.Contains(s) || route.Count == 1) // �߂邱�Ƃ͍ŏ���1���ڂ̂݋���
                    .ToList();

                if (nextStations.Count == 0)
                {
                    // �s���~�܂�̏ꍇ�͒�~
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
                    // ����_�̏ꍇ�͈�U��~�iUI�őI����҂j
                    route.Add(null!); // ����_�}�[�J�[
                    break;
                }
            }

            return route;
        }

        /// <summary>
        /// ����_����I�����������ł̌o�H���Čv�Z
        /// </summary>
        public List<Station> RecalculateRoute(Station branchPoint, Station selectedNext, int remainingSteps)
        {
            if (branchPoint == null || selectedNext == null)
                throw new ArgumentNullException();

            if (!branchPoint.ConnectedStations.Contains(selectedNext))
                throw new ArgumentException("�I�����ꂽ�w�͕���_����ڑ�����Ă��܂���B");

            var route = new List<Station> { selectedNext };
            remainingSteps--;

            if (remainingSteps > 0)
            {
                var continuedRoute = CalculateRoute(selectedNext, remainingSteps);
                route.AddRange(continuedRoute.Skip(1)); // �ŏ��̉w�͏d������̂ŏ��O
            }

            return route;
        }

        /// <summary>
        /// �ړI�n�܂ł̍ŒZ�������v�Z
        /// </summary>
        public int GetDistanceToDestination(Station from, Station destination)
        {
            if (from == null || destination == null)
                return int.MaxValue;

            var route = GetShortestRoute(from, destination);
            return route.Count > 0 ? route.Count - 1 : int.MaxValue;
        }

        /// <summary>
        /// �o�H��̓���}�X�����o
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

                // ����w���`�F�b�N
                if (current != from && current.Type != Core.Enums.StationType.Property)
                {
                    specialStations.Add((current, distance));
                }

                // �אډw��T��
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
        /// �o�H�I���̃q���g�𐶐�
        /// </summary>
        public RouteHint GetRouteHint(Station from, Station destination, int steps)
        {
            var hint = new RouteHint();

            // �e�I�����̕]��
            foreach (var next in from.ConnectedStations)
            {
                var evaluation = new RouteEvaluation
                {
                    NextStation = next,
                    DistanceToDestination = GetDistanceToDestination(next, destination)
                };

                // ���̕����ɐi�񂾏ꍇ�̓��B�\�w���m�F
                var reachableFromNext = GetReachableStations(next, steps - 1);

                // �ǂ��w�i�v���X�w�A�J�[�h�����j�����邩�`�F�b�N
                evaluation.HasGoodStation = reachableFromNext.Any(s =>
                    s.Type == Core.Enums.StationType.Plus ||
                    s.Type == Core.Enums.StationType.CardShop);

                // �����w�i�}�C�i�X�w�j�����邩�`�F�b�N
                evaluation.HasBadStation = reachableFromNext.Any(s =>
                    s.Type == Core.Enums.StationType.Minus);

                // �ړI�n�ɓ��B�\���`�F�b�N
                evaluation.CanReachDestination = reachableFromNext.Contains(destination);

                hint.Evaluations.Add(evaluation);
            }

            return hint;
        }
    }

    /// <summary>
    /// �o�H�I���̃q���g
    /// </summary>
    public class RouteHint
    {
        public List<RouteEvaluation> Evaluations { get; set; } = new List<RouteEvaluation>();
    }

    /// <summary>
    /// �o�H�̕]��
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