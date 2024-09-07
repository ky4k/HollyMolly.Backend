using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Interfaces.NewPost
{
    public interface INewPostInternetDocument
    {
        Task<OperationResult<NewPostInternetDocumentDto>> CreateInternetDocument (int orderid, string typeOfSender, DateTimeOffset date,
            float weight, string serviceType, string description,float cost, CancellationToken cancellationToken);
    }
}
