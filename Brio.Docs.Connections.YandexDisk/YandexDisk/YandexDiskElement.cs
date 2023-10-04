using System;
using System.Collections.Generic;
using System.Xml;
using Brio.Docs.External.Utils;

namespace Brio.Docs.Integration
{
    public class YandexDiskElement : CloudElement
    {
        private string lastModifiedString;
        private string contentLengthString;

        public string MulcaFileUrl { get; private set; }

        public string MulcaDigestUrl { get; private set; }

        public string ETag { get; private set; }

        #region Create
        public static List<YandexDiskElement> GetElements(XmlElement root)
        {
            var result = new List<YandexDiskElement>();
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    if (element.Name == "d:response")
                    {
                        YandexDiskElement item = GetElement(element);
                        result.Add(item);
                    }
                    else
                    {
                        throw new XmlException($"GetElements: Неизвестный тег [{element.Name}]");
                    }
                }
                else
                {
                    throw new XmlException($"GetElements: Неизвестный тип [{node.GetType().Name}]");
                }
            }

            return result;
        }

        public static YandexDiskElement GetElement(XmlElement root)
        {
            YandexDiskElement result = new YandexDiskElement();
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:href":
                            result.Href = GetValueElement(element);
                            break;
                        case "d:propstat":
                            GetPropStatus(result, element);
                            break;
                        case "d:response":
                            result = GetElement(element);
                            break;
                        default:
                            throw new XmlException($"GetElement: Неизвестный тег [{element.Name}]");
                    }
                }
                else
                {
                    throw new XmlException($"GetElement: Неизвестный тип [{node.GetType().Name}]");
                }
            }

            return result;
        }

        internal void SetHref(string href)
            => Href = href;

        private static string GetValueElement(XmlElement element)
        {
            return Uri.UnescapeDataString(element.InnerText);
        }

        private static void GetPropStatus(YandexDiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:status":
                            result.Status = GetValueElement(element);
                            break;
                        case "d:prop":
                            GetProp(result, element);
                            break;
                        default:
                            throw new XmlException($"GetPropStatus: Неизвестный тег [{element.Name}]");
                    }
                }
                else
                {
                    throw new XmlException($"GetPropstat: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }

        private static void GetProp(YandexDiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:creationdate":
                            result.CreationDate = GetDateElement(element);
                            break;
                        case "d:displayname":
                            result.DisplayName = GetValueElement(element);
                            break;
                        case "d:getcontentlength":
                            result.ContentLength = GetUlongElement(element);
                            break;
                        case "d:getlastmodified":
                            result.LastModified = GetDateElement(element);
                            break;
                        case "d:resourcetype":
                            GetResourceType(result, element);
                            break;
                        case "d:getcontenttype":
                            result.ContentType = GetValueElement(element);
                            break;
                        case "mulca_file_url":
                            result.MulcaFileUrl = GetValueElement(element);
                            break;
                        case "d:getetag":
                            result.ETag = GetValueElement(element);
                            break;
                        case "file_url":
                            result.FileUrl = GetValueElement(element);
                            break;
                        case "mulca_digest_url":
                            result.MulcaDigestUrl = GetValueElement(element);
                            break;
                        default:
                            throw new XmlException($"GetProp: Неизвестный тег [{element.Name}]");
                    }
                }
                else
                {
                    throw new XmlException($"GetProp: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }

        private static ulong GetUlongElement(XmlElement element)
        {
            var str = GetValueElement(element);
            if (ulong.TryParse(str, out ulong num))
            {
                return num;
            }

            return ulong.MinValue;
        }

        private static DateTime GetDateElement(XmlElement element)
        {
            var str = GetValueElement(element);
            if (DateTime.TryParse(str, out DateTime date))
            {
                return date;
            }

            return DateTime.MinValue;
        }

        private static void GetResourceType(YandexDiskElement result, XmlElement root)
        {
            foreach (var node in root.ChildNodes)
            {
                if (node is XmlElement element)
                {
                    switch (element.Name)
                    {
                        case "d:collection":
                            result.IsDirectory = true;
                            break;
                        default:
                            throw new XmlException($"GetResourcetype: Неизвестный тег [{element.Name}]");
                    }
                }
                else if (node is XmlText text)
                {
                    result.ResourceType = text.Value;
                }
                else
                {
                    throw new XmlException($"GetResourcetype: Неизвестный тип [{node.GetType().Name}]");
                }
            }
        }
        #endregion
    }
}
