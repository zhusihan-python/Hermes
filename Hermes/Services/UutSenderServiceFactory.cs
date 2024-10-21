using Hermes.Repositories;
using Hermes.Types;

namespace Hermes.Services;

public class UutSenderServiceFactory(
    TriUutSenderService triUutSenderService,
    GkgUutSenderService gkgUutSenderService,
    ISettingsRepository settingsRepository)
{
    public UutSenderService Build()
    {
        return settingsRepository.Settings.Machine is MachineType.ScreenPrinter
            ? gkgUutSenderService
            : triUutSenderService;
    }
}