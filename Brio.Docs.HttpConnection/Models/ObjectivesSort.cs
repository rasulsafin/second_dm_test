using System.Collections.Generic;

namespace Brio.Docs.HttpConnection.Models
{
    public class ObjectivesSort : IReadonlyObjectivesSort
    {
        public static ObjectivesSort Default => new ObjectivesSort
        {
            Sorts = new List<ObjectivesSortParameter>
            {
                new ObjectivesSortParameter
                {
                    FieldName = "CreationDate",
                    IsDescending = false,
                },
            },
        };

        public List<ObjectivesSortParameter> Sorts { get; set; } = new List<ObjectivesSortParameter>();

        IReadOnlyList<ISortParameter> IReadonlyObjectivesSort.Sorts => Sorts;
    }
}
