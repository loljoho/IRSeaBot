using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public interface IBotService
    {
        Task<string> Get(string cmd, string replyTo);
    }
}
