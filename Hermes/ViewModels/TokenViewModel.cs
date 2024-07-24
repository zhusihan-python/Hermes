using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Hermes.Types;

namespace Hermes.ViewModels;

public partial class TokenViewModel : ViewModelBase, ITokenViewModel
{
    private const string TokenText = "Token";

    [ObservableProperty] private bool _isUnlocked;
    [ObservableProperty] private string _textFieldAssist = TokenText;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(UnlockCommand))]
    private bool _canUnlock = true;

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