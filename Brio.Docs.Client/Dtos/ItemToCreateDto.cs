using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Client.Dtos
{
    public struct ItemToCreateDto
    {
        public string RelativePath { get; }

        public string ExternalItemId { get; set; }

        public ItemType ItemType { get; }

        public ItemToCreateDto(string relativePath, string externalId, ItemType itemType)
        {
            RelativePath = relativePath;
            ExternalItemId = externalId;
            ItemType = itemType;
        }
    }
}
