using System.Globalization;
using System.Runtime.InteropServices.JavaScript;

namespace Hermes.Cipher;

public class TokenGenerator
{
    public string Generate(int id, int departmentId, DateOnly date)
    {
        var key = GetKey(date);
        var cipherId = VigenereCipher.Cipher($"{departmentId:00}{id:00}", key, departmentId);
        return $"{Keywords[id % Keywords.Length]}.{cipherId}".ToUpper();
    }

    public int DecodeId(string token, int departmentId, DateOnly date)
    {
        var parts = token.Split('.');
        var tokenDecoded = VigenereCipher.Decode(parts[1], GetKey(date), departmentId);
        return 1;
    }

    private static string GetKey(DateOnly date)
    {
        return $"{date:yyyy}{GetWeekNumber(date):00}";
    }

    public static string CipherId(int id, int departmentId)
    {
        var key = $"{departmentId:00}{id:00}".Select(x => (char)((x * id) % 26 + 'A'));
        return new string(key.ToArray());
    }

    public static int GetWeekNumber(DateOnly date)
    {
        return CultureInfo.InvariantCulture.Calendar
            .GetWeekOfYear(
                date.ToDateTime(new TimeOnly()),
                CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
    }

    private static readonly string[] Keywords =
    [
        "Zeus",
        "Odin",
        "Ra",
        "Cuervo",
        "Uva",
        "Poseidon",
        "Thor",
        "Osiris",
        "Lobo",
        "Naranja",
        "Hades",
        "Loki",
        "Anubis",
        "Caballo",
        "Platano",
        "Hermes",
        "Heimdal",
        "Seth",
        "Ardilla",
        "Melon",
        "Hera",
        "Aesir",
        "Isis",
        "Pato",
        "Guayaba",
        "Hefesto",
        "Vanir",
        "Horus",
        "Ballena",
        "Pera",
        "Dioniso",
        "Freyja",
        "Toth",
        "Conejo",
        "Manzana",
        "Atenea",
        "Sif",
        "Paloma",
        "Kiwi",
        "Apolo",
        "Pato",
        "Hela",
        "Fresa",
        "Hyena",
        "Mandarina",
        "Ares",
        "Loro",
        "Fenrir",
        "Urraca",
        "Afrodita",
        "Camello",
        "Tyr",
        "Cebra",
        "Oveja",
        "Rana",
        "Cereza",
        "Panda",
        "Mula",
        "Jirafa",
        "Iguana",
        "Zorro"
    ];
}