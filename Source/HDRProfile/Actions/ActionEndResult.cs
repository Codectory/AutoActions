namespace AutoHDR.Actions
{
    public class ActionEndResult
    {
        public bool Success { get; private set; }
        public string ErrorInfo { get; set; }

        public object ErrorObject { get; private set; }
        public object ResultObject { get; private set; }

        public ActionEndResult(bool success)
        {
            Success = success;
        }
        public ActionEndResult(bool success, object resultObject)
        {
            Success = success;
            ResultObject = resultObject;
        }

        public ActionEndResult(bool success, string errorInfo, object errorObject)
        {
            Success = success;
            ErrorInfo = errorInfo;
            ErrorObject = errorObject;
        }
    }
}