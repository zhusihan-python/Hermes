using CommunityToolkit.Mvvm.Input;

namespace Hermes.Features.Controls.Token;

public interface ITokenViewModel
{
    public bool CanUnlock { get; set; }
    public bool IsUnlocked { get; set; }
    public string TextFieldAssist { get; set; }
    public IRelayCommand UnlockCommand { get; }
}