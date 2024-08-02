using Newtonsoft.Json;

namespace RSSFeedifyClientCore
{
    public static class JsonConvertor
    {
        public static string ConvertObjectToJsonString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
