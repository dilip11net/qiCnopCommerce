using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nop.Web.MyModel
{
    public class ConsumeApi
    {

        public async Task<List<ProductItem>> GetAPIProducts()
        {
            HttpClient client1 = new HttpClient();
            List<ProductItem> list = new List<ProductItem>();
            var uri = "https://www.stockapi.in/api/nopNewOrUpdatedItemMaster";
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            var httpContent = new StringContent(json);
            using (var response = await client1.PostAsync(uri, httpContent))
            {
                var responseData = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<ProductItem>>(responseData);


            }






            return list;
        }

        public async Task<List<AttributesAndValues>> GetAttributeAndValuesProducts()
        {
            HttpClient client11 = new HttpClient();
            List<AttributesAndValues> list = new List<AttributesAndValues>();
            var uri = "https://www.stockapi.in/api/nopItemAttributesAndValues";
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            var httpContent = new StringContent(json);
            using (var response = await client11.PostAsync(uri, httpContent))
            {
                var responseData = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<AttributesAndValues>>(responseData);


            }






            return list;
        }
        public async Task<CategoriesAll> GetCategoriesAndAll()
        {
            HttpClient client11 = new HttpClient();
            CategoriesAll list = new CategoriesAll();
            var uri = "https://www.stockapi.in/api/nopCategoriesAndAll";
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            var httpContent = new StringContent(json);
            using (var response = await client11.PostAsync(uri, httpContent))
            {
                var responseData = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<CategoriesAll>(responseData);


            }






            return list;
        }
        public async Task<List<ItemImage>> GetProductImages()
        {
            HttpClient client11 = new HttpClient();
            List<ItemImage> list = new List<ItemImage>();
            var uri = "https://www.stockapi.in/api/nopItemImages";
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            var httpContent = new StringContent(json);
            using (var response = await client11.PostAsync(uri, httpContent))
            {
                var responseData = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<ItemImage>>(responseData);


            }






            return list;
        }
    }
}
