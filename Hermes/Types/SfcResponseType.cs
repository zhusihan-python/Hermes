using System.ComponentModel;

namespace Hermes.Types;

public enum SfcResponseType
{
    [Description("Tarea ejecutada con exito.")]
    Ok,

    [Description("El servidor de SFC no responde, por favor contacte al departameto de IT.")]
    Timeout,

    [Description("El flujo de la unidad no corresponde a esta estación.")]
    WrongStation,

    [Description("Error de SFC no identificado.")]
    Unknown
}