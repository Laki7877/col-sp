using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic.ConditionRequest
{
    public class ReportCondition
    {

    }

    public enum ReportEnum
    {
        [StringValue("Sale Report for Seller")]
        SALE_FOR_SELLER,

        [StringValue("Stock Status")]
        STOCK_STATUS,

        [StringValue("Item on Hold")]
        ITEM_ON_HOLD,

        [StringValue("Commission")]
        COMMISSION
    }
}
