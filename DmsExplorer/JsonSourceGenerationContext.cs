using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DmsExplorer;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<ushort, object>))]
[JsonSerializable(typeof(List<Dictionary<ushort, object>>))]
internal partial class JsonSourceGenerationContext : JsonSerializerContext
{
}
