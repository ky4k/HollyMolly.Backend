using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;

namespace HM.BLL.Interfaces.NewPost
{
    public interface INewPostCounerAgentService
    {
        Task<OperationResult<IEnumerable<NewPostCounterAgentDto>>>CreateCounterpartyAsync(CustomerDto customerDto,
            CancellationToken cancellationToken);
        Task<NewPostCounterAgentDto?> GetCounterpartyAsync(CustomerDto customerDto, CancellationToken cancellationToken);
        Task<IEnumerable<NewPostCounterAgentAdress>>GetCounterpartyAdressAsync(string counterPartyRef, 
            CancellationToken cancellationToken);
        Task<IEnumerable<NewPostContactPersonDto>> GetContactPersonsAsync(string counterPartyRef, string Page,
            CancellationToken cancellationToken);
        Task<IEnumerable<NewPostCounterAgents>> GetSendersListAsync(string Page, CancellationToken cancellationToken);
    }
}
