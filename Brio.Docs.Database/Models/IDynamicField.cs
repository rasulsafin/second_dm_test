namespace Brio.Docs.Database.Models
{
    public interface IDynamicField
    {
        string Type { get; set; }

        string Value { get; set; }

        int? ConnectionInfoID { get; set; }
    }
}
