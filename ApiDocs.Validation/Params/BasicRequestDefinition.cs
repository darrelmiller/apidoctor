﻿

namespace ApiDocs.Validation.Params
{
    using System.Collections.Generic;
    using ApiDocs.Validation.Error;
    using Newtonsoft.Json;

    public class BasicRequestDefinition
    {
        /// <summary>
        /// Raw Http Request that is invoked instead of using a method from the documentation
        /// </summary>
        [JsonProperty("http-request", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string RawHttpRequest { get; set; }

        /// <summary>
        /// Name of the method that should be invoked for this call (instead of an http request)
        /// </summary>
        [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string MethodName { get; set; }

        /// <summary>
        /// Specify a list of replacements to be made in the request using values previously captured from
        /// test-setup requests.
        /// </summary>
        [JsonProperty("request-parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, string> RequestParameters { get; set; }

        /// <summary>
        /// A set of status code results that are included as "success" for this operation.
        /// </summary>
        [JsonProperty("allowed-status-codes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<int> AllowedStatusCodes { get; set; }

        /// <summary>
        /// A set of expectations that are validated when the response is complete. The key value
        /// follows the same formatting as RequestParameter values, and the values are either 
        /// a constant or an array of constant values that are expected.
        /// </summary>
        [JsonProperty("expectations", DefaultValueHandling=DefaultValueHandling.Ignore)]
        public Dictionary<string, object> Expectations { get; set; }


        public virtual ValidationError[] CheckForErrors()
        {
            List<ValidationError> errors = new List<ValidationError>();

            if (!string.IsNullOrEmpty(this.RawHttpRequest) && !string.IsNullOrEmpty(this.MethodName))
                errors.Add(new ValidationError(ValidationErrorCode.HttpRequestAndMethodSpecified, null, "http-request and method are mutually exclusive: http-request: {0}, method: {1}", this.RawHttpRequest, this.MethodName));

            foreach (var key in this.RequestParameters.Keys)
            {
                var keyType = LocationForKey(key);
                switch (keyType)
                {
                    case PlaceholderLocation.Invalid:
                    case PlaceholderLocation.StoredValue:
                        errors.Add(new ValidationError(ValidationErrorCode.HttpRequestParameterInvalid, null, "request-parameters key {0} is invalid. KeyType was {1}", key, keyType));
                        break;
                }
            }

            return errors.ToArray();
        }

        /// <summary>
        /// Parse the type of placeholder value the input represents.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static PlaceholderLocation LocationForKey(string key)
        {
            string index;
            return LocationForKey(key, out index);
        }

        public static PlaceholderLocation LocationForKey(string key, out string index)
        {
            index = key;
            if (null == key)
                return PlaceholderLocation.Invalid;

            if (key.StartsWith("{") && key.EndsWith("}") && key.Length > 2)
                return PlaceholderLocation.Url;
            if (key.StartsWith("[") && key.EndsWith("]") && key.Length > 2)
                return PlaceholderLocation.StoredValue;
            if (key == "!body")
                return PlaceholderLocation.Body;
            if (key == "!url")
                return PlaceholderLocation.Url;
            if (key.EndsWith(":") && key.Length > 1)
            {
                index = key.Substring(0, key.Length - 1);
                return PlaceholderLocation.HttpHeader;
            }
            if (key.StartsWith("$"))
                return PlaceholderLocation.Json;

            return PlaceholderLocation.Invalid;
        }
    }
}
