using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDetectionManager.Abstractions
{
    public interface IQueueRepository
    {
        Task<bool> QueueMessage(string message, QueueType queueType);
        Task<CloudQueueMessage> GetMessage(QueueType queueType);
        Task<bool> DeleteMessage(CloudQueueMessage message, QueueType queueType);
        Task<int> GetQueueLength(QueueType queueType);
    }
}
