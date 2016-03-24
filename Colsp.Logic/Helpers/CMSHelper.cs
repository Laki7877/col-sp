using Colsp.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public class CMSHelper
    {
        public static int? GetCMSCategoryId(ColspEntities db, CMSCategory cmsCategory)
        {
            var category = db.CMSCategories.Where(x => 
                                                  x.CMSCategoryNameEN.Equals(cmsCategory.CMSCategoryNameEN) &&
                                                  x.CMSCategoryNameTH.Equals(cmsCategory.CMSCategoryNameTH))
                                                  .FirstOrDefault();

            if (category == null)
                return null;

            return category.CMSCategoryId;
        }
    }
}
