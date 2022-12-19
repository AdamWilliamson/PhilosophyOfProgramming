using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Validations_Tests.Version_5;

public class JsonConverter
{
    public string ToJson(DescriptionResult description)
    {
        return JsonConvert.SerializeObject(description, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                }, 
            }
        });
    }

    public string ToJson(ValidationResult validation)
    {
        return JsonConvert.SerializeObject(validation, new JsonSerializerSettings
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
