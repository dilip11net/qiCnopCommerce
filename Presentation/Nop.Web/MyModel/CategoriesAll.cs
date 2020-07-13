using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.MyModel
{
    public class ParentCategoriesList
    {
        public int parentCategoryId { get; set; }
        public string parentCategoryName { get; set; }
    }

    public class CategoriesList
    {
        public int parentCategoryId { get; set; }
        public int categoryId { get; set; }
        public string categoryName { get; set; }
    }

    public class SubCategoriesList
    {
        public int categoryId { get; set; }
        public int subCategoryId { get; set; }
        public string subCategoryName { get; set; }
    }

    public class AttributeGroupList
    {
        public int? categoryId { get; set; }
        public int attributeGroupId { get; set; }
        public string attributeGroupName { get; set; }
    }

    public class BrandList
    {
        public int brandId { get; set; }
        public string brandName { get; set; }
    }

    public class CategoriesAll
    {
        public IList<ParentCategoriesList> parentCategoriesList { get; set; }
        public IList<CategoriesList> categoriesList { get; set; }
        public IList<SubCategoriesList> subCategoriesList { get; set; }
        public IList<AttributeGroupList> attributeGroupList { get; set; }
        public IList<BrandList> brandList { get; set; }
    }



}
