namespace NearClient.Providers
{
    public class ExecutionError
    {
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }

        public static ExecutionError FromDynamicJsonObject(dynamic jsonObject)
        {
            var result = new ExecutionError()
            {
                ErrorMessage = jsonObject.ToString()
               // ErrorType = jsonObject.error_type
            };
            return result;
        }
    }
}