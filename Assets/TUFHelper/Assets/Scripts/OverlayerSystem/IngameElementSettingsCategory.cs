using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

//[ObservableObject]
public abstract partial class IngameElementSettingsCategory : ObservableObject
{
    [JsonIgnore]
    public abstract string DisplayName { get; }
    [JsonIgnore]
    public abstract Sprite Icon { get; }

    //public event PropertyChangedEventHandler PropertyChanged;

    //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}
}
