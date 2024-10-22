using Hermes.Models;
using Hermes.Types;

namespace Hermes.Services.UutSenderService;

public class UutSenderServiceFactory(
    TriUutSenderService triUutSenderService,
    GkgUutSenderService gkgUutSenderService,
    Session session)
{
    public UutSenderService Build()
    {
        return session.Settings.Machine is MachineType.ScreenPrinter
            ? gkgUutSenderService
            : triUutSenderService;
    }
}