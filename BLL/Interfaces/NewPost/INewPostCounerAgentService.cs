using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using HM.DAL.Entities.NewPost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Interfaces.NewPost
{
    public interface INewPostCounerAgentService
    {
        Task<OperationResult<IEnumerable<NewPostCounterAgentDto>>>CreateCounterpartyAsync(CustomerDto customerDto, CancellationToken cancellationToken);
        Task<OperationResult<NewPostCounterAgentDto>> GetCounterpartyAsync(CustomerDto customerDto, CancellationToken cancellationToken);
        Task<OperationResult<IEnumerable<NewPostCounterAgentAdress>>>GetCounterpartyAdressAsync(string counterPartyRef, CancellationToken cancellationToken);
    }
}
