using System;
using System.IO;
using System.Text.RegularExpressions;

internal class Program
{
    private const string INPUT_FILE_NAME = "input.txt";
    private const string OUTPUT_FILE_NAME = "output.txt";
    private const string REPORT_FILE_NAME = "report.txt";

    static void Main(string[] args)
    {
        try
        {
            if (!File.Exists(INPUT_FILE_NAME))
            {
                Console.WriteLine("Файл input.txt не найден.");
                return;
            }

            string inputText = File.ReadAllText(INPUT_FILE_NAME);

            MyHashMap<string, VariableInfo> variables = new MyHashMap<string, VariableInfo>();
            TextArray variableNames = new TextArray();
            TextArray reportLines = new TextArray();

            ParseDefinitions(inputText, variables, variableNames, reportLines);

            WriteResultFile(variables, variableNames);
            WriteReportFile(reportLines);
            PrintReport(reportLines);

            Console.WriteLine("Результат записан в файл output.txt.");
            Console.WriteLine("Отчёт записан в файл report.txt.");
        }
        catch (Exception exception)
        {
            Console.WriteLine("Ошибка: " + exception.Message);
        }
    }

    private static void ParseDefinitions(string inputText, MyHashMap<string, VariableInfo> variables, TextArray variableNames, TextArray reportLines)
    {
        Regex definitionRegex = new Regex(@"([A-Za-z_][A-Za-z0-9_]*)\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*([0-9]+)\s*;");

        MatchCollection matches = definitionRegex.Matches(inputText);

        int lastIndex = 0;

        for (int i = 0; i < matches.Count; i++)
        {
            Match match = matches[i];

            if (match.Index > lastIndex)
            {
                string skippedText = inputText.Substring(lastIndex, match.Index - lastIndex);
                AddWrongText(skippedText, reportLines);
            }

            string typeName = match.Groups[1].Value;
            string variableName = match.Groups[2].Value;
            string valueText = match.Groups[3].Value;
            string definitionText = NormalizeText(match.Value);

            VariableType variableType;
            bool isCorrectType = TryParseVariableType(typeName, out variableType);

            if (!isCorrectType)
            {
                reportLines.Add("Некорректное определение: " + definitionText + " Неверный тип.");
            }
            else
            {
                if (variables.ContainsKey(variableName))
                {
                    reportLines.Add("Переопределение переменной: " + definitionText + " Первая переменная оставлена.");
                }
                else
                {
                    VariableInfo variableInfo = new VariableInfo(variableType, valueText);
                    variables.Put(variableName, variableInfo);
                    variableNames.Add(variableName);
                }
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < inputText.Length)
        {
            string skippedText = inputText.Substring(lastIndex);
            AddWrongText(skippedText, reportLines);
        }

        if (reportLines.Size() == 0)
        {
            reportLines.Add("Ошибок и переопределений нет.");
        }
    }

    private static bool TryParseVariableType(string typeName, out VariableType variableType)
    {
        if (typeName == "int")
        {
            variableType = VariableType.Int;
            return true;
        }

        if (typeName == "float")
        {
            variableType = VariableType.Float;
            return true;
        }

        if (typeName == "double")
        {
            variableType = VariableType.Double;
            return true;
        }

        variableType = VariableType.Int;
        return false;
    }

    private static void AddWrongText(string text, TextArray reportLines)
    {
        string trimmedText = text.Trim();

        if (trimmedText.Length > 0)
        {
            reportLines.Add("Некорректное определение: " + NormalizeText(trimmedText));
        }
    }

    private static string NormalizeText(string text)
    {
        string result = text.Replace("\r", " ");
        result = result.Replace("\n", " ");
        result = result.Replace("\t", " ");
        result = result.Trim();

        return result;
    }

    private static void WriteResultFile(MyHashMap<string, VariableInfo> variables, TextArray variableNames)
    {
        TextArray resultLines = new TextArray();

        for (int i = 0; i < variableNames.Size(); i++)
        {
            string variableName = variableNames.Get(i);
            VariableInfo variableInfo = variables.Get(variableName);

            string line = variableInfo.GetTypeName() + " => " + variableName + "(" + variableInfo.valueText + ")";
            resultLines.Add(line);
        }

        File.WriteAllLines(OUTPUT_FILE_NAME, resultLines.ToArray());
    }

    private static void WriteReportFile(TextArray reportLines)
    {
        File.WriteAllLines(REPORT_FILE_NAME, reportLines.ToArray());
    }

    private static void PrintReport(TextArray reportLines)
    {
        for (int i = 0; i < reportLines.Size(); i++)
        {
            Console.WriteLine(reportLines.Get(i));
        }
    }
}

enum VariableType
{
    Int,
    Float,
    Double
}

class VariableInfo
{
    public VariableType type;
    public string valueText;

    public VariableInfo(VariableType type, string valueText)
    {
        this.type = type;
        this.valueText = valueText;
    }

    public string GetTypeName()
    {
        if (type == VariableType.Int)
        {
            return "int";
        }

        if (type == VariableType.Float)
        {
            return "float";
        }

        return "double";
    }
}

class TextArray
{
    private string[] items;
    private int size;

    public TextArray()
    {
        items = new string[10];
        size = 0;
    }

    public void Add(string item)
    {
        if (size == items.Length)
        {
            Resize();
        }

        items[size] = item;
        size++;
    }

    public string Get(int index)
    {
        if (index < 0 || index >= size)
        {
            throw new IndexOutOfRangeException("Индекс выходит за границы массива.");
        }

        return items[index];
    }

    public int Size()
    {
        return size;
    }

    public string[] ToArray()
    {
        string[] result = new string[size];

        for (int i = 0; i < size; i++)
        {
            result[i] = items[i];
        }

        return result;
    }

    private void Resize()
    {
        string[] newItems = new string[items.Length * 2];

        for (int i = 0; i < items.Length; i++)
        {
            newItems[i] = items[i];
        }

        items = newItems;
    }
}

class MyMapEntry<K, V>
{
    public K Key;
    public V Value;

    public MyMapEntry(K key, V value)
    {
        Key = key;
        Value = value;
    }
}

class MyHashMap<K, V>
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
            throw new ArgumentException("Начальная ёмкость должна быть больше нуля.");
        }

        table = new Entry<K, V>[initialCapacity];
        size = 0;
        loadFactor = 0.75f;
    }

    public MyHashMap(int initialCapacity, float loadFactor)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("Начальная ёмкость должна быть больше нуля.");
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

    public bool ContainsKey(object key)
    {
        Entry<K, V> entry = FindEntry(key);

        if (entry == null)
        {
            return false;
        }

        return true;
    }

    public bool ContainsValue(object value)
    {
        for (int i = 0; i < table.Length; i++)
        {
            Entry<K, V> current = table[i];

            while (current != null)
            {
                if (object.Equals(current.Value, value))
                {
                    return true;
                }

                current = current.Next;
            }
        }

        return false;
    }

    public MyMapEntry<K, V>[] EntrySet()
    {
        MyMapEntry<K, V>[] entries = new MyMapEntry<K, V>[size];
        int position = 0;

        for (int i = 0; i < table.Length; i++)
        {
            Entry<K, V> current = table[i];

            while (current != null)
            {
                entries[position] = new MyMapEntry<K, V>(current.Key, current.Value);
                position++;

                current = current.Next;
            }
        }

        return entries;
    }

    public V Get(object key)
    {
        Entry<K, V> entry = FindEntry(key);

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
        K[] keys = new K[size];
        int position = 0;

        for (int i = 0; i < table.Length; i++)
        {
            Entry<K, V> current = table[i];

            while (current != null)
            {
                keys[position] = current.Key;
                position++;

                current = current.Next;
            }
        }

        return keys;
    }

    public V Put(K key, V value)
    {
        if ((size + 1) > table.Length * loadFactor)
        {
            Resize();
        }

        int index = GetIndex(key, table.Length);
        Entry<K, V> current = table[index];

        while (current != null)
        {
            if (object.Equals(current.Key, key))
            {
                V oldValue = current.Value;
                current.Value = value;
                return oldValue;
            }

            current = current.Next;
        }

        Entry<K, V> newEntry = new Entry<K, V>(key, value);
        newEntry.Next = table[index];
        table[index] = newEntry;
        size++;

        return default(V);
    }

    public V Remove(object key)
    {
        int index = GetIndex(key, table.Length);

        Entry<K, V> current = table[index];
        Entry<K, V> previous = null;

        while (current != null)
        {
            if (object.Equals(current.Key, key))
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

    private Entry<K, V> FindEntry(object key)
    {
        int index = GetIndex(key, table.Length);

        Entry<K, V> current = table[index];

        while (current != null)
        {
            if (object.Equals(current.Key, key))
            {
                return current;
            }

            current = current.Next;
        }

        return null;
    }

    private int GetIndex(object key, int tableLength)
    {
        int hashCode = 0;

        if (key != null)
        {
            hashCode = key.GetHashCode();
        }

        hashCode = hashCode & 0x7FFFFFFF;

        return hashCode % tableLength;
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

    private class Entry<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
        public Entry<TKey, TValue> Next;

        public Entry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            Next = null;
        }
    }
}
