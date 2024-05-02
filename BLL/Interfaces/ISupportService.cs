using HM.BLL.Models.Common;
using HM.BLL.Models.Supports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Interfaces
{
    public interface ISupportService
    {
        Task<OperationResult> SaveSupportRequestAsync(SupportDto supportRequest, CancellationToken cancellationToken);

    }
}
