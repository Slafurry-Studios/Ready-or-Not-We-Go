using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private int _maxLineCount = 10;

    private readonly Queue<string> _lines = new Queue<string>();

    public void AddLine(string newLine)
    {
        if (_textMeshPro == null) return;

        if (_maxLineCount <= 0)
        {
            _lines.Clear();
            _textMeshPro.text = string.Empty;
            return;
        }

        while (_lines.Count >= _maxLineCount)
        {
            _lines.Dequeue();
        }

        _lines.Enqueue(newLine);
        _textMeshPro.text = string.Join("\n", _lines);
    }

    public void Clear()
    {
        _lines.Clear();

        if (_textMeshPro != null)
        {
            _textMeshPro.text = string.Empty;
        }
    }
}