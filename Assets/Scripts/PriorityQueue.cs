using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new List<(T, int)>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
        elements.Sort((x, y) => x.priority.CompareTo(y.priority)); // 優先度順にソート
    }

    public T Dequeue()
    {
        var bestItem = elements[0];
        elements.RemoveAt(0);
        return bestItem.item;
    }
}
