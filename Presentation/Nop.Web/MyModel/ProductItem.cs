using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.MyModel
{
    public class ProductItem
    {
        public int itemId { get; set; }
        public int categoryId { get; set; }
        public int brandId { get; set; }
        public int? subCategoryId { get; set; }
        public string subSubCategoryId { get; set; }
        public int parentCategoryId { get; set; }
        public int? attributeGroupId { get; set; }
        public string categoryName { get; set; }
        public string brandName { get; set; }
        public string subCategoryName { get; set; }
        public string subSubCategoryName { get; set; }
        public string parentCategoryName { get; set; }
        public string attributeGroupName { get; set; }
        public string itemCode { get; set; }
        public string title { get; set; }
        public string amazonItemCode { get; set; }
        public string flipkartItemCode { get; set; }
        public string vendorSKU { get; set; }
        public string paytmItemCode { get; set; }
        public string backEndDescription { get; set; }
        public string inTheBox { get; set; }
        public string warrantyType { get; set; }
        public string warranty { get; set; }
        public string tags { get; set; }
        public string seoKeyword { get; set; }
        public string specs { get; set; }
        public int? marketPlaceQuantity { get; set; }
        public string webDescription { get; set; }
        public string productHighlights { get; set; }
        public string fromManufacturer { get; set; }
        public string whitePaper2 { get; set; }
        public double? l { get; set; }
        public double? w { get; set; }
        public double? h { get; set; }
        public double? weight { get; set; }
        public double specialPrice { get; set; }
        public double mrp { get; set; }
        public double rrp { get; set; }
        public string deliveryCharge { get; set; }
        public string deliveryTime { get; set; }
        public bool isActive { get; set; }
        public bool isServiceTag { get; set; }
        public bool isNew { get; set; }
        public bool isUpdated { get; set; }
        public IList<object> relatedItemList { get; set; }
        public IList<object> similarItemList { get; set; }
    }

    public class ItemImage
    {
        public string itemId {get;set;}
    public string imageURL {get;set;}
    
    }
}
