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
        Task<OperationResult<NewPostInternetDocumentDto>> CreateInternetDocument(int orderid, string SenderWarehouseIndex, string senderRef,
            string PayerType, string PaymentMethod, DateTimeOffset DateOfSend, float weight, string serviceType, string SeatsAmount,
            string description, float costOfEstimate, float costOfGood, CancellationToken cancellationToken);
        Task<OperationResult> DeleteInternetDocument(string InternetDocumentRef, CancellationToken cancellation);
        Task<IEnumerable<NewPostInternetDocumentDto>> GetAllInternetDocumentsAsync(CancellationToken cancellationToken);
        Task<NewPostInternetDocumentDto?> GetInternetDocumentByRefAsync(string documentRef, CancellationToken cancellationToken);
    }
}
