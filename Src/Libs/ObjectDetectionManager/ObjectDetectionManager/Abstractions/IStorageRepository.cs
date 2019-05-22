using System.IO;
using System.Threading.Tasks;

namespace ObjectDetectionManager.Abstractions
{
    public interface IStorageRepository
    {
        Task<string> CreateFile(string name, Stream stream);
    }
}