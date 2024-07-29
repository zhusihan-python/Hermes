using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hermes.Types;
using System;

namespace Hermes.Features.Controls.Token;

public partial class TokenViewModel : ViewModelBase, ITokenViewModel
{
    private const string TokenText = "Token";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private bool _canUnlock = true;

    [ObservableProperty] private bool _isUnlocked;
    [ObservableProperty] private string _textFieldAssist = TokenText;
    public event EventHandler? Unlocked;

    public DepartmentType Department
    {
        set => this.TextFieldAssist = $"{value.ToString().ToUpper()} {TokenText}";
    }

    [RelayCommand(CanExecute = nameof(CanExecuteUnlock))]
    private void Unlock()
    {
        this.IsUnlocked = true;
        this.Unlocked?.Invoke(this, EventArgs.Empty);
    }

    private bool CanExecuteUnlock => this.CanUnlock;

    public void Reset()
    {
        this.IsUnlocked = false;
    }
}