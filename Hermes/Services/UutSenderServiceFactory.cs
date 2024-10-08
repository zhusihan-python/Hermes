using Hermes.Repositories;
using Hermes.Types;

namespace Hermes.Services;

public class UutSenderServiceFactory(
    UutSenderService uutSenderService,
    GkgUutSenderService gkgUutSenderService,
    ISettingsRepository settingsRepository)
{
    public UutSenderService Build()
    {
        return settingsRepository.Settings.Machine is MachineType.ScreenPrinter
            ? gkgUutSenderService
            : uutSenderService;
    }
}