
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;




namespace Nop.Web.MyModel
{
    public  class MyProductAttributeCallBack 
    {


        protected virtual void UpdateLocales(ProductAttribute productAttribute, ProductAttributeModel model, ILocalizedEntityService _localizedEntityService)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productAttribute,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(productAttribute,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }

        protected virtual void UpdateLocales(PredefinedProductAttributeValue ppav, PredefinedProductAttributeValueModel model, ILocalizedEntityService _localizedEntityService)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(ppav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }







        public int GetProductAttribute(IProductAttributeService _productAttributeService, string AttrubuteName)
        {

            var proAttr = _productAttributeService.GetAllProductAttributes();

            int tt = proAttr.Where(c => c.Name == AttrubuteName).Select(c=>c.Id).FirstOrDefault();

            return tt;
        }

       
        public  int Create(IProductAttributeService _productAttributeService, ILocalizedEntityService localizedEntityService,string ProductAttributeName, string ProductAttributeValue)
        {

            ProductAttributeModel model = new ProductAttributeModel();
            model.Name = ProductAttributeName;
            model.Description = ProductAttributeValue;
            if (true)
            {
                var productAttribute = model.ToEntity<ProductAttribute>();
                _productAttributeService.InsertProductAttribute(productAttribute);
               UpdateLocales(productAttribute,  model, localizedEntityService);

                
            }


            // model = _productAttributeModelFactory.PrepareProductAttributeModel(model, null, true);

            return GetProductAttribute(_productAttributeService,ProductAttributeName);
        }

        

       
        //public virtual IActionResult Edit(ProductAttributeModel model, bool continueEditing)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
        //        return AccessDeniedView();

        //    //try to get a product attribute with the specified id
        //    var productAttribute = _productAttributeService.GetProductAttributeById(model.Id);
        //    if (productAttribute == null)
        //        return RedirectToAction("List");

        //    if (ModelState.IsValid)
        //    {
        //        productAttribute = model.ToEntity(productAttribute);
        //        _productAttributeService.UpdateProductAttribute(productAttribute);

        //        UpdateLocales(productAttribute, model);

        //        //activity log
        //        _customerActivityService.InsertActivity("EditProductAttribute",
        //            string.Format(_localizationService.GetResource("ActivityLog.EditProductAttribute"), productAttribute.Name), productAttribute);

        //        _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Updated"));

        //        if (!continueEditing)
        //            return RedirectToAction("List");

        //        return RedirectToAction("Edit", new { id = productAttribute.Id });
        //    }

        //    //prepare model
        //    model = _productAttributeModelFactory.PrepareProductAttributeModel(model, productAttribute, true);

        //    //if we got this far, something failed, redisplay form
        //    return View(model);
        //}

       
       

       

        

      
       

       
    }
}