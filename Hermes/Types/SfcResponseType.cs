using System.ComponentModel;

namespace Hermes.Types;

public enum SfcResponseType
{
    // TODO: Translate these Task successfully executed
    [Description("msg_success_task")]
    Ok,

    [Description("msg_sfc_timeout")]
    Timeout,

    [Description("msg_wrong_station")]
    WrongStation,

    [Description("msg_sfc_unkown_error")]
    Unknown,

    [Description("msg_scan_error")]
    ScanError
}