using Newtonsoft.Json;

namespace RSSFeedifyCLIClient.Services
{
    public static class JsonConvertor
    {
        public static string ConvertObjectToJsonString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
