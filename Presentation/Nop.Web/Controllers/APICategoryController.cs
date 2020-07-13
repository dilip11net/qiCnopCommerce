using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.MyModel;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Discounts;
using Nop.Services.Caching;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using System.Linq;

namespace Nop.Web.Controllers
{
    public class APICategoryController : Controller
    {
        private readonly IAclService _aclService;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly IManufacturerService _manufacturerService;
        public APICategoryController(IAclService aclService,
            ICacheKeyService cacheKeyService,
            ICategoryModelFactory categoryModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IExportManager exportManager,
            IImportManager importManager,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext, IManufacturerService manufacturerService)
        {
            _aclService = aclService;
            _cacheKeyService = cacheKeyService;
            _categoryModelFactory = categoryModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _exportManager = exportManager;
            _importManager = importManager;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _manufacturerService = manufacturerService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult InsertCategory()
        {

            ConsumeApi consumeApi = new ConsumeApi();

            var CategoriesAndAlls = consumeApi.GetCategoriesAndAll();
            CategoriesAndAlls.Wait();







           






            CategoryModel model = new CategoryModel();
            //using (StreamReader r = new StreamReader("MyModel/CategoriesAll.json"))
            if(true)
            {
               // string json = r.ReadToEnd();
               // CategoriesAll items = JsonConvert.DeserializeObject<CategoriesAll>(json);
                model = new CategoryModel();
                foreach (var item in CategoriesAndAlls.Result.parentCategoriesList)
                {
                   
                        model.AllowCustomersToSelectPageSize = true;
                        model.Breadcrumb = null;
                        model.CategoryTemplateId = 1;
                        model.Deleted = false;
                        model.Description = item.parentCategoryName;
                        model.DisplayOrder = 0;
                        model.Name = item.parentCategoryName;
                        model.PageSize = 6;
                        model.PageSizeOptions = "6;3;9";
                        model.ParentCategoryId = 0;
                        model.Published = true;
                        model.SeName = "--";
                        model.ShowOnHomepage = false;
                        model.ApiCategoryId = item.parentCategoryId;
                        model.ApiParentCategoryId = null;
                        model.ApiSubCategoryId =  null;
                        model.IncludeInTopMenu = true;



                    var IfParentExist = _categoryService.GetAllCategories(item.parentCategoryName);

                    // NOPComm Create Data 
                    if (IfParentExist.Count == 0)
                    {
                        var category = model.ToEntity<Category>();
                        category.CreatedOnUtc = DateTime.UtcNow;
                        category.UpdatedOnUtc = DateTime.UtcNow;
                        _categoryService.InsertCategory(category);

                        //search engine name
                        model.SeName = _urlRecordService.ValidateSeName(category, model.SeName, category.Name, true);
                        _urlRecordService.SaveSlug(category, model.SeName, 0);

                        //locales
                        UpdateLocales(category, model);

                        //discounts
                        var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
                        foreach (var discount in allDiscounts)
                        {
                            if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                                _categoryService.InsertDiscountCategoryMapping(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                        }

                        _categoryService.UpdateCategory(category);

                        //update picture seo file name
                        UpdatePictureSeoNames(category);

                        //ACL (customer roles)
                        SaveCategoryAcl(category, model);

                        //stores
                        SaveStoreMappings(category, model);

                        //activity log
                        _customerActivityService.InsertActivity("AddNewCategory",
                            string.Format(_localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name), category);

                        _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Added"));

                        //if (!continueEditing)
                         //   return RedirectToAction("List");

                       // return RedirectToAction("Edit", new { id = category.Id });
                    }




                }
                var AllCategoryList = _categoryService.GetAllCategories();

                model = new CategoryModel();
                foreach (var item in CategoriesAndAlls.Result.categoriesList)
                {
                    var ParentCategory = AllCategoryList.Where(c => c.ApiCategoryId == item.parentCategoryId).FirstOrDefault();


                    model.AllowCustomersToSelectPageSize = true;
                    model.Breadcrumb = null;
                    model.CategoryTemplateId = 1;
                    model.Deleted = false;
                    model.Description = item.categoryName;
                    model.DisplayOrder = 0;
                    model.Name = item.categoryName;
                    model.PageSize = 6;
                    model.PageSizeOptions = "6;3;9";
                    model.ParentCategoryId = ParentCategory.Id;
                    model.Published = true;
                    model.SeName = "--";
                    model.ShowOnHomepage = false;
                    model.ApiCategoryId = item.categoryId;
                    model.ApiParentCategoryId = item.parentCategoryId;
                    model.ApiSubCategoryId = null;
                    model.IncludeInTopMenu = true;



                    var IfCategoryListExist = AllCategoryList.Where(c => c.ApiCategoryId == item.categoryId).ToList();


                    // NOPComm Create Data 
                    if (IfCategoryListExist.Count == 0)
                    {
                        var category = model.ToEntity<Category>();
                        category.CreatedOnUtc = DateTime.UtcNow;
                        category.UpdatedOnUtc = DateTime.UtcNow;
                        _categoryService.InsertCategory(category);

                        //search engine name
                        model.SeName = _urlRecordService.ValidateSeName(category, model.SeName, category.Name, true);
                        _urlRecordService.SaveSlug(category, model.SeName, 0);

                        //locales
                        UpdateLocales(category, model);

                        //discounts
                        var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
                        foreach (var discount in allDiscounts)
                        {
                            if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                                _categoryService.InsertDiscountCategoryMapping(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                        }

                        _categoryService.UpdateCategory(category);

                        //update picture seo file name
                        UpdatePictureSeoNames(category);

                        //ACL (customer roles)
                        SaveCategoryAcl(category, model);

                        //stores
                        SaveStoreMappings(category, model);

                        //activity log
                        _customerActivityService.InsertActivity("AddNewCategory",
                            string.Format(_localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name), category);

                        _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Added"));

                        //if (!continueEditing)
                        //   return RedirectToAction("List");

                        // return RedirectToAction("Edit", new { id = category.Id });
                    }




                }
                
                AllCategoryList  = _categoryService.GetAllCategories();
                model = new CategoryModel();
                foreach (var item in CategoriesAndAlls.Result.subCategoriesList)
                {
                   
                    var ParentCategory = AllCategoryList.Where(c => c.ApiCategoryId == item.categoryId).FirstOrDefault();


                    model.AllowCustomersToSelectPageSize = true;
                    model.Breadcrumb = null;
                    model.CategoryTemplateId = 1;
                    model.Deleted = false;
                    model.Description = item.subCategoryName;
                    model.DisplayOrder = 0;
                    model.Name = item.subCategoryName;
                    model.PageSize = 6;
                    model.PageSizeOptions = "6;3;9";
                    model.ParentCategoryId = ParentCategory.Id;
                    model.Published = true;
                    model.SeName = "--";
                    model.ShowOnHomepage = false;
                    model.ApiCategoryId = item.categoryId;
                    model.ApiParentCategoryId = null;
                    model.ApiSubCategoryId = item.subCategoryId;
                    model.IncludeInTopMenu = true;


                    var IfSubCategoryListExist = AllCategoryList.Where(c => c.ApiSubCategoryId == item.subCategoryId).ToList();


                    // NOPComm Create Data 
                    if (IfSubCategoryListExist.Count == 0)
                    {
                        var category = model.ToEntity<Category>();
                        category.CreatedOnUtc = DateTime.UtcNow;
                        category.UpdatedOnUtc = DateTime.UtcNow;
                        _categoryService.InsertCategory(category);

                        //search engine name
                        model.SeName = _urlRecordService.ValidateSeName(category, model.SeName, category.Name, true);
                        _urlRecordService.SaveSlug(category, model.SeName, 0);

                        //locales
                        UpdateLocales(category, model);

                        //discounts
                        var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToCategories, showHidden: true);
                        foreach (var discount in allDiscounts)
                        {
                            if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                                _categoryService.InsertDiscountCategoryMapping(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                        }

                        _categoryService.UpdateCategory(category);

                        //update picture seo file name
                        UpdatePictureSeoNames(category);

                        //ACL (customer roles)
                        SaveCategoryAcl(category, model);

                        //stores
                        SaveStoreMappings(category, model);

                        //activity log
                        _customerActivityService.InsertActivity("AddNewCategory",
                            string.Format(_localizationService.GetResource("ActivityLog.AddNewCategory"), category.Name), category);

                        _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Categories.Added"));

                        //if (!continueEditing)
                        //   return RedirectToAction("List");

                        // return RedirectToAction("Edit", new { id = category.Id });
                    }




                }





                













            }










            return RedirectToAction("Index", "Home");

        }
        protected virtual void UpdateLocales(Category category, CategoryModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(category,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(category, localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(category, seName, localized.LanguageId);
            }
        }
        protected virtual void UpdatePictureSeoNames(Category category)
        {
            var picture = _pictureService.GetPictureById(category.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(category.Name));
        }

        protected virtual void SaveCategoryAcl(Category category, CategoryModel model)
        {
            category.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            _categoryService.UpdateCategory(category);

            var existingAclRecords = _aclService.GetAclRecords(category);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(category, customerRole.Id);
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

        protected virtual void SaveStoreMappings(Category category, CategoryModel model)
        {
            category.LimitedToStores = model.SelectedStoreIds.Any();
            _categoryService.UpdateCategory(category);

            var existingStoreMappings = _storeMappingService.GetStoreMappings(category);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(category, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }
    }

    
}