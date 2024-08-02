using System.Text.Json;

namespace Next_Solution.WebApi.Options
{
    public static class JsonSerializerOptionsDefaults
    {
        public static JsonSerializerOptions General
        {
            get
            {
                var generalJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);
                return generalJsonSerializerOptions;
            }
        }

        public static JsonSerializerOptions Web
        {
            get
            {
                var webJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                return webJsonSerializerOptions;
            }
        }
    }
}