using System.Xml.Serialization;

namespace RestAPIImageRetriever
{
    [XmlRoot(ElementName = "object")]
    public class Object
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "entityId")]
        public string EntityId { get; set; }
    }

    [XmlRoot(ElementName = "value")]
    public class Value
    {
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "xs", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xs { get; set; }
        [XmlAttribute(AttributeName = "type", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Type { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "values")]
    public class Values
    {
        [XmlElement(ElementName = "value")]
        public Value Value { get; set; }
    }

    [XmlRoot(ElementName = "row")]
    public class Row
    {
        [XmlElement(ElementName = "object")]
        public Object Object { get; set; }
        [XmlElement(ElementName = "values")]
        public Values Values { get; set; }
    }

    [XmlRoot(ElementName = "rows")]
    public class Rows
    {
        [XmlElement(ElementName = "row")]
        public Row Row { get; set; }
    }

    [XmlRoot(ElementName = "entityItemTable")]
    public class EntityItemTable
    {
        [XmlElement(ElementName = "cacheId")]
        public string CacheId { get; set; }
        [XmlElement(ElementName = "entityIdentifier")]
        public string EntityIdentifier { get; set; }
        [XmlElement(ElementName = "totalSize")]
        public string TotalSize { get; set; }
        [XmlElement(ElementName = "startIndex")]
        public string StartIndex { get; set; }
        [XmlElement(ElementName = "pageSize")]
        public string PageSize { get; set; }
        [XmlElement(ElementName = "rowCount")]
        public string RowCount { get; set; }
        [XmlElement(ElementName = "columnCount")]
        public string ColumnCount { get; set; }
        [XmlElement(ElementName = "columns")]
        public string Columns { get; set; }
        [XmlElement(ElementName = "rows")]
        public Rows Rows { get; set; }
    }
}
