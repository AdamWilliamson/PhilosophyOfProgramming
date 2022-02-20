using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Validations.Utilities;

public class ErrorMessagePrettifier
{
    public string ToJson(ValidationMessage messages)
    {
        return JsonConvert.SerializeObject(messages, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                }
            }
        });
    }

    public string ToJson(ValidationResult results)
    {
        return JsonConvert.SerializeObject(results, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                }
            }
        });
    }
}