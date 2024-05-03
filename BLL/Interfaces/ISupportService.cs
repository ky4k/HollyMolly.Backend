using HM.BLL.Models.Common;
using HM.BLL.Models.Supports;

namespace HM.BLL.Interfaces
{
    public interface ISupportService
    {
        Task<OperationResult> SaveSupportRequestAsync(SupportDto supportDto, CancellationToken cancellationToken);
    }
}
