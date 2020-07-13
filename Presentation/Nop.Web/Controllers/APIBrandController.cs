using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
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
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System.IO;
using Nop.Web.MyModel;
using Newtonsoft.Json;

namespace Nop.Web.Controllers
{
    public class APIBrandController : Controller
    {
        private readonly IAclService _aclService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IManufacturerModelFactory _manufacturerModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;

        public APIBrandController(IAclService aclService,
      ICustomerActivityService customerActivityService,
      ICustomerService customerService,
      IDiscountService discountService,
      IExportManager exportManager,
      IImportManager importManager,
      ILocalizationService localizationService,
      ILocalizedEntityService localizedEntityService,
      IManufacturerModelFactory manufacturerModelFactory,
      IManufacturerService manufacturerService,
      INotificationService notificationService,
      IPermissionService permissionService,
      IPictureService pictureService,
      IProductService productService,
      IStoreMappingService storeMappingService,
      IStoreService storeService,
      IUrlRecordService urlRecordService,
      IWorkContext workContext)
        {
            _aclService = aclService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _exportManager = exportManager;
            _importManager = importManager;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _manufacturerModelFactory = manufacturerModelFactory;
            _manufacturerService = manufacturerService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
        }



      

        protected virtual void UpdateLocales(Manufacturer manufacturer, ManufacturerModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(manufacturer,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = _urlRecordService.ValidateSeName(manufacturer, localized.SeName, localized.Name, false);
                _urlRecordService.SaveSlug(manufacturer, seName, localized.LanguageId);
            }
        }

        protected virtual void UpdatePictureSeoNames(Manufacturer manufacturer)
        {
            var picture = _pictureService.GetPictureById(manufacturer.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(manufacturer.Name));
        }

        protected virtual void SaveManufacturerAcl(Manufacturer manufacturer, ManufacturerModel model)
        {
            manufacturer.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            _manufacturerService.UpdateManufacturer(manufacturer);

            var existingAclRecords = _aclService.GetAclRecords(manufacturer);
            var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                        _aclService.InsertAclRecord(manufacturer, customerRole.Id);
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

        protected virtual void SaveStoreMappings(Manufacturer manufacturer, ManufacturerModel model)
        {
            manufacturer.LimitedToStores = model.SelectedStoreIds.Any();
            _manufacturerService.UpdateManufacturer(manufacturer);

            var existingStoreMappings = _storeMappingService.GetStoreMappings(manufacturer);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(manufacturer, store.Id);
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

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult InsertBrand()
        {
            ConsumeApi consumeApi = new ConsumeApi();

            var CategoriesAndAlls = consumeApi.GetCategoriesAndAll();
            CategoriesAndAlls.Wait();





            //using (StreamReader r = new StreamReader("MyModel/CategoriesAll.json"))
           if(true)
            {
               // string json = r.ReadToEnd();

               // CategoriesAll items = JsonConvert.DeserializeObject<CategoriesAll>(json);
                var AllBrandList = _manufacturerService.GetAllManufacturers();
                ManufacturerModel model = new ManufacturerModel();
                foreach (var item in CategoriesAndAlls.Result.brandList)
                {


                    model.Name = item.brandName;
                    model.Api_BrandId = item.brandId;


                    model.MetaKeywords = null;
                    model.MetaTitle = null;
                    model.PriceRanges = null;
                    model.PageSizeOptions = "6;3;9";


                    model.Description = null;
                    model.ManufacturerTemplateId = 1;
                    model.MetaDescription = null;
                    model.PictureId = 0;

                    model.PageSize = 6;

                    model.AllowCustomersToSelectPageSize = true;
                    model.Published = true;
                    model.Deleted = false;
                    model.DisplayOrder = 0;



                    var IfManufacListExist = AllBrandList.Where(c => c.Name == item.brandName).ToList();


                    // NOPComm Create Data 
                    if (IfManufacListExist.Count == 0)
                    {

                        if (ModelState.IsValid)
                        {
                            var manufacturer = model.ToEntity<Manufacturer>();
                            manufacturer.CreatedOnUtc = DateTime.UtcNow;
                            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                            _manufacturerService.InsertManufacturer(manufacturer);

                            //search engine name
                            model.SeName = _urlRecordService.ValidateSeName(manufacturer, model.SeName, manufacturer.Name, true);
                            _urlRecordService.SaveSlug(manufacturer, model.SeName, 0);

                            //locales
                            UpdateLocales(manufacturer, model);

                            //discounts
                            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
                            foreach (var discount in allDiscounts)
                            {
                                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                                    //manufacturer.AppliedDiscounts.Add(discount);
                                    _manufacturerService.InsertDiscountManufacturerMapping(new DiscountManufacturerMapping { EntityId = manufacturer.Id, DiscountId = discount.Id });

                            }

                            _manufacturerService.UpdateManufacturer(manufacturer);

                            //update picture seo file name
                            UpdatePictureSeoNames(manufacturer);

                            //ACL (customer roles)
                            SaveManufacturerAcl(manufacturer, model);

                            //stores
                            SaveStoreMappings(manufacturer, model);

                            //activity log
                            _customerActivityService.InsertActivity("AddNewManufacturer",
                                string.Format(_localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name), manufacturer);

                            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Manufacturers.Added"));

                            // if (!continueEditing)
                            //  return RedirectToAction("List");

                            //  return RedirectToAction("Edit", new { id = manufacturer.Id });
                        }

                        //prepare model
                        // model = _manufacturerModelFactory.PrepareManufacturerModel(model, null, true);

                        //if we got this far, something failed, redisplay form
                        // return View(model);
                    }




                }













            }










            return RedirectToAction("Index", "Home");

        }
    }
}