using System.Collections.Generic;
using Newtonsoft.Json;

namespace TarimHibe.Models.ApiModels
{
    public class ApiResponse<T>
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("isError")]
        public bool IsError { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        [JsonProperty("pageCount")]
        public int PageCount { get; set; }

        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }
    }
}