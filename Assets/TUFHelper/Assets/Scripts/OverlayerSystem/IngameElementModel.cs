using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class IngameElementModel : INotifyPropertyChanged
{
    private Anchor _anchor = Anchor.Center;
    [ShowInOverlayerSettings("Anchor")]
    public Anchor Anchor
    {
        get => _anchor;
        set
        {
            _anchor = value;
            OnPropertyChanged();
        }
    }

    private Vector2 _position = new();

    [ShowInOverlayerSettings("Position")]
    public Vector2 Position
    {
        get => _position;
        set
        {
            if (_position.X == value.X && _position.Y == value.Y) return;
            _position = value;
            OnPropertyChanged();
        }
    }

    private float _scale = 0.5f;
    [ShowInOverlayerSettings("Scale")]
    [SettingsRange(0.1f, 1f)]
    public float Scale
    {
        get => _scale;
        set
        {
            if (Mathf.Approximately(_scale, value)) return; 
            _scale = value;
            OnPropertyChanged();
        }
    }

    private bool _isShowed = true;
    public bool IsShowed
    {
        get => _isShowed;
        set
        {
            if (_isShowed == value) return;
            _isShowed = value;
            OnPropertyChanged();
        }
    }

    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public Dictionary<string, IngameElementSettingsCategory> Categories { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public T GetCategory<T>(string key, T defaultCategory) where T : IngameElementSettingsCategory
    {
        if (!Categories.TryGetValue(key, out var category))
        {
            Categories[key] = defaultCategory;
            defaultCategory.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Categories));
            return defaultCategory;
        }

        return (T)category;
    }
}