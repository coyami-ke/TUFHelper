using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

public abstract class IngameElementSettingsCategory : INotifyPropertyChanged
{
    [JsonIgnore]
    public abstract string DisplayName { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
