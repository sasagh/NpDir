using NpDirectory.Domain.Enum;

namespace NpDirectory.Application.Common;

public class PhoneNumberModel
{
    public string Number { get; set; }
    
    public PhoneNumberType Type { get; set; }
}