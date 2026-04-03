using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Выберите задачу:");
        Console.WriteLine("1 - Компоненты сильной связности (алгоритм Мальгранжа)");
        Console.WriteLine("2 - Максимальный поток (проталкивание предпотока)");
        Console.WriteLine("3 - Проверка изоморфности двух графов");

        string mode = Console.ReadLine();

        if (mode == "1")
        {
            RunStronglyConnectedComponents();
        }
        else if (mode == "2")
        {
            RunMaxFlow();
        }
        else if (mode == "3")
        {
            RunGraphIsomorphism();
        }
        else
        {
            Console.WriteLine("Неверный режим.");
        }
    }

    static void RunStronglyConnectedComponents()
    {
        Console.WriteLine("Введите количество вершин и рёбер:");
        string[] firstLine = Console.ReadLine().Split();
        int vertexCount = int.Parse(firstLine[0]);
        int edgeCount = int.Parse(firstLine[1]);

        List<int>[] graph = CreateGraph(vertexCount);
        List<int>[] reverseGraph = CreateGraph(vertexCount);

        Console.WriteLine("Введите ориентированные рёбра: from to");
        for (int i = 0; i < edgeCount; i++)
        {
            string[] parts = Console.ReadLine().Split();
            int from = int.Parse(parts[0]);
            int to = int.Parse(parts[1]);

            graph[from].Add(to);
            reverseGraph[to].Add(from);
        }

        List<List<int>> components = FindStronglyConnectedComponentsMalgrange(graph, reverseGraph);

        Console.WriteLine("Количество компонент сильной связности: " + components.Count);
        for (int i = 0; i < components.Count; i++)
        {
            Console.Write("Компонента " + (i + 1) + ": ");
            for (int j = 0; j < components[i].Count; j++)
            {
                Console.Write(components[i][j] + " ");
            }
            Console.WriteLine();
        }
    }

    static List<List<int>> FindStronglyConnectedComponentsMalgrange(List<int>[] graph, List<int>[] reverseGraph)
    {
        int vertexCount = graph.Length;
        bool[] used = new bool[vertexCount];
        List<List<int>> components = new List<List<int>>();

        while (true)
        {
            int startVertex = -1;

            for (int i = 0; i < vertexCount; i++)
            {
                if (!used[i])
                {
                    startVertex = i;
                    break;
                }
            }

            if (startVertex == -1)
            {
                break;
            }

            bool[] forwardReachable = new bool[vertexCount];
            bool[] backwardReachable = new bool[vertexCount];

            Dfs(graph, startVertex, forwardReachable, used);
            Dfs(reverseGraph, startVertex, backwardReachable, used);

            List<int> component = new List<int>();

            for (int i = 0; i < vertexCount; i++)
            {
                if (!used[i] && forwardReachable[i] && backwardReachable[i])
                {
                    component.Add(i);
                }
            }

            for (int i = 0; i < component.Count; i++)
            {
                used[component[i]] = true;
            }

            components.Add(component);
        }

        return components;
    }

    static void Dfs(List<int>[] graph, int vertex, bool[] reachable, bool[] blocked)
    {
        reachable[vertex] = true;

        for (int i = 0; i < graph[vertex].Count; i++)
        {
            int nextVertex = graph[vertex][i];

            if (!reachable[nextVertex] && !blocked[nextVertex])
            {
                Dfs(graph, nextVertex, reachable, blocked);
            }
        }
    }

    static void RunMaxFlow()
    {
        Console.WriteLine("Введите количество вершин и рёбер:");
        string[] firstLine = Console.ReadLine().Split();
        int vertexCount = int.Parse(firstLine[0]);
        int edgeCount = int.Parse(firstLine[1]);

        int[,] capacity = new int[vertexCount, vertexCount];
        List<int>[] graph = CreateGraph(vertexCount);

        Console.WriteLine("Введите рёбра: from to capacity");
        for (int i = 0; i < edgeCount; i++)
        {
            string[] parts = Console.ReadLine().Split();
            int from = int.Parse(parts[0]);
            int to = int.Parse(parts[1]);
            int edgeCapacity = int.Parse(parts[2]);

            capacity[from, to] += edgeCapacity;
            graph[from].Add(to);
            graph[to].Add(from);
        }

        Console.WriteLine("Введите исток и сток:");
        string[] sourceSinkLine = Console.ReadLine().Split();
        int source = int.Parse(sourceSinkLine[0]);
        int sink = int.Parse(sourceSinkLine[1]);

        int maxFlow = PushRelabelMaxFlow(graph, capacity, source, sink);

        Console.WriteLine("Максимальный поток: " + maxFlow);
    }

    static int PushRelabelMaxFlow(List<int>[] graph, int[,] capacity, int source, int sink)
    {
        int vertexCount = graph.Length;
        int[,] flow = new int[vertexCount, vertexCount];
        int[] excess = new int[vertexCount];
        int[] height = new int[vertexCount];

        height[source] = vertexCount;

        for (int i = 0; i < graph[source].Count; i++)
        {
            int to = graph[source][i];
            int pushed = capacity[source, to];

            if (pushed > 0)
            {
                flow[source, to] += pushed;
                flow[to, source] -= pushed;
                excess[to] += pushed;
                excess[source] -= pushed;
            }
        }

        Queue<int> queue = new Queue<int>();
        bool[] inQueue = new bool[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            if (i != source && i != sink && excess[i] > 0)
            {
                queue.Enqueue(i);
                inQueue[i] = true;
            }
        }

        while (queue.Count > 0)
        {
            int vertex = queue.Dequeue();
            inQueue[vertex] = false;

            Discharge(vertex, graph, capacity, flow, excess, height, queue, inQueue, source, sink);

            if (excess[vertex] > 0)
            {
                queue.Enqueue(vertex);
                inQueue[vertex] = true;
            }
        }

        int maxFlow = 0;
        for (int i = 0; i < vertexCount; i++)
        {
            maxFlow += flow[source, i];
        }

        return maxFlow;
    }

    static void Discharge(
        int vertex,
        List<int>[] graph,
        int[,] capacity,
        int[,] flow,
        int[] excess,
        int[] height,
        Queue<int> queue,
        bool[] inQueue,
        int source,
        int sink)
    {
        while (excess[vertex] > 0)
        {
            bool pushedSomething = false;

            for (int i = 0; i < graph[vertex].Count; i++)
            {
                int to = graph[vertex][i];
                int residual = capacity[vertex, to] - flow[vertex, to];

                if (residual > 0 && height[vertex] == height[to] + 1)
                {
                    int pushValue = Math.Min(excess[vertex], residual);

                    flow[vertex, to] += pushValue;
                    flow[to, vertex] -= pushValue;
                    excess[vertex] -= pushValue;
                    excess[to] += pushValue;

                    if (to != source && to != sink && excess[to] > 0 && !inQueue[to])
                    {
                        queue.Enqueue(to);
                        inQueue[to] = true;
                    }

                    pushedSomething = true;

                    if (excess[vertex] == 0)
                    {
                        break;
                    }
                }
            }

            if (!pushedSomething)
            {
                int minHeight = int.MaxValue;

                for (int i = 0; i < graph[vertex].Count; i++)
                {
                    int to = graph[vertex][i];
                    int residual = capacity[vertex, to] - flow[vertex, to];

                    if (residual > 0)
                    {
                        if (height[to] < minHeight)
                        {
                            minHeight = height[to];
                        }
                    }
                }

                if (minHeight == int.MaxValue)
                {
                    break;
                }

                height[vertex] = minHeight + 1;
            }
        }
    }

    static void RunGraphIsomorphism()
    {
        Console.WriteLine("Введите количество вершин первого графа и число рёбер:");
        string[] firstGraphLine = Console.ReadLine().Split();
        int vertexCount1 = int.Parse(firstGraphLine[0]);
        int edgeCount1 = int.Parse(firstGraphLine[1]);

        int[,] graph1 = new int[vertexCount1, vertexCount1];

        Console.WriteLine("Введите рёбра первого графа: u v");
        for (int i = 0; i < edgeCount1; i++)
        {
            string[] parts = Console.ReadLine().Split();
            int u = int.Parse(parts[0]);
            int v = int.Parse(parts[1]);

            graph1[u, v] = 1;
            graph1[v, u] = 1;
        }

        Console.WriteLine("Введите количество вершин второго графа и число рёбер:");
        string[] secondGraphLine = Console.ReadLine().Split();
        int vertexCount2 = int.Parse(secondGraphLine[0]);
        int edgeCount2 = int.Parse(secondGraphLine[1]);

        int[,] graph2 = new int[vertexCount2, vertexCount2];

        Console.WriteLine("Введите рёбра второго графа: u v");
        for (int i = 0; i < edgeCount2; i++)
        {
            string[] parts = Console.ReadLine().Split();
            int u = int.Parse(parts[0]);
            int v = int.Parse(parts[1]);

            graph2[u, v] = 1;
            graph2[v, u] = 1;
        }

        bool isomorphic = AreGraphsIsomorphic(graph1, graph2);

        if (isomorphic)
        {
            Console.WriteLine("Графы изоморфны.");
        }
        else
        {
            Console.WriteLine("Графы не изоморфны.");
        }
    }

    static bool AreGraphsIsomorphic(int[,] graph1, int[,] graph2)
    {
        int vertexCount1 = graph1.GetLength(0);
        int vertexCount2 = graph2.GetLength(0);

        if (vertexCount1 != vertexCount2)
        {
            return false;
        }

        int edgeCount1 = CountEdges(graph1);
        int edgeCount2 = CountEdges(graph2);

        if (edgeCount1 != edgeCount2)
        {
            return false;
        }

        int[] degree1 = GetDegrees(graph1);
        int[] degree2 = GetDegrees(graph2);

        Array.Sort(degree1);
        Array.Sort(degree2);

        for (int i = 0; i < degree1.Length; i++)
        {
            if (degree1[i] != degree2[i])
            {
                return false;
            }
        }

        int vertexCount = vertexCount1;
        int[] mapping = new int[vertexCount];
        bool[] used = new bool[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            mapping[i] = -1;
        }

        return TryIsomorphism(0, graph1, graph2, mapping, used);
    }

    static bool TryIsomorphism(int currentVertex, int[,] graph1, int[,] graph2, int[] mapping, bool[] used)
    {
        int vertexCount = graph1.GetLength(0);

        if (currentVertex == vertexCount)
        {
            return true;
        }

        int[] degree1 = GetDegrees(graph1);
        int[] degree2 = GetDegrees(graph2);

        for (int candidate = 0; candidate < vertexCount; candidate++)
        {
            if (!used[candidate] && degree1[currentVertex] == degree2[candidate])
            {
                bool correct = true;

                for (int i = 0; i < currentVertex; i++)
                {
                    if (mapping[i] != -1)
                    {
                        if (graph1[currentVertex, i] != graph2[candidate, mapping[i]])
                        {
                            correct = false;
                            break;
                        }
                    }
                }

                if (correct)
                {
                    mapping[currentVertex] = candidate;
                    used[candidate] = true;

                    if (TryIsomorphism(currentVertex + 1, graph1, graph2, mapping, used))
                    {
                        return true;
                    }

                    mapping[currentVertex] = -1;
                    used[candidate] = false;
                }
            }
        }

        return false;
    }

    static int CountEdges(int[,] graph)
    {
        int vertexCount = graph.GetLength(0);
        int count = 0;

        for (int i = 0; i < vertexCount; i++)
        {
            for (int j = i + 1; j < vertexCount; j++)
            {
                if (graph[i, j] != 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    static int[] GetDegrees(int[,] graph)
    {
        int vertexCount = graph.GetLength(0);
        int[] degree = new int[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            for (int j = 0; j < vertexCount; j++)
            {
                if (graph[i, j] != 0)
                {
                    degree[i]++;
                }
            }
        }

        return degree;
    }

    static List<int>[] CreateGraph(int vertexCount)
    {
        List<int>[] graph = new List<int>[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            graph[i] = new List<int>();
        }

        return graph;
    }
}
