using System;
using EasyNetQ.Management.Client.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasyNetQ.Management.Client.Serialization
{
    public class PropertyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            var prop = value as Properties;
            if (prop != null)
            {
                foreach (var currObj in prop)
                {
                    switch (currObj.Key)
                    {
                        case "content_type":
                        case "content_encoding":
                        case "correlation_id":
                        case "reply_to":
                        case "expiration":
                        case "message_id":
                        case "type":
                        case "user_id":
                        case "app_id":
                        case "cluster_id":
                            writer.WritePropertyName(currObj.Key);
                            writer.WriteValue(currObj.Value);
                            break;
                        case "priority":
                        case "timestamp":
                        case "delivery_mode":
                            int val;
                            if (Int32.TryParse(currObj.Value, out val))
                            {
                                writer.WritePropertyName(currObj.Key);
                                writer.WriteValue(val);
                            }
                            break;
                        case "headers":
                            continue;
                        default:
                            throw new Exception("unsupported property: " + currObj.Key );
                    }
                }
                if (prop.Headers.Count > 0)
                {
                    writer.WritePropertyName("headers");
                    writer.WriteStartObject();
                    foreach (var currObj in prop.Headers)
                    {
                        writer.WritePropertyName(currObj.Key);
                        writer.WriteValue(currObj.Value);
                    }

                    writer.WriteEndObject();
                }
            }
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.ReadFrom(reader);

            if (jToken.Type == JTokenType.Array)
            {
                return new Properties();
            }
            
            if (jToken.Type == JTokenType.Object)
            {
                var properties = new Properties();
                foreach (var property in ((JObject)jToken).Properties())
                {
                    if(property.Name == "headers")
                    {
                        if (property.Value.Type == JTokenType.Object)
                        {
                            var headers = (JObject) property.Value;
                            foreach (var header in headers.Properties())
                            {
                                properties.Headers.Add(header.Name, header.Value.ToString());
                            }
                        }
                    }
                    else
                    {
                        properties.Add(property.Name, property.Value.ToString());
                    }
                }
                return properties;
            }

            throw new JsonException(
                string.Format("Expected array or object for properties, but was {0}", jToken.Type), null);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (Properties);
        }
    }
}