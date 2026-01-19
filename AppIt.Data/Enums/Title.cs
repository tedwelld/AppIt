using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AppIt.Data.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Title
    {
        None,
        Mr,
        Mrs,
        Miss,
        Ms,
    }
}
