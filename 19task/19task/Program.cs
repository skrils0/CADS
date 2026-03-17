using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MyTreeMap<K, V>
{
    private const bool RED = true;
    private const bool BLACK = false;

    private readonly IComparer<K> comparator;
    private Node root;
    private int size;

    private class Node
    {
        public K key;
        public V value;
        public Node left;
        public Node right;
        public Node parent;
        public bool color;

        public Node(K key, V value, bool color, Node parent)
        {
            this.key = key;
            this.value = value;
            this.color = color;
            this.parent = parent;
        }
    }

    public class MyEntry
    {
        public K Key { get; }
        public V Value { get; }

        public MyEntry(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }

    // 1. Конструктор без параметров
    public MyTreeMap()
    {
        comparator = Comparer<K>.Default;
        root = null;
        size = 0;
    }

    // 2. Конструктор с компаратором
    public MyTreeMap(IComparer<K> comparator)
    {
        if (comparator == null)
        {
            throw new ArgumentNullException(nameof(comparator));
        }

        this.comparator = comparator;
        root = null;
        size = 0;
    }

    // 3. Метод Clear
    public void Clear()
    {
        root = null;
        size = 0;
    }

    // 4. Метод IsEmpty
    public bool IsEmpty()
    {
        return size == 0;
    }

    // 5. Метод Size
    public int Size()
    {
        return size;
    }

    // 6. Метод ContainsKey
    public bool ContainsKey(object key)
    {
        if (key == null)
        {
            return false;
        }

        if (!(key is K typedKey))
        {
            return false;
        }

        return FindNode(typedKey) != null;
    }

    // 7. Метод Get
    public V Get(object key)
    {
        if (key == null)
        {
            return default(V);
        }

        if (!(key is K typedKey))
        {
            return default(V);
        }

        Node node = FindNode(typedKey);

        if (node == null)
        {
            return default(V);
        }

        return node.value;
    }

    // 8. Метод Put
    public void Put(K key, V value)
    {
        ValidateKey(key);

        if (root == null)
        {
            root = new Node(key, value, BLACK, null);
            size = 1;
            return;
        }

        Node current = root;
        Node parent = null;
        int comparison = 0;

        while (current != null)
        {
            parent = current;
            comparison = CompareKeys(key, current.key);

            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                current = current.right;
            }
            else
            {
                current.value = value;
                return;
            }
        }

        Node newNode = new Node(key, value, RED, parent);

        if (comparison < 0)
        {
            parent.left = newNode;
        }
        else
        {
            parent.right = newNode;
        }

        FixAfterInsert(newNode);
        size++;
    }

    // 9. Метод Remove
    public V Remove(object key)
    {
        if (key == null)
        {
            return default(V);
        }

        if (!(key is K typedKey))
        {
            return default(V);
        }

        Node node = FindNode(typedKey);

        if (node == null)
        {
            return default(V);
        }

        V oldValue = node.value;
        DeleteNode(node);
        size--;
        return oldValue;
    }

    // 10. Метод KeySet
    public List<K> KeySet()
    {
        List<K> keys = new List<K>();
        TraverseInOrder(root, keys);
        return keys;
    }

    // 11. Метод FirstKey
    public K FirstKey()
    {
        if (root == null)
        {
            throw new InvalidOperationException("Отображение пустое.");
        }

        return GetFirstNode(root).key;
    }

    // 12. Метод LastKey
    public K LastKey()
    {
        if (root == null)
        {
            throw new InvalidOperationException("Отображение пустое.");
        }

        return GetLastNode(root).key;
    }

    // 13. Метод LowerKey
    public K LowerKey(K key)
    {
        ValidateKey(key);

        Node node = FindLowerNode(key);

        if (node == null)
        {
            return default(K);
        }

        return node.key;
    }

    // 14. Метод FloorKey
    public K FloorKey(K key)
    {
        ValidateKey(key);

        Node node = FindFloorNode(key);

        if (node == null)
        {
            return default(K);
        }

        return node.key;
    }

    // 15. Метод HigherKey
    public K HigherKey(K key)
    {
        ValidateKey(key);

        Node node = FindHigherNode(key);

        if (node == null)
        {
            return default(K);
        }

        return node.key;
    }

    // 16. Метод CeilingKey
    public K CeilingKey(K key)
    {
        ValidateKey(key);

        Node node = FindCeilingNode(key);

        if (node == null)
        {
            return default(K);
        }

        return node.key;
    }

    // 17. Метод HeadMap
    public MyTreeMap<K, V> HeadMap(K toKey)
    {
        ValidateKey(toKey);

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        FillHeadMap(root, toKey, result);
        return result;
    }

    // 18. Метод TailMap
    public MyTreeMap<K, V> TailMap(K fromKey)
    {
        ValidateKey(fromKey);

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        FillTailMap(root, fromKey, result);
        return result;
    }

    // 19. Метод SubMap
    public MyTreeMap<K, V> SubMap(K fromKey, K toKey)
    {
        ValidateKey(fromKey);
        ValidateKey(toKey);

        if (CompareKeys(fromKey, toKey) > 0)
        {
            throw new ArgumentException("Левая граница больше правой.");
        }

        MyTreeMap<K, V> result = new MyTreeMap<K, V>(comparator);
        FillSubMap(root, fromKey, toKey, result);
        return result;
    }

    // 20. Метод PollFirstEntry
    public MyEntry PollFirstEntry()
    {
        if (root == null)
        {
            return null;
        }

        Node first = GetFirstNode(root);
        MyEntry result = new MyEntry(first.key, first.value);
        DeleteNode(first);
        size--;
        return result;
    }

    // 21. Метод PollLastEntry
    public MyEntry PollLastEntry()
    {
        if (root == null)
        {
            return null;
        }

        Node last = GetLastNode(root);
        MyEntry result = new MyEntry(last.key, last.value);
        DeleteNode(last);
        size--;
        return result;
    }

    // 22. Вспомогательный метод ValidateKey
    private void ValidateKey(K key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
    }

    // 23. Вспомогательный метод CompareKeys
    private int CompareKeys(K firstKey, K secondKey)
    {
        return comparator.Compare(firstKey, secondKey);
    }

    // 24. Вспомогательный метод FindNode
    private Node FindNode(K key)
    {
        Node current = root;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);

            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                current = current.right;
            }
            else
            {
                return current;
            }
        }

        return null;
    }

    // 25. Вспомогательный метод GetFirstNode
    private Node GetFirstNode(Node node)
    {
        Node current = node;

        while (current.left != null)
        {
            current = current.left;
        }

        return current;
    }

    // 26. Вспомогательный метод GetLastNode
    private Node GetLastNode(Node node)
    {
        Node current = node;

        while (current.right != null)
        {
            current = current.right;
        }

        return current;
    }

    // 27. Вспомогательный метод TraverseInOrder
    private void TraverseInOrder(Node node, List<K> keys)
    {
        if (node == null)
        {
            return;
        }

        TraverseInOrder(node.left, keys);
        keys.Add(node.key);
        TraverseInOrder(node.right, keys);
    }

    // 28. Вспомогательный метод FindLowerNode
    private Node FindLowerNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);

            if (comparison <= 0)
            {
                current = current.left;
            }
            else
            {
                candidate = current;
                current = current.right;
            }
        }

        return candidate;
    }

    // 29. Вспомогательный метод FindFloorNode
    private Node FindFloorNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);

            if (comparison < 0)
            {
                current = current.left;
            }
            else if (comparison > 0)
            {
                candidate = current;
                current = current.right;
            }
            else
            {
                return current;
            }
        }

        return candidate;
    }

    // 30. Вспомогательный метод FindHigherNode
    private Node FindHigherNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);

            if (comparison < 0)
            {
                candidate = current;
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }

        return candidate;
    }

    // 31. Вспомогательный метод FindCeilingNode
    private Node FindCeilingNode(K key)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareKeys(key, current.key);

            if (comparison <= 0)
            {
                candidate = current;
                current = current.left;
            }
            else
            {
                current = current.right;
            }
        }

        return candidate;
    }

    // 32. Вспомогательный метод FillHeadMap
    private void FillHeadMap(Node node, K toKey, MyTreeMap<K, V> result)
    {
        if (node == null)
        {
            return;
        }

        int comparison = CompareKeys(node.key, toKey);

        if (comparison < 0)
        {
            FillHeadMap(node.left, toKey, result);
            result.Put(node.key, node.value);
            FillHeadMap(node.right, toKey, result);
        }
        else
        {
            FillHeadMap(node.left, toKey, result);
        }
    }

    // 33. Вспомогательный метод FillTailMap
    private void FillTailMap(Node node, K fromKey, MyTreeMap<K, V> result)
    {
        if (node == null)
        {
            return;
        }

        int comparison = CompareKeys(node.key, fromKey);

        if (comparison >= 0)
        {
            FillTailMap(node.left, fromKey, result);
            result.Put(node.key, node.value);
            FillTailMap(node.right, fromKey, result);
        }
        else
        {
            FillTailMap(node.right, fromKey, result);
        }
    }

    // 34. Вспомогательный метод FillSubMap
    private void FillSubMap(Node node, K fromKey, K toKey, MyTreeMap<K, V> result)
    {
        if (node == null)
        {
            return;
        }

        int lowerComparison = CompareKeys(node.key, fromKey);
        int upperComparison = CompareKeys(node.key, toKey);

        if (lowerComparison >= 0)
        {
            FillSubMap(node.left, fromKey, toKey, result);
        }

        if (lowerComparison >= 0 && upperComparison < 0)
        {
            result.Put(node.key, node.value);
        }

        if (upperComparison < 0)
        {
            FillSubMap(node.right, fromKey, toKey, result);
        }
    }

    // 35. Вспомогательный метод ColorOf
    private bool ColorOf(Node node)
    {
        if (node == null)
        {
            return BLACK;
        }

        return node.color;
    }

    // 36. Вспомогательный метод ParentOf
    private Node ParentOf(Node node)
    {
        if (node == null)
        {
            return null;
        }

        return node.parent;
    }

    // 37. Вспомогательный метод LeftOf
    private Node LeftOf(Node node)
    {
        if (node == null)
        {
            return null;
        }

        return node.left;
    }

    // 38. Вспомогательный метод RightOf
    private Node RightOf(Node node)
    {
        if (node == null)
        {
            return null;
        }

        return node.right;
    }

    // 39. Вспомогательный метод SetColor
    private void SetColor(Node node, bool color)
    {
        if (node != null)
        {
            node.color = color;
        }
    }

    // 40. Вспомогательный метод RotateLeft
    private void RotateLeft(Node node)
    {
        if (node == null)
        {
            return;
        }

        Node rightChild = node.right;
        node.right = rightChild.left;

        if (rightChild.left != null)
        {
            rightChild.left.parent = node;
        }

        rightChild.parent = node.parent;

        if (node.parent == null)
        {
            root = rightChild;
        }
        else if (node == node.parent.left)
        {
            node.parent.left = rightChild;
        }
        else
        {
            node.parent.right = rightChild;
        }

        rightChild.left = node;
        node.parent = rightChild;
    }

    // 41. Вспомогательный метод RotateRight
    private void RotateRight(Node node)
    {
        if (node == null)
        {
            return;
        }

        Node leftChild = node.left;
        node.left = leftChild.right;

        if (leftChild.right != null)
        {
            leftChild.right.parent = node;
        }

        leftChild.parent = node.parent;

        if (node.parent == null)
        {
            root = leftChild;
        }
        else if (node == node.parent.right)
        {
            node.parent.right = leftChild;
        }
        else
        {
            node.parent.left = leftChild;
        }

        leftChild.right = node;
        node.parent = leftChild;
    }

    // 42. Вспомогательный метод FixAfterInsert
    private void FixAfterInsert(Node node)
    {
        node.color = RED;

        while (node != null && node != root && ColorOf(ParentOf(node)) == RED)
        {
            if (ParentOf(node) == LeftOf(ParentOf(ParentOf(node))))
            {
                Node uncle = RightOf(ParentOf(ParentOf(node)));

                if (ColorOf(uncle) == RED)
                {
                    SetColor(ParentOf(node), BLACK);
                    SetColor(uncle, BLACK);
                    SetColor(ParentOf(ParentOf(node)), RED);
                    node = ParentOf(ParentOf(node));
                }
                else
                {
                    if (node == RightOf(ParentOf(node)))
                    {
                        node = ParentOf(node);
                        RotateLeft(node);
                    }

                    SetColor(ParentOf(node), BLACK);
                    SetColor(ParentOf(ParentOf(node)), RED);
                    RotateRight(ParentOf(ParentOf(node)));
                }
            }
            else
            {
                Node uncle = LeftOf(ParentOf(ParentOf(node)));

                if (ColorOf(uncle) == RED)
                {
                    SetColor(ParentOf(node), BLACK);
                    SetColor(uncle, BLACK);
                    SetColor(ParentOf(ParentOf(node)), RED);
                    node = ParentOf(ParentOf(node));
                }
                else
                {
                    if (node == LeftOf(ParentOf(node)))
                    {
                        node = ParentOf(node);
                        RotateRight(node);
                    }

                    SetColor(ParentOf(node), BLACK);
                    SetColor(ParentOf(ParentOf(node)), RED);
                    RotateLeft(ParentOf(ParentOf(node)));
                }
            }
        }

        root.color = BLACK;
    }

    // 43. Вспомогательный метод Successor
    private Node Successor(Node node)
    {
        if (node == null)
        {
            return null;
        }

        if (node.right != null)
        {
            Node current = node.right;

            while (current.left != null)
            {
                current = current.left;
            }

            return current;
        }

        Node parent = node.parent;
        Node child = node;

        while (parent != null && child == parent.right)
        {
            child = parent;
            parent = parent.parent;
        }

        return parent;
    }

    // 44. Вспомогательный метод DeleteNode
    private void DeleteNode(Node node)
    {
        if (node.left != null && node.right != null)
        {
            Node successor = Successor(node);
            node.key = successor.key;
            node.value = successor.value;
            node = successor;
        }

        Node replacement;

        if (node.left != null)
        {
            replacement = node.left;
        }
        else
        {
            replacement = node.right;
        }

        if (replacement != null)
        {
            replacement.parent = node.parent;

            if (node.parent == null)
            {
                root = replacement;
            }
            else if (node == node.parent.left)
            {
                node.parent.left = replacement;
            }
            else
            {
                node.parent.right = replacement;
            }

            node.left = null;
            node.right = null;
            node.parent = null;

            if (node.color == BLACK)
            {
                FixAfterDelete(replacement);
            }
        }
        else if (node.parent == null)
        {
            root = null;
        }
        else
        {
            if (node.color == BLACK)
            {
                FixAfterDelete(node);
            }

            if (node.parent != null)
            {
                if (node == node.parent.left)
                {
                    node.parent.left = null;
                }
                else if (node == node.parent.right)
                {
                    node.parent.right = null;
                }

                node.parent = null;
            }
        }
    }

    // 45. Вспомогательный метод FixAfterDelete
    private void FixAfterDelete(Node node)
    {
        while (node != root && ColorOf(node) == BLACK)
        {
            if (node == LeftOf(ParentOf(node)))
            {
                Node brother = RightOf(ParentOf(node));

                if (ColorOf(brother) == RED)
                {
                    SetColor(brother, BLACK);
                    SetColor(ParentOf(node), RED);
                    RotateLeft(ParentOf(node));
                    brother = RightOf(ParentOf(node));
                }

                if (ColorOf(LeftOf(brother)) == BLACK && ColorOf(RightOf(brother)) == BLACK)
                {
                    SetColor(brother, RED);
                    node = ParentOf(node);
                }
                else
                {
                    if (ColorOf(RightOf(brother)) == BLACK)
                    {
                        SetColor(LeftOf(brother), BLACK);
                        SetColor(brother, RED);
                        RotateRight(brother);
                        brother = RightOf(ParentOf(node));
                    }

                    SetColor(brother, ColorOf(ParentOf(node)));
                    SetColor(ParentOf(node), BLACK);
                    SetColor(RightOf(brother), BLACK);
                    RotateLeft(ParentOf(node));
                    node = root;
                }
            }
            else
            {
                Node brother = LeftOf(ParentOf(node));

                if (ColorOf(brother) == RED)
                {
                    SetColor(brother, BLACK);
                    SetColor(ParentOf(node), RED);
                    RotateRight(ParentOf(node));
                    brother = LeftOf(ParentOf(node));
                }

                if (ColorOf(RightOf(brother)) == BLACK && ColorOf(LeftOf(brother)) == BLACK)
                {
                    SetColor(brother, RED);
                    node = ParentOf(node);
                }
                else
                {
                    if (ColorOf(LeftOf(brother)) == BLACK)
                    {
                        SetColor(RightOf(brother), BLACK);
                        SetColor(brother, RED);
                        RotateLeft(brother);
                        brother = LeftOf(ParentOf(node));
                    }

                    SetColor(brother, ColorOf(ParentOf(node)));
                    SetColor(ParentOf(node), BLACK);
                    SetColor(LeftOf(brother), BLACK);
                    RotateRight(ParentOf(node));
                    node = root;
                }
            }
        }

        SetColor(node, BLACK);
    }
}
public class MyTreeSet<E>
{
    private static readonly object PRESENT = new object();

    private MyTreeMap<E, object> m;

    // 1. Конструктор без параметров
    public MyTreeSet()
    {
        m = new MyTreeMap<E, object>();
    }

    // 2. Конструктор с компаратором
    public MyTreeSet(IComparer<E> comparator)
    {
        if (comparator == null)
        {
            throw new ArgumentNullException(nameof(comparator));
        }

        m = new MyTreeMap<E, object>(comparator);
    }

    // 3. Конструктор из массива
    public MyTreeSet(E[] array)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        m = new MyTreeMap<E, object>();
        AddAll(array);
    }

    // 4. Метод Add
    public bool Add(E element)
    {
        ValidateElement(element);

        if (m.ContainsKey(element))
        {
            return false;
        }

        m.Put(element, PRESENT);
        return true;
    }

    // 5. Метод AddAll
    public bool AddAll(E[] array)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        bool changed = false;

        for (int i = 0; i < array.Length; i++)
        {
            if (Add(array[i]))
            {
                changed = true;
            }
        }

        return changed;
    }

    // 6. Метод Clear
    public void Clear()
    {
        m.Clear();
    }

    // 7. Метод Contains
    public bool Contains(object element)
    {
        return m.ContainsKey(element);
    }

    // 8. Метод ContainsAll
    public bool ContainsAll(E[] array)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        for (int i = 0; i < array.Length; i++)
        {
            if (!Contains(array[i]))
            {
                return false;
            }
        }

        return true;
    }

    // 9. Метод IsEmpty
    public bool IsEmpty()
    {
        return m.IsEmpty();
    }

    // 10. Метод Remove
    public bool Remove(object element)
    {
        if (!m.ContainsKey(element))
        {
            return false;
        }

        m.Remove(element);
        return true;
    }

    // 11. Метод RemoveAll
    public bool RemoveAll(E[] array)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        bool changed = false;

        for (int i = 0; i < array.Length; i++)
        {
            if (Remove(array[i]))
            {
                changed = true;
            }
        }

        return changed;
    }

    // 12. Метод RetainAll
    public bool RetainAll(E[] array)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        bool changed = false;
        List<E> keys = m.KeySet();

        for (int i = 0; i < keys.Count; i++)
        {
            if (!ArrayContains(array, keys[i]))
            {
                m.Remove(keys[i]);
                changed = true;
            }
        }

        return changed;
    }

    // 13. Метод Size
    public int Size()
    {
        return m.Size();
    }

    // 14. Метод ToArray без параметров
    public object[] ToArray()
    {
        List<E> keys = m.KeySet();
        object[] result = new object[keys.Count];

        for (int i = 0; i < keys.Count; i++)
        {
            result[i] = keys[i];
        }

        return result;
    }

    // 15. Метод ToArray с массивом
    public E[] ToArray(E[] array)
    {
        List<E> keys = m.KeySet();

        if (array == null || array.Length < keys.Count)
        {
            array = new E[keys.Count];
        }

        for (int i = 0; i < keys.Count; i++)
        {
            array[i] = keys[i];
        }

        if (array.Length > keys.Count)
        {
            array[keys.Count] = default(E);
        }

        return array;
    }

    // 16. Метод First
    public E First()
    {
        return m.FirstKey();
    }

    // 17. Метод Last
    public E Last()
    {
        return m.LastKey();
    }

    // 18. Метод Lower
    public E Lower(E element)
    {
        ValidateElement(element);
        return m.LowerKey(element);
    }

    // 19. Метод Floor
    public E Floor(E element)
    {
        ValidateElement(element);
        return m.FloorKey(element);
    }

    // 20. Метод Higher
    public E Higher(E element)
    {
        ValidateElement(element);
        return m.HigherKey(element);
    }

    // 21. Метод Ceiling
    public E Ceiling(E element)
    {
        ValidateElement(element);
        return m.CeilingKey(element);
    }

    // 22. Метод PollFirst
    public E PollFirst()
    {
        MyTreeMap<E, object>.MyEntry entry = m.PollFirstEntry();

        if (entry == null)
        {
            return default(E);
        }

        return entry.Key;
    }

    // 23. Метод PollLast
    public E PollLast()
    {
        MyTreeMap<E, object>.MyEntry entry = m.PollLastEntry();

        if (entry == null)
        {
            return default(E);
        }

        return entry.Key;
    }

    // 24. Метод HeadSet
    public MyTreeSet<E> HeadSet(E toElement)
    {
        ValidateElement(toElement);

        MyTreeMap<E, object> map = m.HeadMap(toElement);
        MyTreeSet<E> result = new MyTreeSet<E>();

        foreach (E key in map.KeySet())
        {
            result.Add(key);
        }

        return result;
    }

    // 25. Метод TailSet
    public MyTreeSet<E> TailSet(E fromElement)
    {
        ValidateElement(fromElement);

        MyTreeMap<E, object> map = m.TailMap(fromElement);
        MyTreeSet<E> result = new MyTreeSet<E>();

        foreach (E key in map.KeySet())
        {
            result.Add(key);
        }

        return result;
    }

    // 26. Метод SubSet
    public MyTreeSet<E> SubSet(E fromElement, E toElement)
    {
        ValidateElement(fromElement);
        ValidateElement(toElement);

        MyTreeMap<E, object> map = m.SubMap(fromElement, toElement);
        MyTreeSet<E> result = new MyTreeSet<E>();

        foreach (E key in map.KeySet())
        {
            result.Add(key);
        }

        return result;
    }

    // 27. Вспомогательный метод ValidateElement
    private void ValidateElement(E element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }
    }

    // 28. Вспомогательный метод ArrayContains
    private bool ArrayContains(E[] array, E value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (Equals(array[i], value))
            {
                return true;
            }
        }

        return false;
    }
}
internal class Program
{
    static void Main(string[] args)
    {
        MyTreeSet<int> set = new MyTreeSet<int>();

        set.Add(10);
        set.Add(5);
        set.Add(15);
        set.Add(3);
        set.Add(7);
        set.Add(12);
        set.Add(18);

        Console.WriteLine("Размер: " + set.Size());
        Console.WriteLine("Содержит 7: " + set.Contains(7));
        Console.WriteLine("Первый: " + set.First());
        Console.WriteLine("Последний: " + set.Last());
        Console.WriteLine("Lower(10): " + set.Lower(10));
        Console.WriteLine("Floor(10): " + set.Floor(10));
        Console.WriteLine("Higher(10): " + set.Higher(10));
        Console.WriteLine("Ceiling(10): " + set.Ceiling(10));

        Console.WriteLine("Элементы:");
        object[] array = set.ToArray();

        for (int i = 0; i < array.Length; i++)
        {
            Console.WriteLine(array[i]);
        }
    }
}
