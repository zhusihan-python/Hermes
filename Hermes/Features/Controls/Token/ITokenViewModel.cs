using CommunityToolkit.Mvvm.Input;

namespace Hermes.Features.Controls.Token;

public interface ITokenViewModel
{
    public bool CanUnlock { get; set; }
    public bool IsUnlocked { get; set; }
    public string Watermark { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public IAsyncRelayCommand UnlockCommand { get; }
}