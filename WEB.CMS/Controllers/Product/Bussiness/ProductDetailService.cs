using Caching.RedisWorker;
using Entities.ViewModels.Products;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Utilities.Contants.ProductV2;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers.Product.Bussiness
{
    public class ProductDetailService
    {
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly ProductSpecificationMongoAccess _productSpecificationMongoAccess;
        private readonly IConfiguration _configuration;
        public ProductDetailService(IConfiguration configuration)
        {
            _productV2DetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _productSpecificationMongoAccess = new ProductSpecificationMongoAccess(configuration);
            _configuration = configuration;
        }

        public async Task<List<ProductMongoDbModel>> ConvertToProducts(List<ProductExcelUploadModel> request)
        {
            var list=new List<ProductMongoDbModel>();
            try
            {
                foreach (var item in request)
                {
                    ProductMongoDbModel model = new ProductMongoDbModel()
                    {
                        amount = item.price + item.profit,
                        amount_max = null,
                        amount_min = null,
                        attributes = null,
                        attributes_detail = null,
                        avatar = item.avatar,
                        code = item.product_code,
                        condition_of_product = 0,
                        created_date = DateTime.Now,
                        description = item.description,
                        discount = 0,
                        discount_group_buy = new List<ProductDiscountOnGroupsBuyModel>(),
                        group_product_id = item.group_product_id.ToString(),
                        images = new List<string>(),
                        is_one_weight = true,
                        name = item.name,
                        package_depth = item.depth,
                        package_height = item.height,
                        package_width = item.width,
                        parent_product_id = null,
                        preorder_status = 0,
                        price = item.price,
                        profit = item.profit,
                        quanity_of_stock = item.stock,
                        sku = item.sku,
                        specification = new List<ProductSpecificationDetailMongoDbModel>(),
                        star = 0,
                        status = (int)ProductStatus.ACTIVE,
                        updated_last = DateTime.Now,
                        variation_detail = new List<ProductDetailVariationAttributesMongoDbModel>(),
                        videos = item.video==null || item.video.Trim()==""?new List<string>(): new List<string>() { item.video },
                        weight = item.weight,
                    };

                    if(item.image_1!=null && item.image_1.Trim()!="") model.images.Add(item.image_1);
                    if(item.image_2!=null && item.image_2.Trim()!="") model.images.Add(item.image_2);
                    if(item.image_3!=null && item.image_3.Trim()!="") model.images.Add(item.image_3);
                    if(item.image_4!=null && item.image_4.Trim()!="") model.images.Add(item.image_4);
                    if(item.image_5!=null && item.image_5.Trim()!="") model.images.Add(item.image_5);
                    if(item.image_6!=null && item.image_6.Trim()!="") model.images.Add(item.image_6);
                    if(item.image_7!=null && item.image_7.Trim()!="") model.images.Add(item.image_7);
                    if(item.image_8!=null && item.image_8.Trim()!="") model.images.Add(item.image_8);
                    if(item.brand != null && item.brand.Trim()!="")
                    {
                        //-- Add new "Brand" specification if not exists:
                        var exists_spec = await _productSpecificationMongoAccess.GetByNameAndType(1, item.brand);
                       if(exists_spec!=null && exists_spec._id != null)
                        {

                        }
                        else
                        {
                            exists_spec = new ProductSpecificationMongoDbModel();
                            exists_spec._id= await _productSpecificationMongoAccess.AddNewAsync(new ProductSpecificationMongoDbModel()
                            {
                                attribute_type = 1,
                                attribute_name = item.brand,
                            });
                        }
                        model.specification.Add(new ProductSpecificationDetailMongoDbModel()
                        {
                            _id = "-1",
                            attribute_id = 1,
                            type_ids = exists_spec._id,
                            value = item.brand,
                            value_type = 1
                        });
                    }
                    if (item.attribute_1_name != null && item.attribute_1_name.Trim() != "")
                    {
                        //--Attributes 1:
                        model.attributes = new List<ProductAttributeMongoDbModel>()
                        {
                            new ProductAttributeMongoDbModel(){_id="0",name=item.attribute_1_name.Trim()}
                        };
                        //--Attribute Detail 1:
                        model.attributes_detail = new List<ProductAttributeMongoDbModelItem>()
                        {
                            new ProductAttributeMongoDbModelItem(){attribute_id="0",name=item.variation_1_name.Trim()}
                        };
                        //-- Variation 1
                        model.variation_detail = new List<ProductDetailVariationAttributesMongoDbModel>()
                        {
                            new ProductDetailVariationAttributesMongoDbModel(){id="0",name=item.variation_1_name.Trim()}
                        };
                        //--Attributes 2 & Attribute Detail 2 & Variation 2:
                        if (item.attribute_2_name != null && item.attribute_2_name.Trim() != "")
                        {
                            model.attributes.Add(new ProductAttributeMongoDbModel() { _id = "1", name = item.attribute_2_name.Trim() });
                            model.attributes_detail.Add(new ProductAttributeMongoDbModelItem() { attribute_id = "1", name = item.variation_2_name.Trim() });
                            model.variation_detail.Add(new ProductDetailVariationAttributesMongoDbModel() { id = "1", name = item.variation_2_name.Trim() });
                        }
                       
                        //-- Check if main_products exists? :
                        if (list.Any(x =>  item.sku.ToLower().Trim() == x.sku.ToLower().Trim()
                            && (x.parent_product_id == null || x.parent_product_id.Trim() == "")
                            && (x._id != null && x._id.Trim() != "")
                            ))
                        {
                            var main_product = list.First(x =>
                             item.sku.ToLower().Trim() == x.sku.ToLower().Trim()
                            && (x.parent_product_id==null || x.parent_product_id.Trim()=="")
                            && (x._id != null && x._id.Trim() != "")
                            );
                            list.Remove(main_product);
                            model.parent_product_id = main_product._id;
                            //var list_sub = list.Where(x => x.parent_product_id == main_product._id).ToList();
                            //list.RemoveAll(x => x.parent_product_id == main_product._id);
                            main_product.attributes.AddRange(model.attributes);
                            main_product.attributes = main_product.attributes.GroupBy(x => x._id).Select(x => x.First()).ToList();
                            main_product.attributes_detail.AddRange(model.attributes_detail);
                            main_product.attributes_detail = main_product.attributes_detail.GroupBy(x=>x.name).Select(x=>x.First()).ToList();
                            //model.attributes = main_product.attributes;
                            //model.attributes_detail = main_product.attributes_detail;
                            if (main_product.amount_min == null || (double)main_product.amount_min > model.amount) main_product.amount_min = model.amount;
                            if (main_product.amount_max == null || (double)main_product.amount_max < model.amount) main_product.amount_max = model.amount;
                            if (item.weight != main_product.weight) main_product.is_one_weight = false;
                            list.Add(main_product);

                        }
                        //-- Main products not exists, add main & product first variation:
                        else
                        {
                            var main_product = JsonConvert.DeserializeObject<ProductMongoDbModel>(JsonConvert.SerializeObject(model));
                            main_product.variation_detail = null;
                            main_product.parent_product_id = null;
                            main_product.weight = null;
                            main_product.package_depth = null;
                            main_product.package_height = null;
                            main_product.package_width = null;
                            //---- Check exists SKU & name:
                            var exists = await _productV2DetailMongoAccess.GetByNameAndSKU(main_product.name.Trim(), main_product.sku.Trim());
                            if (exists != null && exists._id != null)
                            {
                                main_product._id = exists._id;
                                await _productV2DetailMongoAccess.RemoveSubProductByParentId(exists._id);
                            }
                            else
                            {
                                await _productV2DetailMongoAccess.AddNewAsync(main_product);
                            }
                            list.Add(main_product);
                            model.parent_product_id = main_product._id;
                        }

                    }
                    else
                    {
                        //---- Check exists SKU & name:
                        var exists = await _productV2DetailMongoAccess.GetByNameAndSKU(model.name.Trim(), model.sku.Trim());
                        if(exists!=null && exists._id != null)
                        {
                            model._id = exists._id;
                            await _productV2DetailMongoAccess.RemoveSubProductByParentId(exists._id);

                        }
                        else
                        {
                           await _productV2DetailMongoAccess.AddNewAsync(model);
                        }
                    }
                    model.sku = item.variation_sku;
                    list.Add(model);
                }
                foreach (var product in list)
                {
                    //-- if parent, check if have child  && just 1 child, its is single product,remove parent_product :
                    if (product._id != null && product._id.Trim() != "")
                    {
                        var childs = list.Where(x => x.parent_product_id != null && x.parent_product_id == product._id);
                        if (childs.Count() == 1)
                        {
                            product.status = 2; // status deactive 
                        }
                        continue;
                    }
                }

               
                foreach (var product in list)
                {
                    if (product.status == 2) continue;
                    product.created_date=DateTime.Now;
                    product.updated_last = DateTime.Now;
                    if (product._id==null || product._id.Trim() == "")
                    {
                        if(product.parent_product_id!=null && product.parent_product_id.Trim() != "")
                        {
                            var parent = list.First(x => x._id == product.parent_product_id);
                            if (parent.status == 2)
                            {
                                product.variation_detail = null;
                                product.attributes = null;
                                product.attributes_detail = null;
                                product.parent_product_id = null;
                                product.is_one_weight = true;
                                product.amount_min = parent.amount;
                                product.amount_max = parent.amount;
                            }
                            else
                            {
                                product.attributes = parent.attributes;
                                product.attributes_detail = parent.attributes_detail;
                                product.amount_min = parent.amount_min;
                                product.amount_max = parent.amount_max;
                                product.is_one_weight = parent.is_one_weight;
                            }
                        }
                        if ((product.parent_product_id == null || product.parent_product_id.Trim() == "") && product.variation_detail!=null && product.variation_detail.Count>0)
                        {
                            continue;
                        }
                         await _productV2DetailMongoAccess.AddNewAsync(product);
                    }
                    else
                    {

                        await _productV2DetailMongoAccess.UpdateAsync(product);
                    }

                }
                if (list != null && list.Count > 0)
                {
                    list = list.Where(x => x._id!=null && x._id.Trim()!="" && x.status==1 &&( x.parent_product_id == null || x.parent_product_id.Trim() == "")).ToList();
                }
            }
            catch(Exception ex) 
            {

            }
            return list;
        
        }
    }
   
}
