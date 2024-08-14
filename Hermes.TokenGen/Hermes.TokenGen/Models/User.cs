using Hermes.Cipher.Types;

namespace Hermes.TokenGen.Models;

public class User
{
    public string EmployeeNumber { get; set; } = "0";
    public DepartmentType Department { get; set; }
    public bool IsManager { get; set; }
}