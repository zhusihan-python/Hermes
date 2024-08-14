using System.Globalization;
using System.Runtime.InteropServices.JavaScript;

namespace Hermes.Cipher;

public class TokenGenerator
{
    public string Generate(int id, int departmentId, DateOnly date)
    {
        var key = GetKey(date);
        var department = VigenereCipher.Cipher(NumberToAlphabet($"{departmentId:00}"), key);
        var alphabetId = NumberToAlphabet($"{id:00}");
        var cipherDepartmentAndId =
            VigenereCipher.Cipher($"{department}{alphabetId}", key);
        return $"{GetKeyword(id, departmentId, date)}.{cipherDepartmentAndId}".ToUpper();
    }

    public bool TryDecode(string token, int departmentId, DateOnly date, out int id)
    {
        try
        {
            var parts = token.Split('.');
            var key = GetKey(date);
            var decodedDepartmentAndId = VigenereCipher.Decode(parts[1], key);
            id = AlphabetToInt(decodedDepartmentAndId[2..]);
            var decodedDepartmentId = VigenereCipher.Decode(decodedDepartmentAndId[..2], key);
            return departmentId == AlphabetToInt(decodedDepartmentId) && GetKeyword(id, departmentId, date) == parts[0];
        }
        catch (Exception)
        {
            id = 0;
            return false;
        }
    }

    private string GetKeyword(int id, int departmentId, DateOnly date)
    {
        return Keywords[id * GetSeed(departmentId, date) % Keywords.Length];
    }

    private int GetSeed(int departmentId, DateOnly date)
    {
        return departmentId + GetWeekNumber(date) + date.Month + (date.Year - 2000);
    }

    private Tuple<DateOnly, string> _cachedKey = new(DateOnly.MinValue, "");

    private string GetKey(DateOnly date)
    {
        if (_cachedKey.Item1 != date)
        {
            _cachedKey = new Tuple<DateOnly, string>(
                date,
                NumberToAlphabet($"{GetWeekNumber(date):00}{date.Month:00}{date:yyyy}"));
        }

        return _cachedKey.Item2;
    }

    public static string NumberToAlphabet(string number)
    {
        var key = number.Select(x => (char)(x - '0' + 'A'));
        return new string(key.ToArray());
    }

    public static int AlphabetToInt(string alphabet)
    {
        return int.TryParse(AlphabetToNumber(alphabet), out var result) ? result : 0;
    }

    public static string AlphabetToNumber(string alphabet)
    {
        var key = alphabet.Select(x => (char)(x + '0' - 'A'));
        return new string(key.ToArray());
    }

    private Tuple<DateOnly, int> _cachedWeekNumber = new(DateOnly.MinValue, 0);

    public int GetWeekNumber(DateOnly date)
    {
        if (_cachedWeekNumber.Item1 != date)
        {
            _cachedWeekNumber = new Tuple<DateOnly, int>(
                date, CultureInfo.InvariantCulture.Calendar
                    .GetWeekOfYear(
                        date.ToDateTime(new TimeOnly()),
                        CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday));
        }

        return _cachedWeekNumber.Item2;
    }

    private static readonly string[] Keywords =
    [
        "ZEUS",
        "ODIN",
        "RA",
        "CUERVO",
        "UVA",
        "POSEIDON",
        "THOR",
        "OSIRIS",
        "LOBO",
        "NARANJA",
        "HADES",
        "LOKI",
        "ANUBIS",
        "CABALLO",
        "PLATANO",
        "HERMES",
        "HEIMDAL",
        "SETH",
        "ARDILLA",
        "MELON",
        "HERA",
        "AESIR",
        "ISIS",
        "PATO",
        "GUAYABA",
        "HEFESTO",
        "VANIR",
        "HORUS",
        "BALLENA",
        "PERA",
        "DIONISO",
        "FREYJA",
        "TOTH",
        "CONEJO",
        "MANZANA",
        "ATENEA",
        "SIF",
        "PALOMA",
        "KIWI",
        "APOLO",
        "PATO",
        "HELA",
        "FRESA",
        "HYENA",
        "MANDARINA",
        "ARES",
        "LORO",
        "FENRIR",
        "URRACA",
        "AFRODITA",
        "CAMELLO",
        "TYR",
        "CEBRA",
        "OVEJA",
        "RANA",
        "CEREZA",
        "PANDA",
        "MULA",
        "JIRAFA",
        "IGUANA",
        "ZORRO"
    ];
}