using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


internal class Program
{
    const int RUN_COUNT = 20;

    static void Main(string[] args)
    {
        int[] sizes = { 100000, 1000000, 10000000 };

        Console.WriteLine("Size | Operation | MyHashMap мс | MyTreeMap мс");

        for (int i = 0; i < sizes.Length; i++)
        {
            int currentSize = sizes[i];

            int[] keys = CreateKeys(currentSize);
            Shuffle(keys);

            double hashMapPutTime = MeasureHashMapPut(keys);
            double treeMapPutTime = MeasureTreeMapPut(keys);

            MyHashMap<int, int> hashMap = CreateHashMap(keys);
            MyTreeMap<int, int> treeMap = CreateTreeMap(keys);

            double hashMapGetTime = MeasureHashMapGet(hashMap, keys);
            double treeMapGetTime = MeasureTreeMapGet(treeMap, keys);

            double hashMapRemoveTime = MeasureHashMapRemove(keys);
            double treeMapRemoveTime = MeasureTreeMapRemove(keys);

            PrintResult(currentSize, "Put", hashMapPutTime, treeMapPutTime);
            PrintResult(currentSize, "Get", hashMapGetTime, treeMapGetTime);
            PrintResult(currentSize, "Remove", hashMapRemoveTime, treeMapRemoveTime);

            Console.WriteLine();
        }

        Console.WriteLine("Готово.");
    }

    static int[] CreateKeys(int size)
    {
        int[] keys = new int[size];

        for (int i = 0; i < size; i++)
        {
            keys[i] = i;
        }

        return keys;
    }

    static void Shuffle(int[] keys)
    {
        Random random = new Random(1);

        for (int i = keys.Length - 1; i > 0; i--)
        {
            int randomIndex = random.Next(i + 1);

            int temp = keys[i];
            keys[i] = keys[randomIndex];
            keys[randomIndex] = temp;
        }
    }

    static double MeasureHashMapPut(int[] keys)
    {
        long totalTime = 0;

        for (int run = 0; run < RUN_COUNT; run++)
        {
            MyHashMap<int, int> map = new MyHashMap<int, int>(keys.Length);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < keys.Length; i++)
            {
                map.Put(keys[i], keys[i]);
            }

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        return (double)totalTime / RUN_COUNT;
    }

    static double MeasureTreeMapPut(int[] keys)
    {
        long totalTime = 0;

        for (int run = 0; run < RUN_COUNT; run++)
        {
            MyTreeMap<int, int> map = new MyTreeMap<int, int>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < keys.Length; i++)
            {
                map.Put(keys[i], keys[i]);
            }

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        return (double)totalTime / RUN_COUNT;
    }

    static double MeasureHashMapGet(MyHashMap<int, int> map, int[] keys)
    {
        long totalTime = 0;

        for (int run = 0; run < RUN_COUNT; run++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < keys.Length; i++)
            {
                map.Get(keys[i]);
            }

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        return (double)totalTime / RUN_COUNT;
    }

    static double MeasureTreeMapGet(MyTreeMap<int, int> map, int[] keys)
    {
        long totalTime = 0;

        for (int run = 0; run < RUN_COUNT; run++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < keys.Length; i++)
            {
                map.Get(keys[i]);
            }

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        return (double)totalTime / RUN_COUNT;
    }

    static double MeasureHashMapRemove(int[] keys)
    {
        long totalTime = 0;

        for (int run = 0; run < RUN_COUNT; run++)
        {
            MyHashMap<int, int> map = CreateHashMap(keys);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < keys.Length; i++)
            {
                map.Remove(keys[i]);
            }

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        return (double)totalTime / RUN_COUNT;
    }

    static double MeasureTreeMapRemove(int[] keys)
    {
        long totalTime = 0;

        for (int run = 0; run < RUN_COUNT; run++)
        {
            MyTreeMap<int, int> map = CreateTreeMap(keys);

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < keys.Length; i++)
            {
                map.Remove(keys[i]);
            }

            stopwatch.Stop();
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        return (double)totalTime / RUN_COUNT;
    }

    static MyHashMap<int, int> CreateHashMap(int[] keys)
    {
        MyHashMap<int, int> map = new MyHashMap<int, int>(keys.Length);

        for (int i = 0; i < keys.Length; i++)
        {
            map.Put(keys[i], keys[i]);
        }

        return map;
    }

    static MyTreeMap<int, int> CreateTreeMap(int[] keys)
    {
        MyTreeMap<int, int> map = new MyTreeMap<int, int>();

        for (int i = 0; i < keys.Length; i++)
        {
            map.Put(keys[i], keys[i]);
        }

        return map;
    }

    static void PrintResult(int size, string operation, double hashMapTime, double treeMapTime)
    {
        Console.WriteLine(size + " | " + operation + " | " + hashMapTime + " | " + treeMapTime);
    }
}

public class MyHashMap<K, V>
{
    private class Node
    {
        public K key;
        public V value;
        public Node next;

        public Node(K key, V value)
        {
            this.key = key;
            this.value = value;
            next = null;
        }
    }

    private Node[] table;
    private int count;
    private float loadFactor;

    public MyHashMap()
    {
        table = new Node[16];
        count = 0;
        loadFactor = 0.75f;
    }

    public MyHashMap(int initialCapacity)
    {
        table = new Node[initialCapacity];
        count = 0;
        loadFactor = 0.75f;
    }

    public void Put(K key, V value)
    {
        int index = GetIndex(key);

        Node current = table[index];

        while (current != null)
        {
            if (EqualityComparer<K>.Default.Equals(current.key, key))
            {
                current.value = value;
                return;
            }

            current = current.next;
        }

        Node newNode = new Node(key, value);
        newNode.next = table[index];
        table[index] = newNode;
        count++;

        if (count > loadFactor * table.Length)
        {
            Resize();
        }
    }

    public V Get(K key)
    {
        int index = GetIndex(key);

        Node current = table[index];

        while (current != null)
        {
            if (EqualityComparer<K>.Default.Equals(current.key, key))
            {
                return current.value;
            }

            current = current.next;
        }

        return default(V);
    }

    public bool Remove(K key)
    {
        int index = GetIndex(key);

        Node current = table[index];
        Node previous = null;

        while (current != null)
        {
            if (EqualityComparer<K>.Default.Equals(current.key, key))
            {
                if (previous == null)
                {
                    table[index] = current.next;
                }
                else
                {
                    previous.next = current.next;
                }

                count--;
                return true;
            }

            previous = current;
            current = current.next;
        }

        return false;
    }

    private int GetIndex(K key)
    {
        int hash = key.GetHashCode() & 0x7FFFFFFF;
        return hash % table.Length;
    }

    private void Resize()
    {
        Node[] oldTable = table;
        table = new Node[oldTable.Length * 2];
        count = 0;

        for (int i = 0; i < oldTable.Length; i++)
        {
            Node current = oldTable[i];

            while (current != null)
            {
                Put(current.key, current.value);
                current = current.next;
            }
        }
    }
}

public class MyTreeMap<K, V> where K : IComparable<K>
{
    private class Node
    {
        public K key;
        public V value;
        public Node left;
        public Node right;

        public Node(K key, V value)
        {
            this.key = key;
            this.value = value;
        }
    }

    private Node root;
    private int count;

    public void Put(K key, V value)
    {
        if (root == null)
        {
            root = new Node(key, value);
            count++;
            return;
        }

        Node current = root;
        Node parent = null;
        int compareResult = 0;

        while (current != null)
        {
            parent = current;
            compareResult = key.CompareTo(current.key);

            if (compareResult < 0)
            {
                current = current.left;
            }
            else if (compareResult > 0)
            {
                current = current.right;
            }
            else
            {
                current.value = value;
                return;
            }
        }

        Node newNode = new Node(key, value);

        if (compareResult < 0)
        {
            parent.left = newNode;
        }
        else
        {
            parent.right = newNode;
        }

        count++;
    }

    public V Get(K key)
    {
        Node current = root;

        while (current != null)
        {
            int compareResult = key.CompareTo(current.key);

            if (compareResult == 0)
            {
                return current.value;
            }
            else if (compareResult < 0)
            {
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }

        return default(V);
    }

    public bool Remove(K key)
    {
        if (FindNode(key) == null)
        {
            return false;
        }

        root = RemoveRecursive(root, key);
        count--;
        return true;
    }

    private Node FindNode(K key)
    {
        Node current = root;

        while (current != null)
        {
            int compareResult = key.CompareTo(current.key);

            if (compareResult == 0)
            {
                return current;
            }
            else if (compareResult < 0)
            {
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }

        return null;
    }

    private Node RemoveRecursive(Node node, K key)
    {
        if (node == null)
        {
            return null;
        }

        int compareResult = key.CompareTo(node.key);

        if (compareResult < 0)
        {
            node.left = RemoveRecursive(node.left, key);
        }
        else if (compareResult > 0)
        {
            node.right = RemoveRecursive(node.right, key);
        }
        else
        {
            if (node.left == null)
            {
                return node.right;
            }

            if (node.right == null)
            {
                return node.left;
            }

            Node minimumNode = node.right;

            while (minimumNode.left != null)
            {
                minimumNode = minimumNode.left;
            }

            node.key = minimumNode.key;
            node.value = minimumNode.value;
            node.right = RemoveRecursive(node.right, minimumNode.key);
        }

        return node;
    }
}
