namespace Hermes.Models;

public class User
{
    public static readonly User Null = new NullUser();

    public int ViewLevel { get; set; }
    public int UpdateLevel { get; set; }

    public bool CanExit { get; set; }
    public bool IsNull => this == Null;
}

public class NullUser : User
{
}