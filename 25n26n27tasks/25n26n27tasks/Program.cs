using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Выберите задачу:");
        Console.WriteLine("26 - сравнение строк");
        Console.WriteLine("27 - уникальные слова");

        string taskNumber = Console.ReadLine();

        if (taskNumber == "26")
        {
            RunTask26();
        }
        else if (taskNumber == "27")
        {
            RunTask27();
        }
        else
        {
            Console.WriteLine("Такой задачи в меню нет.");
        }
    }

    static void RunTask26()
    {
        string inputFileName = "input.txt";
        string outputFileName = "output26.txt";

        if (!File.Exists(inputFileName))
        {
            Console.WriteLine("Файл input.txt не найден.");
            return;
        }

        MyHashSet<LineInfo> lines = new MyHashSet<LineInfo>();
        string[] fileLines = File.ReadAllLines(inputFileName);

        for (int i = 0; i < fileLines.Length; i++)
        {
            if (fileLines[i].Trim().Length > 0)
            {
                LineInfo lineInfo = new LineInfo(fileLines[i]);
                lines.Add(lineInfo);
            }
        }

        LineInfo[] result = lines.ToArray(new LineInfo[0]);
        Array.Sort(result);

        string[] outputLines = new string[result.Length];

        for (int i = 0; i < result.Length; i++)
        {
            outputLines[i] = result[i].Text;
        }

        File.WriteAllLines(outputFileName, outputLines);
        Console.WriteLine("Задача 26 выполнена. Результат записан в output26.txt");
    }

    static void RunTask27()
    {
        string inputFileName = "input.txt";
        string outputFileName = "output27.txt";

        if (!File.Exists(inputFileName))
        {
            Console.WriteLine("Файл input.txt не найден.");
            return;
        }

        MyHashSet<string> words = new MyHashSet<string>();
        string text = File.ReadAllText(inputFileName);
        MatchCollection matches = Regex.Matches(text, "[A-Za-z]+");

        for (int i = 0; i < matches.Count; i++)
        {
            string word = matches[i].Value.ToLower();
            words.Add(word);
        }

        string[] result = words.ToArray(new string[0]);
        Array.Sort(result);

        File.WriteAllLines(outputFileName, result);
        Console.WriteLine("Задача 27 выполнена. Результат записан в output27.txt");
    }
}

public class MyHashSet<T>
{
    private MyHashMap<T, object> map;
    private static readonly object PRESENT = new object();

    public MyHashSet()
    {
        map = new MyHashMap<T, object>();
    }

    public MyHashSet(T[] a)
    {
        map = new MyHashMap<T, object>();
        AddAll(a);
    }

    public MyHashSet(int initialCapacity, float loadFactor)
    {
        map = new MyHashMap<T, object>(initialCapacity, loadFactor);
    }

    public MyHashSet(int initialCapacity)
    {
        map = new MyHashMap<T, object>(initialCapacity);
    }

    public bool Add(T e)
    {
        if (map.ContainsKey(e))
        {
            return false;
        }

        map.Put(e, PRESENT);
        return true;
    }

    public bool AddAll(T[] a)
    {
        if (a == null)
        {
            throw new ArgumentNullException("a");
        }

        bool wasChanged = false;

        for (int i = 0; i < a.Length; i++)
        {
            if (Add(a[i]))
            {
                wasChanged = true;
            }
        }

        return wasChanged;
    }

    public void Clear()
    {
        map.Clear();
    }

    public bool Contains(object o)
    {
        if (o is T)
        {
            return map.ContainsKey((T)o);
        }

        if (o == null)
        {
            return map.ContainsKey(default(T));
        }

        return false;
    }

    public bool ContainsAll(T[] a)
    {
        if (a == null)
        {
            throw new ArgumentNullException("a");
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (!Contains(a[i]))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsEmpty()
    {
        return map.IsEmpty();
    }

    public bool Remove(object o)
    {
        if (o is T)
        {
            return map.Remove((T)o);
        }

        if (o == null)
        {
            return map.Remove(default(T));
        }

        return false;
    }

    public bool RemoveAll(T[] a)
    {
        if (a == null)
        {
            throw new ArgumentNullException("a");
        }

        bool wasChanged = false;

        for (int i = 0; i < a.Length; i++)
        {
            if (Remove(a[i]))
            {
                wasChanged = true;
            }
        }

        return wasChanged;
    }

    public bool RetainAll(T[] a)
    {
        if (a == null)
        {
            throw new ArgumentNullException("a");
        }

        T[] currentElements = ToArray(new T[0]);
        bool wasChanged = false;

        for (int i = 0; i < currentElements.Length; i++)
        {
            bool needSave = false;

            for (int j = 0; j < a.Length; j++)
            {
                if (EqualityComparer<T>.Default.Equals(currentElements[i], a[j]))
                {
                    needSave = true;
                    break;
                }
            }

            if (!needSave)
            {
                Remove(currentElements[i]);
                wasChanged = true;
            }
        }

        return wasChanged;
    }

    public int Size()
    {
        return map.Size();
    }

    public object[] ToArray()
    {
        T[] elements = map.KeysToArray();
        object[] result = new object[elements.Length];

        for (int i = 0; i < elements.Length; i++)
        {
            result[i] = elements[i];
        }

        return result;
    }

    public T[] ToArray(T[] a)
    {
        T[] elements = map.KeysToArray();

        if (a == null || a.Length < elements.Length)
        {
            a = new T[elements.Length];
        }

        for (int i = 0; i < elements.Length; i++)
        {
            a[i] = elements[i];
        }

        return a;
    }
}

public class MyHashMap<K, V>
{
    private Entry<K, V>[] table;
    private int size;
    private float loadFactor;

    public MyHashMap()
    {
        table = new Entry<K, V>[16];
        size = 0;
        loadFactor = 0.75f;
    }

    public MyHashMap(int initialCapacity)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("Начальная емкость должна быть больше нуля.");
        }

        table = new Entry<K, V>[initialCapacity];
        size = 0;
        loadFactor = 0.75f;
    }

    public MyHashMap(int initialCapacity, float loadFactor)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("Начальная емкость должна быть больше нуля.");
        }

        if (loadFactor <= 0)
        {
            throw new ArgumentException("Коэффициент загрузки должен быть больше нуля.");
        }

        table = new Entry<K, V>[initialCapacity];
        size = 0;
        this.loadFactor = loadFactor;
    }

    public void Clear()
    {
        table = new Entry<K, V>[table.Length];
        size = 0;
    }

    public bool ContainsKey(K key)
    {
        int index = GetIndex(key, table.Length);
        Entry<K, V> current = table[index];

        while (current != null)
        {
            if (EqualityComparer<K>.Default.Equals(current.Key, key))
            {
                return true;
            }

            current = current.Next;
        }

        return false;
    }

    public bool ContainsValue(V value)
    {
        for (int i = 0; i < table.Length; i++)
        {
            Entry<K, V> current = table[i];

            while (current != null)
            {
                if (EqualityComparer<V>.Default.Equals(current.Value, value))
                {
                    return true;
                }

                current = current.Next;
            }
        }

        return false;
    }

    public V Get(K key)
    {
        int index = GetIndex(key, table.Length);
        Entry<K, V> current = table[index];

        while (current != null)
        {
            if (EqualityComparer<K>.Default.Equals(current.Key, key))
            {
                return current.Value;
            }

            current = current.Next;
        }

        return default(V);
    }

    public bool IsEmpty()
    {
        return size == 0;
    }

    public void Put(K key, V value)
    {
        if (size + 1 > table.Length * loadFactor)
        {
            Resize();
        }

        int index = GetIndex(key, table.Length);
        Entry<K, V> current = table[index];

        while (current != null)
        {
            if (EqualityComparer<K>.Default.Equals(current.Key, key))
            {
                current.Value = value;
                return;
            }

            current = current.Next;
        }

        Entry<K, V> newEntry = new Entry<K, V>(key, value);
        newEntry.Next = table[index];
        table[index] = newEntry;
        size++;
    }

    public bool Remove(K key)
    {
        int index = GetIndex(key, table.Length);
        Entry<K, V> current = table[index];
        Entry<K, V> previous = null;

        while (current != null)
        {
            if (EqualityComparer<K>.Default.Equals(current.Key, key))
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
                return true;
            }

            previous = current;
            current = current.Next;
        }

        return false;
    }

    public int Size()
    {
        return size;
    }

    public Entry<K, V>[] EntrySet()
    {
        Entry<K, V>[] result = new Entry<K, V>[size];
        int resultIndex = 0;

        for (int i = 0; i < table.Length; i++)
        {
            Entry<K, V> current = table[i];

            while (current != null)
            {
                result[resultIndex] = current;
                resultIndex++;
                current = current.Next;
            }
        }

        return result;
    }

    public K[] KeysToArray()
    {
        K[] result = new K[size];
        int resultIndex = 0;

        for (int i = 0; i < table.Length; i++)
        {
            Entry<K, V> current = table[i];

            while (current != null)
            {
                result[resultIndex] = current.Key;
                resultIndex++;
                current = current.Next;
            }
        }

        return result;
    }

    private int GetIndex(K key, int length)
    {
        if (key == null)
        {
            return 0;
        }

        int hashCode = key.GetHashCode();

        if (hashCode < 0)
        {
            hashCode = -hashCode;
        }

        return hashCode % length;
    }

    private void Resize()
    {
        Entry<K, V>[] oldTable = table;
        table = new Entry<K, V>[oldTable.Length * 2];
        size = 0;

        for (int i = 0; i < oldTable.Length; i++)
        {
            Entry<K, V> current = oldTable[i];

            while (current != null)
            {
                Put(current.Key, current.Value);
                current = current.Next;
            }
        }
    }
}

public class Entry<K, V>
{
    public K Key { get; set; }
    public V Value { get; set; }
    public Entry<K, V> Next { get; set; }

    public Entry(K key, V value)
    {
        Key = key;
        Value = value;
        Next = null;
    }
}

public class LineInfo : IComparable<LineInfo>
{
    private int[] wordLengths;

    public string Text { get; private set; }

    public LineInfo(string text)
    {
        Text = text;
        string[] words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        wordLengths = new int[words.Length];

        for (int i = 0; i < words.Length; i++)
        {
            wordLengths[i] = words[i].Length;
        }

        Array.Sort(wordLengths);
    }

    public int CompareTo(LineInfo other)
    {
        if (other == null)
        {
            return 1;
        }

        int minLength = wordLengths.Length;

        if (other.wordLengths.Length < minLength)
        {
            minLength = other.wordLengths.Length;
        }

        for (int i = 0; i < minLength; i++)
        {
            if (wordLengths[i] < other.wordLengths[i])
            {
                return -1;
            }

            if (wordLengths[i] > other.wordLengths[i])
            {
                return 1;
            }
        }

        if (wordLengths.Length < other.wordLengths.Length)
        {
            return -1;
        }

        if (wordLengths.Length > other.wordLengths.Length)
        {
            return 1;
        }

        return 0;
    }

    public override bool Equals(object obj)
    {
        LineInfo other = obj as LineInfo;

        if (other == null)
        {
            return false;
        }

        return CompareTo(other) == 0;
    }

    public override int GetHashCode()
    {
        int hashCode = 17;

        for (int i = 0; i < wordLengths.Length; i++)
        {
            hashCode = hashCode * 31 + wordLengths[i];
        }

        return hashCode;
    }
}

