using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Cipher.Types;
using Hermes.Language;
using System;

namespace Hermes.Features.Controls.Token;

public partial class TokenViewModel : ViewModelBase, ITokenViewModel
{
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private bool _canUnlock = true;

    [ObservableProperty] private bool _isUnlocked;
    [ObservableProperty] private string _watermark = Resources.c_token_watermark;
    public event EventHandler? Unlocked;

    public DepartmentType Department
    {
        set => this.Watermark = $"{value.ToString().ToUpper()} {Resources.c_token_watermark.ToLower()}";
    }

    [RelayCommand(CanExecute = nameof(CanExecuteUnlock))]
    private void Unlock()
    {
        // TODO: Validar token
        this.IsUnlocked = true;
        this.Unlocked?.Invoke(this, EventArgs.Empty);
    }

    private bool CanExecuteUnlock => this.CanUnlock;

    public void Reset()
    {
        this.IsUnlocked = false;
    }

    public TokenViewModel Clone()
    {
        return new TokenViewModel();
    }
}