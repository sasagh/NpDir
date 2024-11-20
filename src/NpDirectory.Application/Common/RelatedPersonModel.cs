using NpDirectory.Domain.Enum;

namespace NpDirectory.Application.Common;

public class RelatedPersonModel
{
    public PersonInfoModel Person { get; set; }

    public RelationType RelationType { get; set; }
}