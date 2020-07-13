using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.MyModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Nop.Web.Controllers
{
    public class APIItemController : Controller
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly ICategoryService _categoryService;
        private readonly ICopyProductService _copyProductService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public APIItemController(IAclService aclService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICategoryService categoryService,
            ICopyProductService copyProductService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IExportManager exportManager,
            IImportManager importManager,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IManufacturerService manufacturerService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISettingService settingService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            VendorSettings vendorSettings)
        {
            _aclService = aclService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _categoryService = categoryService;
            _copyProductService = copyProductService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _downloadService = downloadService;
            _exportManager = exportManager;
            _importManager = importManager;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _manufacturerService = manufacturerService;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _settingService = settingService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities

        protected virtual void UpdateLocales(Product product, ProductModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.ShortDescription,
                    localized.ShortDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.FullDescription,
                    localized.FullDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(product, localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(product, seName, localized.LanguageId);
            }
        }

        protected virtual void UpdateLocales(ProductTag productTag, ProductTagModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productTag,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                var seName = _urlRecordService.ValidateSeName(productTag, string.Empty, localized.Name, false);
                _urlRecordService.SaveSlug(productTag, seName, localized.LanguageId);
            }
        }

        protected virtual void UpdateLocales(ProductAttributeMapping pam, ProductAttributeMappingModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pam,
                    x => x.TextPrompt,
                    localized.TextPrompt,
                    localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(pam,
                    x => x.DefaultValue,
                    localized.DefaultValue,
                    localized.LanguageId);
            }
        }

        protected virtual void UpdateLocales(ProductAttributeValue pav, ProductAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        protected virtual void UpdatePictureSeoNames(Product product)
        {
            foreach (var pp in _productService.GetProductPicturesByProductId(product.Id))
                _pictureService.SetSeoFilename(pp.PictureId, _pictureService.GetPictureSeName(product.Name));
        }

        protected virtual void SaveProductAcl(Product product, ProductModel model)
        {
            product.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            _productService.UpdateProduct(product);

            var existingAclRecords = _aclService.GetAclRecords(product);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(product, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        _aclService.DeleteAclRecord(aclRecordToDelete);
                }
            }
        }

        protected virtual void SaveCategoryMappings(Product product, ProductModel model)
        {
            var existingProductCategories = _categoryService.GetProductCategoriesByProductId(product.Id, true);

            //delete categories
            foreach (var existingProductCategory in existingProductCategories)
                if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                    _categoryService.DeleteProductCategory(existingProductCategory);

            //add categories
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                if (_categoryService.FindProductCategory(existingProductCategories, product.Id, categoryId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingCategoryMapping = _categoryService.GetProductCategoriesByCategoryId(categoryId, showHidden: true);
                    if (existingCategoryMapping.Any())
                        displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                    _categoryService.InsertProductCategory(new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual void SaveManufacturerMappings(Product product, ProductModel model)
        {
            var existingProductManufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id, true);

            //delete manufacturers
            foreach (var existingProductManufacturer in existingProductManufacturers)
                if (!model.SelectedManufacturerIds.Contains(existingProductManufacturer.ManufacturerId))
                    _manufacturerService.DeleteProductManufacturer(existingProductManufacturer);

            //add manufacturers
            foreach (var manufacturerId in model.SelectedManufacturerIds)
            {
                if (_manufacturerService.FindProductManufacturer(existingProductManufacturers, product.Id, manufacturerId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingManufacturerMapping = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, showHidden: true);
                    if (existingManufacturerMapping.Any())
                        displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
                    _manufacturerService.InsertProductManufacturer(new ProductManufacturer
                    {
                        ProductId = product.Id,
                        ManufacturerId = manufacturerId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual void SaveDiscountMappings(Product product, ProductModel model)
        {
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);

            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (_productService.GetDiscountAppliedToProduct(product.Id, discount.Id) is null)
                        _productService.InsertDiscountProductMapping(new DiscountProductMapping { EntityId = product.Id, DiscountId = discount.Id });
                }
                else
                {
                    //remove discount
                    if (_productService.GetDiscountAppliedToProduct(product.Id, discount.Id) is DiscountProductMapping discountProductMapping)
                        _productService.DeleteDiscountProductMapping(discountProductMapping);
                }
            }

            _productService.UpdateProduct(product);
            _productService.UpdateHasDiscountsApplied(product);
        }

        protected virtual string GetAttributesXmlForProductAttributeCombination(IFormCollection form, List<string> warnings, int productId)
        {
            var attributesXml = string.Empty;

            //get product attribute mappings (exclude non-combinable attributes)
            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(productId)
                .Where(productAttributeMapping => !productAttributeMapping.IsNonCombinable()).ToList();

            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                StringValues ctrlAttributes;

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        ctrlAttributes = form[controlId];
                        if (!string.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId].ToString();
                        if (!string.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.Split(new[] { ',' },
                                StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        ctrlAttributes = form[controlId];
                        if (!string.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, enteredText);
                        }

                        break;
                    case AttributeControlType.Datepicker:
                        var date = form[controlId + "_day"];
                        var month = form[controlId + "_month"];
                        var year = form[controlId + "_year"];
                        DateTime? selectedDate = null;
                        try
                        {
                            selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                        }
                        catch
                        {
                            //ignore any exception
                        }

                        if (selectedDate.HasValue)
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    case AttributeControlType.FileUpload:
                        var httpPostedFile = Request.Form.Files[controlId];
                        if (!string.IsNullOrEmpty(httpPostedFile?.FileName))
                        {
                            var fileSizeOk = true;
                            if (attribute.ValidationFileMaximumSize.HasValue)
                            {
                                //compare in bytes
                                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                                if (httpPostedFile.Length > maxFileSizeBytes)
                                {
                                    warnings.Add(string.Format(
                                        _localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"),
                                        attribute.ValidationFileMaximumSize.Value));
                                    fileSizeOk = false;
                                }
                            }

                            if (fileSizeOk)
                            {
                                //save an uploaded file
                                var download = new Download
                                {
                                    DownloadGuid = Guid.NewGuid(),
                                    UseDownloadUrl = false,
                                    DownloadUrl = string.Empty,
                                    DownloadBinary = _downloadService.GetDownloadBits(httpPostedFile),
                                    ContentType = httpPostedFile.ContentType,
                                    Filename = _fileProvider.GetFileNameWithoutExtension(httpPostedFile.FileName),
                                    Extension = _fileProvider.GetFileExtension(httpPostedFile.FileName),
                                    IsNew = true
                                };
                                _downloadService.InsertDownload(download);

                                //save attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in attributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            return attributesXml;
        }

        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(productTags))
                return result.ToArray();

            var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var val in values)
                if (!string.IsNullOrEmpty(val.Trim()))
                    result.Add(val.Trim());

            return result.ToArray();
        }

        protected virtual void SaveProductWarehouseInventory(Product product, ProductModel model)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (model.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return;

            if (!model.UseMultipleWarehouses)
                return;

            var warehouses = _shippingService.GetAllWarehouses();

            var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

            foreach (var warehouse in warehouses)
            {
                //parse stock quantity
                var stockQuantity = 0;
                foreach (var formKey in formData.Keys)
                {
                    if (!formKey.Equals($"warehouse_qty_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    int.TryParse(formData[formKey], out stockQuantity);
                    break;
                }

                //parse reserved quantity
                var reservedQuantity = 0;
                foreach (var formKey in formData.Keys)
                    if (formKey.Equals($"warehouse_reserved_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(formData[formKey], out reservedQuantity);
                        break;
                    }

                //parse "used" field
                var used = false;
                foreach (var formKey in formData.Keys)
                    if (formKey.Equals($"warehouse_used_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(formData[formKey], out var tmp);
                        used = tmp == warehouse.Id;
                        break;
                    }

                //quantity change history message
                var message = $"{_localizationService.GetResource("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} {_localizationService.GetResource("Admin.StockQuantityHistory.Messages.Edit")}";

                var existingPwI = _productService.GetAllProductWarehouseInventoryRecords(product.Id).FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                if (existingPwI != null)
                {
                    if (used)
                    {
                        var previousStockQuantity = existingPwI.StockQuantity;

                        //update existing record
                        existingPwI.StockQuantity = stockQuantity;
                        existingPwI.ReservedQuantity = reservedQuantity;
                        _productService.UpdateProductWarehouseInventory(existingPwI);

                        //quantity change history
                        _productService.AddStockQuantityHistoryEntry(product, existingPwI.StockQuantity - previousStockQuantity, existingPwI.StockQuantity,
                            existingPwI.WarehouseId, message);
                    }
                    else
                    {
                        //delete. no need to store record for qty 0
                        _productService.DeleteProductWarehouseInventory(existingPwI);

                        //quantity change history
                        _productService.AddStockQuantityHistoryEntry(product, -existingPwI.StockQuantity, 0, existingPwI.WarehouseId, message);
                    }
                }
                else
                {
                    if (!used)
                        continue;

                    //no need to insert a record for qty 0
                    existingPwI = new ProductWarehouseInventory
                    {
                        WarehouseId = warehouse.Id,
                        ProductId = product.Id,
                        StockQuantity = stockQuantity,
                        ReservedQuantity = reservedQuantity
                    };

                    _productService.InsertProductWarehouseInventory(existingPwI);

                    //quantity change history
                    _productService.AddStockQuantityHistoryEntry(product, existingPwI.StockQuantity, existingPwI.StockQuantity,
                        existingPwI.WarehouseId, message);
                }
            }
        }

        protected virtual void SaveConditionAttributes(ProductAttributeMapping productAttributeMapping,
            ProductAttributeConditionModel model, IFormCollection form)
        {
            string attributesXml = null;
            if (model.EnableCondition)
            {
                var attribute = _productAttributeService.GetProductAttributeMappingById(model.SelectedProductAttributeId);
                if (attribute != null)
                {
                    var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null, attribute,
                                    selectedAttributeId > 0 ? selectedAttributeId.ToString() : string.Empty);
                            }
                            else
                            {
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.Checkboxes:
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                var anyValueSelected = false;
                                foreach (var item in cblAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId <= 0)
                                        continue;

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                                    anyValueSelected = true;
                                }

                                if (!anyValueSelected)
                                {
                                    //for conditions we should save empty values even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(null,
                                        attribute, string.Empty);
                                }
                            }
                            else
                            {
                                //for conditions we should save empty values even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }

            productAttributeMapping.ConditionAttributeXml = attributesXml;
            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);
        }

        protected virtual void GenerateAttributeCombinations(Product product, IList<int> allowedAttributeIds = null)
        {
            var allAttributesXml = _productAttributeParser.GenerateAllCombinations(product, true, allowedAttributeIds);
            foreach (var attributesXml in allAttributesXml)
            {
                var existingCombination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);

                //already exists?
                if (existingCombination != null)
                    continue;

                //new one
                var warnings = new List<string>();
                warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart, product, 1, attributesXml, true, true));
                if (warnings.Count != 0)
                    continue;

                //save combination
                var combination = new ProductAttributeCombination
                {
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    StockQuantity = 0,
                    AllowOutOfStockOrders = false,
                    Sku = null,
                    ManufacturerPartNumber = null,
                    Gtin = null,
                    OverriddenPrice = null,
                    NotifyAdminForQuantityBelow = 1,
                    PictureId = 0
                };
                _productAttributeService.InsertProductAttributeCombination(combination);
            }
        }

        #endregion
        public IActionResult Index()
        {

            ConsumeApi consumeApi = new ConsumeApi();
           
           // var ProductItemJsonResult = consumeApi.GetAPIProducts();
           // ProductItemJsonResult.Wait();
           // var av = consumeApi.GetAttributeAndValuesProducts();
           // av.Wait();



           // ProductModel model = new ProductModel();
           //// List<ProductModel> model2 = new List<ProductModel>();

           // var CategoryList = _categoryService.GetAllCategories();
           // var BrandList = _manufacturerService.GetAllManufacturers();


            

           //     foreach (var item in ProductItemJsonResult.Result)
           //     {

           //         //   model2.Add(new ProductModel()
           //         //  {

           //         model.BasepriceUnitId = 1;
           //         model.CallForPrice = false;
           //         model.CanCreateCombinations = false;
           //         model.CustomerEntersPrice = false;
           //         model.DeliveryDateId = 2;
           //         model.DisableBuyButton = false;
           //         model.DisableWishlistButton = false;
           //         model.DisplayOrder = 0;
           //         model.DisplayStockAvailability = false;
           //         model.DisplayStockQuantity = false;
           //         model.DownloadActivationTypeId = 0;
           //         model.DownloadExpirationDays = null;
           //         model.DownloadId = 0;
           //         model.FullDescription = item.webDescription;
           //         model.GiftCardTypeId = 0;
           //         model.HasAvailableSpecificationAttributes = false;
           //         model.HasSampleDownload = false;
           //         model.HasUserAgreement = false;
           //         model.IsDownload = false;
           //         model.IsFreeShipping = false;
           //         model.IsGiftCard = false;
           //         model.IsLoggedInAsVendor = false;
           //         model.IsRecurring = false;
           //         model.IsRental = false;
           //         model.IsShipEnabled = true;
           //         model.IsTaxExempt = false;
           //         model.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
           //         model.ManufacturerPartNumber = null;
           //         model.MarkAsNew = item.isNew;
           //         model.MaxNumberOfDownloads = 10;
           //         model.MaximumCustomerEnteredPrice = 1000;
                   
           //         model.NotReturnable = false;
           //         model.NotifyAdminForQuantityBelow = 1;
           //         model.OldPrice = Convert.ToDecimal(item.mrp);
           //         model.OrderMaximumQuantity = 1000;
           //         model.OrderMinimumQuantity = 1;
           //         model.Price = Convert.ToDecimal(item.mrp);
           //         model.ProductCost = Convert.ToDecimal(item.rrp);
           //         model.ProductTemplateId = 1;
           //         model.ProductTypeId = 5;
           //         model.Published = true;
           //         model.RecurringCycleLength = 100;
           //         model.RecurringTotalCycles = 10;
           //         model.RentalPriceLength = 1;
           //         model.RentalPricePeriodId = 0;
           //         model.ShipSeparately = false;
           //         model.ShortDescription = "";
           //         model.ShowOnHomepage = false;
           //         model.StockQuantity = 10000;
           //         model.UnlimitedDownloads = true;
           //         model.UseMultipleWarehouses = false;
           //         model.VisibleIndividually = true;
           //         model.WarehouseId = 0;
           //         model.Weight = Convert.ToDecimal(item.weight);
           //         model.Width = Convert.ToDecimal(item.w);
           //         model.Height = Convert.ToDecimal(item.h);
           //         model.Api_itemId = item.itemId;
           //         model.SelectedCategoryIds = CategoryList.Where(c => c.ApiCategoryId == item.categoryId).Select(c => c.Id).ToArray();
           //         model.AdditionalShippingCharge = Convert.ToDecimal(item.deliveryCharge);
           //         model.AddPictureModel = null;
           //         model.AdminComment = null;
           //         model.ProductTags = item.tags;
           //         model.Length = Convert.ToDecimal(item.l);
           //         model.MetaKeywords = item.seoKeyword;
           //         model.Name = item.itemCode;

           //         model.Api_itemCode = item.itemCode;
           //         model.Api_amazonItemCode = item.amazonItemCode;
           //         model.Api_flipkartItemCode = item.flipkartItemCode;
           //         model.Api_Sku = item.vendorSKU;
           //         model.Api_paytmItemCode = item.paytmItemCode;
           //         model.Api_backEndDescription = item.backEndDescription;
           //         model.Api_inTheBox = item.inTheBox;
           //         model.Api_warrantyType = item.warrantyType;
           //         model.Api_warranty = item.warranty;
           //         model.Api_specs = item.specs;
           //         model.Api_marketPlaceQuantity = item.marketPlaceQuantity;
           //         model.Api_productHighlights = item.productHighlights;
           //         model.Api_fromManufacturer = item.fromManufacturer;
           //         model.Api_whitePaper2 = item.whitePaper2;
           //         model.Api_specialPrice = Convert.ToDecimal(item.specialPrice);
           //         model.Api_rrp = Convert.ToDecimal(item.rrp);
           //         model.Api_deliveryTime = item.deliveryTime;
           //         model.Api_isServiceTag = item.isServiceTag;
           //         model.Api_isUpdated = item.isUpdated;
           //         model.Api_Title = item.title;

           //         // model.brandId = 1;
           //         model.SelectedManufacturerIds = BrandList.Where(c => c.Api_BrandId == item.brandId).Select(c => c.Id).ToList();
           //         //  }); ;


           //         var dbItemID = _productService.GetProductByApi_itemId(item.itemId);


                    




           //         if (dbItemID == null)
           //         { 

           //        //validate maximum number of products per vendor
           //         if (_vendorSettings.MaximumProductNumber > 0 && _workContext.CurrentVendor != null
           //             && _productService.GetNumberOfProductsByVendorId(_workContext.CurrentVendor.Id) >= _vendorSettings.MaximumProductNumber)
           //         {
           //             _notificationService.ErrorNotification(string.Format(_localizationService.GetResource("Admin.Catalog.Products.ExceededMaximumNumber"),
           //                 _vendorSettings.MaximumProductNumber));
           //             return RedirectToAction("List");
           //         }

           //             if (ModelState.IsValid)
           //             {
           //                 //a vendor should have access only to his products
           //                 if (_workContext.CurrentVendor != null)
           //                     model.VendorId = _workContext.CurrentVendor.Id;

           //                 //vendors cannot edit "Show on home page" property
           //                 if (_workContext.CurrentVendor != null && model.ShowOnHomepage)
           //                     model.ShowOnHomepage = false;

           //                 //product
           //                 var product = model.ToEntity<Product>();
           //                 product.CreatedOnUtc = DateTime.UtcNow;
           //                 product.UpdatedOnUtc = DateTime.UtcNow;
           //                 _productService.InsertProduct(product);

           //                 //search engine name
           //                 model.SeName = _urlRecordService.ValidateSeName(product, model.SeName, product.Name, true);
           //                 _urlRecordService.SaveSlug(product, model.SeName, 0);

           //                 //locales
           //                 UpdateLocales(product, model);

           //                 //categories
           //                 SaveCategoryMappings(product, model);

           //                 //manufacturers
           //                 SaveManufacturerMappings(product, model);

           //                 //ACL (customer roles)
           //                 SaveProductAcl(product, model);

           //                 //stores
           //                 _productService.UpdateProductStoreMappings(product, model.SelectedStoreIds);

           //                 //discounts
           //                 SaveDiscountMappings(product, model);

           //                 //tags
           //                 _productTagService.UpdateProductTags(product, ParseProductTags(model.ProductTags));

           //                 //warehouses
           //                 SaveProductWarehouseInventory(product, model);

           //                 //quantity change history
           //                 _productService.AddStockQuantityHistoryEntry(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
           //                     _localizationService.GetResource("Admin.StockQuantityHistory.Messages.Edit"));

           //                 //activity log
           //                 _customerActivityService.InsertActivity("AddNewProduct",
           //                     string.Format(_localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name), product);

           //                 _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Added"));
           //             }

           //         }
           //         else
           //         {

           //             model.Id = dbItemID.Id;

           //             //if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
           //             //   return AccessDeniedView();

           //             //try to get a product with the specified id
           //             var product = _productService.GetProductById(model.Id);
           //             if (product == null || product.Deleted)
           //                 return RedirectToAction("List");

           //             //a vendor should have access only to his products
           //             if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
           //                 return RedirectToAction("List");

           //             //check if the product quantity has been changed while we were editing the product
           //             //and if it has been changed then we show error notification
           //             //and redirect on the editing page without data saving
           //             if (product.StockQuantity != model.LastStockQuantity)
           //             {
           //                 _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
           //                 //return RedirectToAction("Edit", new { id = product.Id });
           //             }

           //             if (ModelState.IsValid)
           //             {
           //                 //a vendor should have access only to his products
           //                 if (_workContext.CurrentVendor != null)
           //                     model.VendorId = _workContext.CurrentVendor.Id;

           //                 //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
           //                 //vendors cannot edit "Show on home page" property
           //                 if (_workContext.CurrentVendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
           //                     model.ShowOnHomepage = product.ShowOnHomepage;

           //                 //some previously used values
           //                 var prevTotalStockQuantity = _productService.GetTotalStockQuantity(product);
           //                 var prevDownloadId = product.DownloadId;
           //                 var prevSampleDownloadId = product.SampleDownloadId;
           //                 var previousStockQuantity = product.StockQuantity;
           //                 var previousWarehouseId = product.WarehouseId;
           //                 var previousProductType = product.ProductType;

           //                 //product
           //                 product = model.ToEntity(product);

           //                 product.UpdatedOnUtc = DateTime.UtcNow;
           //                 _productService.UpdateProduct(product);

           //                 //remove associated products
           //                 if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
           //                 {
           //                     var storeId = _storeContext.CurrentStore?.Id ?? 0;
           //                     var vendorId = _workContext.CurrentVendor?.Id ?? 0;

           //                     var associatedProducts = _productService.GetAssociatedProducts(product.Id, storeId, vendorId);
           //                     foreach (var associatedProduct in associatedProducts)
           //                     {
           //                         associatedProduct.ParentGroupedProductId = 0;
           //                         _productService.UpdateProduct(associatedProduct);
           //                     }
           //                 }

           //                 //search engine name
           //                 model.SeName = _urlRecordService.ValidateSeName(product, model.SeName, product.Name, true);
           //                 _urlRecordService.SaveSlug(product, model.SeName, 0);

           //                 //locales
           //                 UpdateLocales(product, model);

           //                 //tags
           //                 _productTagService.UpdateProductTags(product, ParseProductTags(model.ProductTags));

           //                 //warehouses
           //                 SaveProductWarehouseInventory(product, model);

           //                 //categories
           //                 SaveCategoryMappings(product, model);

           //                 //manufacturers
           //                 SaveManufacturerMappings(product, model);

           //                 //ACL (customer roles)
           //                 SaveProductAcl(product, model);

           //                 //stores
           //                 _productService.UpdateProductStoreMappings(product, model.SelectedStoreIds);

           //                 //discounts
           //                 SaveDiscountMappings(product, model);

           //                 //picture seo names
           //                 UpdatePictureSeoNames(product);

           //                 //back in stock notifications
           //                 if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
           //                     product.BackorderMode == BackorderMode.NoBackorders &&
           //                     product.AllowBackInStockSubscriptions &&
           //                     _productService.GetTotalStockQuantity(product) > 0 &&
           //                     prevTotalStockQuantity <= 0 &&
           //                     product.Published &&
           //                     !product.Deleted)
           //                 {
           //                     _backInStockSubscriptionService.SendNotificationsToSubscribers(product);
           //                 }

           //                 //delete an old "download" file (if deleted or updated)
           //                 if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
           //                 {
           //                     var prevDownload = _downloadService.GetDownloadById(prevDownloadId);
           //                     if (prevDownload != null)
           //                         _downloadService.DeleteDownload(prevDownload);
           //                 }

           //                 //delete an old "sample download" file (if deleted or updated)
           //                 if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
           //                 {
           //                     var prevSampleDownload = _downloadService.GetDownloadById(prevSampleDownloadId);
           //                     if (prevSampleDownload != null)
           //                         _downloadService.DeleteDownload(prevSampleDownload);
           //                 }

           //                 //quantity change history
           //                 if (previousWarehouseId != product.WarehouseId)
           //                 {
           //                     //warehouse is changed 
           //                     //compose a message
           //                     var oldWarehouseMessage = string.Empty;
           //                     if (previousWarehouseId > 0)
           //                     {
           //                         var oldWarehouse = _shippingService.GetWarehouseById(previousWarehouseId);
           //                         if (oldWarehouse != null)
           //                             oldWarehouseMessage = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
           //                     }

           //                     var newWarehouseMessage = string.Empty;
           //                     if (product.WarehouseId > 0)
           //                     {
           //                         var newWarehouse = _shippingService.GetWarehouseById(product.WarehouseId);
           //                         if (newWarehouse != null)
           //                             newWarehouseMessage = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
           //                     }

           //                     var message = string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

           //                     //record history
           //                     _productService.AddStockQuantityHistoryEntry(product, -previousStockQuantity, 0, previousWarehouseId, message);
           //                     _productService.AddStockQuantityHistoryEntry(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
           //                 }
           //                 else
           //                 {
           //                     _productService.AddStockQuantityHistoryEntry(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
           //                         product.WarehouseId, _localizationService.GetResource("Admin.StockQuantityHistory.Messages.Edit"));
           //                 }

           //                 //activity log
           //                 _customerActivityService.InsertActivity("EditProduct",
           //                     string.Format(_localizationService.GetResource("ActivityLog.EditProduct"), product.Name), product);

           //                 _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));

           //                 //if (!continueEditing)
           //                 // return RedirectToAction("List");

           //                 // return RedirectToAction("Edit", new { id = product.Id });
           //             }

           //             //prepare model
           //             //  model = _productModelFactory.PrepareProductModel(model, product, true);

           //             //if we got this far, something failed, redisplay form
           //             // return View(model);
           //         }

           //     //prepare model
           //     // model = _productModelFactory.PrepareProductModel(model, null, true);

           //     //if we got this far, something failed, redisplay form
           //     // return View(model);

                

           //     }
           // //Product Attribute Start 



           


           // foreach (var attribute in av.Result)
           // {

           //    var Item = _productService.GetProductByApi_itemId(attribute.itemId);
           //     MyProductAttributeCallBack cb = new MyProductAttributeCallBack();




           //     foreach (var attributeNameList in attribute.attributeNameList)
           //     {
           //         int Attributeid = cb.GetProductAttribute(_productAttributeService, attributeNameList.attributeName);
           //         if (Attributeid == 0)
           //         {
           //             Attributeid = cb.Create(_productAttributeService, _localizedEntityService, attributeNameList.attributeName, attributeNameList.valueList.FirstOrDefault().value);
           //         }

           //         //insert mapping
           //         ProductAttributeMappingModel attrib_model = new ProductAttributeMappingModel();
           //         attrib_model.ProductAttributeId = Attributeid;
           //         attrib_model.ProductId = Item.Id;
           //         var productAttributeMapping = attrib_model.ToEntity<ProductAttributeMapping>();

           //         _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);

           //     }

                



               



           // }





            //Product Attrubte End 



            //Product Image Start 

            var ProductImageJsonResult = consumeApi.GetProductImages();
            ProductImageJsonResult.Wait();



            int pictureId = 0;
            int productId = 0;

            List<ItemImage> ImageList = new List<ItemImage>();
            string connectionString = "Data Source=ffd.cuvzrhfpgw1r.ap-south-1.rds.amazonaws.com,1433;Initial Catalog=futureforward;uid=Admin;password=Future123;Integrated Security=False;Persist Security Info=False";
            System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(connectionString);
            foreach (var product in ProductImageJsonResult.Result)
            {

                var Item = _productService.GetProductByApi_itemId(Convert.ToInt16(product.itemId));

                SqlCommand cmdselect = new SqlCommand("select id from Picture_TempApi where imageURL='" + product.imageURL + "' and itemId='" + product.itemId + "'",con);
                con.Open();
                var iidd = cmdselect.ExecuteScalar();
                con.Close();


                if (Item != null && iidd == null)
                {
                    try
                    {
                        productId = Item.Id;
                        ///////////////

                        string imgUrl = product.imageURL;

                        // var httpPostedFile = Request.Form.Files.FirstOrDefault();
                        WebClient wc = new WebClient();
                        byte[] bytes = wc.DownloadData(imgUrl);
                        MemoryStream ms = new MemoryStream(bytes);
                        //System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        var qqFileName = imgUrl.Split('/')[imgUrl.Split('/').Length - 1];


                        var piccttuurreess = _pictureService.GetPicturesByProductId(productId);







                        IFormFile file = new FormFile(ms, 0, bytes.Length, "name", qqFileName);
                        //IFormFile formFile =(IFormFile)img;


                        //const string qqFileNameParameter = "qqfilename";



                        var pictureRaw = _pictureService.InsertPicture(file, qqFileName);

                        //when returning JSON the mime-type must be set to text/plain
                        //otherwise some browsers will pop-up a "Save As" dialog.
                        // var resultt = Json(new
                        //  {
                        bool success = true;
                        pictureId = pictureRaw.Id;
                        string imageUrl = _pictureService.GetPictureUrl(ref pictureRaw, 100);
                        // });



                        ///////////////









                        //try to get a product with the specified id
                        var productforImage = _productService.GetProductById(productId)
                            ?? throw new ArgumentException("No product found with the specified id");

                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && productforImage.VendorId != _workContext.CurrentVendor.Id)
                            return RedirectToAction("List");

                        if (_productService.GetProductPicturesByProductId(productId).Any(p => p.PictureId == pictureId))
                            return Json(new { Result = false });

                        //try to get a picture with the specified id
                        var picture = _pictureService.GetPictureById(pictureId)
                            ?? throw new ArgumentException("No picture found with the specified id");

                        _pictureService.UpdatePicture(picture.Id,
                            _pictureService.LoadPictureBinary(picture),
                            picture.MimeType,
                            picture.SeoFilename,
                            null,
                            null);

                        _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(productforImage.Name));

                        _productService.InsertProductPicture(new ProductPicture
                        {
                            PictureId = pictureId,
                            ProductId = productId,
                            DisplayOrder = 0
                        });






                       
                        SqlCommand cmd = new SqlCommand("insert into Picture_TempApi(itemId,imageURL)values('" + product.itemId + "','" + product.imageURL + "')",con);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();




                    }
                    catch (Exception ex)
                    {

                       
                    }
                }
            }

            //Product Image End




            return RedirectToAction("Index", "Home");
        }

        //public async Task<List<ProductItem>> Index2s)
        //{
        //    List<ProductItem> reservationList = new List<ProductItem>();
        //    using (var httpClient = new HttpClient())
        //    {
        //        using (var response = await httpClient.GetAsync("https://www.stockapi.in/api/nopNewOrUpdatedItemMaster"))
        //        {
        //            string apiResponse = await response.Content.ReadAsStringAsync();
        //            reservationList = JsonConvert.DeserializeObject<List<ProductItem>>(apiResponse);
        //        }
        //    }
        //    return (reservationList);
        //}

     


    }
}