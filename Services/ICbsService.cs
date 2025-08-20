using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TarimHibe.Models.ApiModels;

namespace TarimHibe.Services
{
    public interface ICbsService
    {
        Task<List<IlceModel>> GetIlcelerAsync();
        Task<List<MahalleModel>> GetMahallelerAsync(string ilceId);
        //  Task<JToken> GetParselAsync(string mahalleId, string ada, string parsel);
        Task<JObject> GetParselAsync(string mahalleId, string ada, string parsel);
    }
}