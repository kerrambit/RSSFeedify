using Newtonsoft.Json;

namespace ClientNetLib.Services
{
    public static class JsonConvertor
    {
        public static string ConvertObjectToJsonString(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
