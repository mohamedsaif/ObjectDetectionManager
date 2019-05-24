using System.IO;
using System.Threading.Tasks;

namespace ObjectDetectionManager.Abstractions
{
    public interface IStorageRepository
    {
        Task<string> CreateFileAsync(string name, Stream stream);
    }
}