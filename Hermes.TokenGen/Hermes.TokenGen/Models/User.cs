using CommunityToolkit.Mvvm.ComponentModel;
using Hermes.Cipher.Types;

namespace Hermes.TokenGen.Models;

public partial class User : ObservableObject
{
    [ObservableProperty] private string _number = "0";
    public string Name { get; set; } = "";
    public virtual DepartmentType Department { get; set; }
    public bool IsManager { get; set; }
}