
namespace StackoverflowChatbot.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Uploads a file and returns a link
        /// </summary>
        /// <param name="file">byte array of the file</param>
        /// <returns>url to access the file</returns>
        string UploadFile(byte[] file);
    }
}