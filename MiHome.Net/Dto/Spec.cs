using System.Text.Json.Serialization;

namespace MiHome.Net.Dto;

public class MiotBaseModel
{
    public string Type        { get; set; } = "";
    public string Description { get; set; } = "";
}

public class MiotSpec : MiotBaseModel
{
    public List<MiotService> Services { get; set; } = [];
}

public class MiotService : MiotBaseModel
{
    public int                Iid        { get; set; }
    public List<MiotProperty> Properties { get; set; } = [];
    public List<MiotAction>   Actions    { get; set; } = [];
    public List<MiotEvent>    Events     { get; set; } = [];
}

public class MiotProperty : MiotBaseModel
{
    public int          Iid    { get; set; }
    public string       Format { get; set; } = "";
    public List<string> Access { get; set; } = [];

    [JsonPropertyName("value-list")]
    public List<MiotValueList> Valuelist { get; set; } = [];

    [JsonPropertyName("value-range")]
    public float[] ValueRange { get; set; } = [];

    [JsonPropertyName("gatt-access")]
    public List<object> GattAccess { get; set; } = [];

    public string Unit { get; set; } = "";
}

public class MiotValueList
{
    public int    Value       { get; set; }
    public string Description { get; set; } = "";
}

public class MiotAction : MiotBaseModel
{
    public int      Iid { get; set; }
    public object[] In  { get; set; } = [];
    public object[] Out { get; set; } = [];
}

public class MiotEvent : MiotBaseModel
{
    public int          Iid       { get; set; }
    public List<object> Arguments { get; set; } = [];
}