using CommunityToolkit.Mvvm.Messaging.Messages;
using Hermes.TokenGen.ViewModels;

namespace Hermes.TokenGen.Common.Messages;

public class OpenWindowMessage(ViewModelBase viewModel) : ValueChangedMessage<ViewModelBase>(viewModel);