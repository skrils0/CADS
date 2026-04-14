using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            MyHashMap<string, int> tagCounts = new MyHashMap<string, int>();

            string inputFilePath = "input.txt";
            string outputFilePath = "output.txt";

            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine("Файл input.txt не найден.");
                return;
            }

            string[] lines = File.ReadAllLines(inputFilePath);

            Regex regex = new Regex(@"</?[A-Za-z][A-Za-z0-9]*>", RegexOptions.IgnoreCase);

            for (int i = 0; i < lines.Length; i++)
            {
                MatchCollection matches = regex.Matches(lines[i]);

                for (int j = 0; j < matches.Count; j++)
                {
                    string tag = NormalizeTag(matches[j].Value);

                    int? currentCount = tagCounts.Get(tag);

                    if (currentCount == null)
                    {
                        tagCounts.Put(tag, 1);
                    }
                    else
                    {
                        tagCounts.Put(tag, currentCount.Value + 1);
                    }
                }
            }

            MyHashMap<string, int>.Entry[] entries = tagCounts.EntrySet();

            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    string line = entries[i].Key + " " + entries[i].Value;
                    Console.WriteLine(line);
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine("Результат записан в output.txt");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }

    static string NormalizeTag(string tag)
    {
        if (tag == null)
        {
            return "";
        }

        tag = tag.Trim().ToLower();

        if (tag.StartsWith("</"))
        {
            tag = "<" + tag.Substring(2);
        }

        return tag;
    }
}

public class MyHashMap<K, V>
{
    public class Entry
    {
        public K Key;
        public V Value;
        public int Hash;
        public Entry Next;

        public Entry(K key, V value, int hash)
        {
            Key = key;
            Value = value;
            Hash = hash;
            Next = null;
        }
    }

    private Entry[] table;
    private int size;
    private float loadFactor;

    private const int DEFAULT_CAPACITY = 16;
    private const float DEFAULT_LOAD_FACTOR = 0.75f;

    public MyHashMap()
    {
        table = new Entry[DEFAULT_CAPACITY];
        size = 0;
        loadFactor = DEFAULT_LOAD_FACTOR;
    }

    public MyHashMap(int initialCapacity)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("Начальная ёмкость должна быть больше 0.");
        }

        table = new Entry[initialCapacity];
        size = 0;
        loadFactor = DEFAULT_LOAD_FACTOR;
    }

    public MyHashMap(int initialCapacity, float loadFactor)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("Начальная ёмкость должна быть больше 0.");
        }

        if (loadFactor <= 0)
        {
            throw new ArgumentException("Коэффициент загрузки должен быть больше 0.");
        }

        table = new Entry[initialCapacity];
        size = 0;
        this.loadFactor = loadFactor;
    }

    public void Clear()
    {
        table = new Entry[table.Length];
        size = 0;
    }

    public bool ContainsKey(object key)
    {
        Entry entry = FindEntry(key);
        return entry != null;
    }

    public bool ContainsValue(object value)
    {
        for (int i = 0; i < table.Length; i++)
        {
            Entry current = table[i];

            while (current != null)
            {
                if (AreEqual(current.Value, value))
                {
                    return true;
                }

                current = current.Next;
            }
        }

        return false;
    }

    public Entry[] EntrySet()
    {
        Entry[] result = new Entry[size];
        int index = 0;

        for (int i = 0; i < table.Length; i++)
        {
            Entry current = table[i];

            while (current != null)
            {
                result[index] = new Entry(current.Key, current.Value, current.Hash);
                index++;
                current = current.Next;
            }
        }

        return result;
    }

    public V Get(object key)
    {
        Entry entry = FindEntry(key);

        if (entry == null)
        {
            return default(V);
        }

        return entry.Value;
    }

    public bool IsEmpty()
    {
        return size == 0;
    }

    public K[] KeySet()
    {
        K[] result = new K[size];
        int index = 0;

        for (int i = 0; i < table.Length; i++)
        {
            Entry current = table[i];

            while (current != null)
            {
                result[index] = current.Key;
                index++;
                current = current.Next;
            }
        }

        return result;
    }

    public V Put(K key, V value)
    {
        EnsureCapacity();

        int hash = GetHash(key);
        int index = GetIndex(hash);

        Entry current = table[index];

        while (current != null)
        {
            if (current.Hash == hash && AreEqual(current.Key, key))
            {
                V oldValue = current.Value;
                current.Value = value;
                return oldValue;
            }

            current = current.Next;
        }

        Entry newEntry = new Entry(key, value, hash);
        newEntry.Next = table[index];
        table[index] = newEntry;
        size++;

        return default(V);
    }

    public V Remove(object key)
    {
        int hash = GetHash(key);
        int index = GetIndex(hash);

        Entry current = table[index];
        Entry previous = null;

        while (current != null)
        {
            if (current.Hash == hash && AreEqual(current.Key, key))
            {
                if (previous == null)
                {
                    table[index] = current.Next;
                }
                else
                {
                    previous.Next = current.Next;
                }

                size--;
                return current.Value;
            }

            previous = current;
            current = current.Next;
        }

        return default(V);
    }

    public int Size()
    {
        return size;
    }

    private Entry FindEntry(object key)
    {
        int hash = GetHash(key);
        int index = GetIndex(hash);

        Entry current = table[index];

        while (current != null)
        {
            if (current.Hash == hash && AreEqual(current.Key, key))
            {
                return current;
            }

            current = current.Next;
        }

        return null;
    }

    private void EnsureCapacity()
    {
        float currentLoad = (float)(size + 1) / table.Length;

        if (currentLoad > loadFactor)
        {
            Resize();
        }
    }

    private void Resize()
    {
        Entry[] oldTable = table;
        table = new Entry[oldTable.Length * 2];
        int oldSize = size;
        size = 0;

        for (int i = 0; i < oldTable.Length; i++)
        {
            Entry current = oldTable[i];

            while (current != null)
            {
                Put(current.Key, current.Value);
                current = current.Next;
            }
        }

        size = oldSize;
    }

    private int GetHash(object key)
    {
        if (key == null)
        {
            return 0;
        }

        int hash = key.GetHashCode();

        if (hash == int.MinValue)
        {
            hash = 0;
        }

        if (hash < 0)
        {
            hash = -hash;
        }

        return hash;
    }

    private int GetIndex(int hash)
    {
        return hash % table.Length;
    }

    private bool AreEqual(object first, object second)
    {
        if (first == null && second == null)
        {
            return true;
        }

        if (first == null || second == null)
        {
            return false;
        }

        return first.Equals(second);
    }
}
