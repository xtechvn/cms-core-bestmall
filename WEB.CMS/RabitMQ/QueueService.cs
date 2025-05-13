using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WEB.CMS.RabitMQ
{
    public class QueueService
    {
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient work_queue;

        // Constructor để inject dependency (nếu có)
        public QueueService(IConfiguration configuration)
        {
            work_queue = new WorkQueueClient(configuration);
        }

        // Hàm dùng để push message vào queue
        public JsonResult PushMessageToQueue(string storeName, string indexEs, int projectType, long id, string queueName)
        {
            // Tạo message để push vào queue
            var j_param = new Dictionary<string, object>
        {
            { "store_name", storeName },
            { "index_es", indexEs },
            { "project_type", projectType },
            { "id", id }
        };

            var _data_push = JsonConvert.SerializeObject(j_param);

            // Push message vào queue
            var response_queue = work_queue.InsertQueueSimple(_data_push, queueName);

            return new JsonResult(new
            {
                isSuccess = true,
                message = "Đã push message thành công",
                dataId = id
            });
        }

      
    }


}
