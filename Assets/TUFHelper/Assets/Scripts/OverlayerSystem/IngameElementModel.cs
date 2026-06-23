using System.ComponentModel;
using System.Numerics;
using UnityEngine;
public class IngameElementModel : INotifyPropertyChanged
{
    private System.Numerics.Vector2 _position = new();
    public System.Numerics.Vector2 Position
    {
        get => _position; 
        set
        {
            _position = value;
            PropertyChanged?.Invoke(this, new(nameof(Position)));
        }
    }

    private float _scale = 0.5f;
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            PropertyChanged?.Invoke(this, new(nameof(Scale)));
        }
    }

    private bool _isShowed = true;
    public bool IsShowed
    {
        get => _isShowed;
        set
        {
            _isShowed = value;
            PropertyChanged?.Invoke(this, new(nameof(IsShowed)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
