using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MyTreeSet<E>
{
    private const bool RED = true;
    private const bool BLACK = false;

    private readonly IComparer<E> comparator;
    private Node root;
    private int size;

    private class Node
    {
        public E value;
        public Node left;
        public Node right;
        public Node parent;
        public bool color;

        public Node(E value, bool color, Node parent)
        {
            this.value = value;
            this.color = color;
            this.parent = parent;
        }
    }

    // 1. Конструктор без параметров
    public MyTreeSet()
    {
        comparator = Comparer<E>.Default;
        root = null;
        size = 0;
    }

    // 2. Конструктор с компаратором
    public MyTreeSet(IComparer<E> comparator)
    {
        if (comparator == null)
        {
            throw new ArgumentNullException(nameof(comparator), "Компаратор не должен быть null.");
        }

        this.comparator = comparator;
        root = null;
        size = 0;
    }

    // 3. Конструктор из массива
    public MyTreeSet(E[] a)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a), "Массив не должен быть null.");
        }

        comparator = Comparer<E>.Default;
        root = null;
        size = 0;

        AddAll(a);
    }

    // 4. Метод Add
    public bool Add(E value)
    {
        ValidateValue(value);

        if (root == null)
        {
            root = new Node(value, BLACK, null);
            size = 1;
            return true;
        }

        Node current = root;
        Node parent = null;
        int comparison = 0;

        while (current != null)
        {
            parent = current;
            comparison = CompareValues(value, current.value);

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
                return false;
            }
        }

        Node newNode = new Node(value, RED, parent);

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
        return true;
    }

    // 5. Метод AddAll
    public bool AddAll(E[] a)
    {
        if (a == null)
        {
            throw new ArgumentNullException(nameof(a), "Массив не должен быть null.");
        }

        bool changed = false;

        for (int i = 0; i < a.Length; i++)
        {
            if (Add(a[i]))
            {
                changed = true;
            }
        }

        return changed;
    }

    // 6. Метод Contains
    public bool Contains(object value)
    {
        if (value == null)
        {
            return false;
        }

        if (!(value is E typedValue))
        {
            return false;
        }

        return FindNode(typedValue) != null;
    }

    // 7. Метод Remove
    public bool Remove(object value)
    {
        if (value == null)
        {
            return false;
        }

        if (!(value is E typedValue))
        {
            return false;
        }

        Node node = FindNode(typedValue);

        if (node == null)
        {
            return false;
        }

        DeleteNode(node);
        size--;
        return true;
    }

    // 8. Метод Clear
    public void Clear()
    {
        root = null;
        size = 0;
    }

    // 9. Метод IsEmpty
    public bool IsEmpty()
    {
        return size == 0;
    }

    // 10. Метод Size
    public int Size()
    {
        return size;
    }

    // 11. Метод First
    public E First()
    {
        if (root == null)
        {
            throw new InvalidOperationException("Множество пустое.");
        }

        return GetFirstNode(root).value;
    }

    // 12. Метод Last
    public E Last()
    {
        if (root == null)
        {
            throw new InvalidOperationException("Множество пустое.");
        }

        return GetLastNode(root).value;
    }

    // 13. Метод Lower
    public E Lower(E value)
    {
        ValidateValue(value);

        Node node = FindLowerNode(value);

        if (node == null)
        {
            return default(E);
        }

        return node.value;
    }

    // 14. Метод Floor
    public E Floor(E value)
    {
        ValidateValue(value);

        Node node = FindFloorNode(value);

        if (node == null)
        {
            return default(E);
        }

        return node.value;
    }

    // 15. Метод Higher
    public E Higher(E value)
    {
        ValidateValue(value);

        Node node = FindHigherNode(value);

        if (node == null)
        {
            return default(E);
        }

        return node.value;
    }

    // 16. Метод Ceiling
    public E Ceiling(E value)
    {
        ValidateValue(value);

        Node node = FindCeilingNode(value);

        if (node == null)
        {
            return default(E);
        }

        return node.value;
    }

    // 17. Метод PollFirst
    public E PollFirst()
    {
        if (root == null)
        {
            return default(E);
        }

        Node firstNode = GetFirstNode(root);
        E result = firstNode.value;
        DeleteNode(firstNode);
        size--;

        return result;
    }

    // 18. Метод PollLast
    public E PollLast()
    {
        if (root == null)
        {
            return default(E);
        }

        Node lastNode = GetLastNode(root);
        E result = lastNode.value;
        DeleteNode(lastNode);
        size--;

        return result;
    }

    // 19. Метод ToArray
    public E[] ToArray()
    {
        List<E> list = new List<E>();
        TraverseInOrder(root, list);
        return list.ToArray();
    }

    // 20. Метод HeadSet
    public E[] HeadSet(E toElement)
    {
        ValidateValue(toElement);

        List<E> list = new List<E>();
        FillHeadSet(root, toElement, list);
        return list.ToArray();
    }

    // 21. Метод TailSet
    public E[] TailSet(E fromElement)
    {
        ValidateValue(fromElement);

        List<E> list = new List<E>();
        FillTailSet(root, fromElement, list);
        return list.ToArray();
    }

    // 22. Метод SubSet
    public E[] SubSet(E fromElement, E toElement)
    {
        ValidateValue(fromElement);
        ValidateValue(toElement);

        if (CompareValues(fromElement, toElement) > 0)
        {
            throw new ArgumentException("Левая граница должна быть меньше или равна правой.");
        }

        List<E> list = new List<E>();
        FillSubSet(root, fromElement, toElement, list);
        return list.ToArray();
    }

    // 23. Вспомогательный метод CompareValues
    private int CompareValues(E firstValue, E secondValue)
    {
        return comparator.Compare(firstValue, secondValue);
    }

    // 24. Вспомогательный метод ValidateValue
    private void ValidateValue(E value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Элемент не должен быть null.");
        }
    }

    // 25. Вспомогательный метод FindNode
    private Node FindNode(E value)
    {
        Node current = root;

        while (current != null)
        {
            int comparison = CompareValues(value, current.value);

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

    // 26. Вспомогательный метод GetFirstNode
    private Node GetFirstNode(Node node)
    {
        Node current = node;

        while (current.left != null)
        {
            current = current.left;
        }

        return current;
    }

    // 27. Вспомогательный метод GetLastNode
    private Node GetLastNode(Node node)
    {
        Node current = node;

        while (current.right != null)
        {
            current = current.right;
        }

        return current;
    }

    // 28. Вспомогательный метод TraverseInOrder
    private void TraverseInOrder(Node node, List<E> list)
    {
        if (node == null)
        {
            return;
        }

        TraverseInOrder(node.left, list);
        list.Add(node.value);
        TraverseInOrder(node.right, list);
    }

    // 29. Вспомогательный метод FillHeadSet
    private void FillHeadSet(Node node, E toElement, List<E> list)
    {
        if (node == null)
        {
            return;
        }

        int comparison = CompareValues(node.value, toElement);

        if (comparison < 0)
        {
            FillHeadSet(node.left, toElement, list);
            list.Add(node.value);
            FillHeadSet(node.right, toElement, list);
        }
        else
        {
            FillHeadSet(node.left, toElement, list);
        }
    }

    // 30. Вспомогательный метод FillTailSet
    private void FillTailSet(Node node, E fromElement, List<E> list)
    {
        if (node == null)
        {
            return;
        }

        int comparison = CompareValues(node.value, fromElement);

        if (comparison >= 0)
        {
            FillTailSet(node.left, fromElement, list);
            list.Add(node.value);
            FillTailSet(node.right, fromElement, list);
        }
        else
        {
            FillTailSet(node.right, fromElement, list);
        }
    }

    // 31. Вспомогательный метод FillSubSet
    private void FillSubSet(Node node, E fromElement, E toElement, List<E> list)
    {
        if (node == null)
        {
            return;
        }

        int lowerComparison = CompareValues(node.value, fromElement);
        int upperComparison = CompareValues(node.value, toElement);

        if (lowerComparison >= 0)
        {
            FillSubSet(node.left, fromElement, toElement, list);
        }

        if (lowerComparison >= 0 && upperComparison < 0)
        {
            list.Add(node.value);
        }

        if (upperComparison < 0)
        {
            FillSubSet(node.right, fromElement, toElement, list);
        }
    }

    // 32. Вспомогательный метод FindLowerNode
    private Node FindLowerNode(E value)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareValues(value, current.value);

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

    // 33. Вспомогательный метод FindFloorNode
    private Node FindFloorNode(E value)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareValues(value, current.value);

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

    // 34. Вспомогательный метод FindHigherNode
    private Node FindHigherNode(E value)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareValues(value, current.value);

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

    // 35. Вспомогательный метод FindCeilingNode
    private Node FindCeilingNode(E value)
    {
        Node current = root;
        Node candidate = null;

        while (current != null)
        {
            int comparison = CompareValues(value, current.value);

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

    // 36. Вспомогательный метод ColorOf
    private bool ColorOf(Node node)
    {
        if (node == null)
        {
            return BLACK;
        }

        return node.color;
    }

    // 37. Вспомогательный метод ParentOf
    private Node ParentOf(Node node)
    {
        if (node == null)
        {
            return null;
        }

        return node.parent;
    }

    // 38. Вспомогательный метод LeftOf
    private Node LeftOf(Node node)
    {
        if (node == null)
        {
            return null;
        }

        return node.left;
    }

    // 39. Вспомогательный метод RightOf
    private Node RightOf(Node node)
    {
        if (node == null)
        {
            return null;
        }

        return node.right;
    }

    // 40. Вспомогательный метод SetColor
    private void SetColor(Node node, bool color)
    {
        if (node != null)
        {
            node.color = color;
        }
    }

    // 41. Вспомогательный метод RotateLeft
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

    // 42. Вспомогательный метод RotateRight
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

    // 43. Вспомогательный метод FixAfterInsert
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

    // 44. Вспомогательный метод Successor
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

    // 45. Вспомогательный метод DeleteNode
    private void DeleteNode(Node node)
    {
        if (node.left != null && node.right != null)
        {
            Node successor = Successor(node);
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

    // 46. Вспомогательный метод FixAfterDelete
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
        int[] array = set.ToArray();

        for (int i = 0; i < array.Length; i++)
        {
            Console.WriteLine(array[i]);
        }

        Console.WriteLine("Удаляем 10: " + set.Remove(10));
        Console.WriteLine("Новый размер: " + set.Size());
    }
}