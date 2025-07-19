using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utilities.Contants;
using WEB.CMS.Models;
using WEB.CMS.RabitMQ;

namespace WEB.CMS.Controllers.Elastic.Bussiness
{
    public  class ElasticService
    {
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient work_queue;
        public ElasticService(IConfiguration configuration)
        {
            work_queue = new WorkQueueClient(configuration);
            _configuration = configuration;
        }
        public  bool PushToQueue(string store_procedure,long id=-1)
        {
            try
            {
                var j_param = new Dictionary<string, object>
                {
                    { "store_name", store_procedure },
                    { "index_es", "hulotoys_" + store_procedure.ToLower().Trim() },
                    { "project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                    { "id", id }
                };

                var _data_push = JsonConvert.SerializeObject(j_param);
                var response_queue = work_queue.InsertQueueSimpleSyncES(_data_push);

                if (response_queue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
