using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Supports;
using HM.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Services
{
    public class SupportService(
        HmDbContext context,
        ILogger<SupportService> logger
        ) : ISupportService
    {
        public async Task<OperationResult> SaveSupportRequestAsync(SupportDto supportDto, CancellationToken cancellationToken)
        {
            try
            {
                var support = supportDto.ToSupport();  

                await context.Supports.AddAsync(support, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return new OperationResult(true, "The request to the support service has been successfully saved.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while saving the support request");
                return new OperationResult(false, "Failed to save support request.");
            }
        }
    }
}
