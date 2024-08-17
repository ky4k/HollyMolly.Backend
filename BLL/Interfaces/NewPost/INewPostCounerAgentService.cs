using HM.BLL.Models.NewPost;
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
        Task<NewPostResponse<NewPostCounterAgentDto>> CreateCounterpartyAsync(NewPostCounterAgentDto counterAgent);
    }
}
