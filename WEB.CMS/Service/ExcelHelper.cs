using ClosedXML.Excel;
using Entities.Models;
using Entities.ViewModels.Products;
using System.Linq;
using WEB.CMS.Service.Product; // Add this for LINQ methods

namespace Utilities
{
    public static class ExcelHelper
    {
        public static void ExportProductsToExcel(
            List<ProductMongoDbModel> products,
            List<ProductMongoDbModel> sub_product, // This is already in your provided code
            List<GroupProduct> groupProducts,
            List<Label> labels,
            List<Supplier> suppliers,
            string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sản phẩm");

                // --- Cập nhật Headers ---
                // Row 1: Main Headers
                worksheet.Cell(1, 1).Value = "Mã ID";
                worksheet.Cell(1, 2).Value = "Mã sản phẩm";
                worksheet.Cell(1, 3).Value = "Tên sản phẩm";
                worksheet.Cell(1, 4).Value = "Giá sản phẩm";
                worksheet.Cell(1, 5).Value = "Giá thấp nhất";
                worksheet.Cell(1, 6).Value = "Giá cao nhất";
                worksheet.Cell(1, 7).Value = "Link ảnh đại diện (Avatar)";
                worksheet.Cell(1, 8).Value = "Link ảnh sản phẩm"; 
                worksheet.Cell(1, 9).Value = "Số sản phẩm trong kho";
                worksheet.Cell(1, 10).Value = "Ngành hàng sản phẩm"; 
                worksheet.Cell(1, 11).Value = "Nhãn hàng";
                // Cột mới cho label_id
                worksheet.Cell(1, 12).Value = "Nhà cung cấp";
                // Cột mới cho supplier_id

                // Kích thước đóng hàng vẫn gộp
                worksheet.Range("M1:O1").Merge().Value = "Kích thước đóng hàng";
                worksheet.Cell(1, 16).Value = "Khối lượng (gram)";
                worksheet.Cell(1, 17).Value = "SKU";
                // Row 2: Sub - headers for "Kích thước đóng hàng"

                worksheet.Cell(2, 13).Value = "Rộng (cm)";
                worksheet.Cell(2, 14).Value = "Cao (cm)";
                worksheet.Cell(2, 15).Value = "Sâu (cm)";
                // --- Định dạng Header Cells ---
                // Các header ở hàng 1 không bị gộp
                var headerRangeA1L1 = worksheet.Range("A1:L1");
                headerRangeA1L1.Style.Font.Bold = true;
                headerRangeA1L1.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRangeA1L1.Style.Font.FontColor = XLColor.DarkBlue;
                headerRangeA1L1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRangeA1L1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                var headerRangeP1Q1 = worksheet.Range("P1:Q1");
                headerRangeP1Q1.Style.Font.Bold = true;
                headerRangeP1Q1.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRangeP1Q1.Style.Font.FontColor = XLColor.DarkBlue;
                headerRangeP1Q1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRangeP1Q1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                // Header "Kích thước đóng hàng" và các sub-header của nó
                var headerRangeM1O2 = worksheet.Range("M1:O2");
                headerRangeM1O2.Style.Font.Bold = true;
                headerRangeM1O2.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRangeM1O2.Style.Font.FontColor = XLColor.DarkBlue;
                headerRangeM1O2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRangeM1O2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                // Các header còn lại ở hàng 2
                var headerRangeA2L2 = worksheet.Range("A2:L2");
                // Vùng này sẽ trống ở hàng 2, nhưng vẫn định dạng nền
                headerRangeA2L2.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRangeA2L2.Style.Font.FontColor = XLColor.DarkBlue;
                headerRangeA2L2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRangeA2L2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                var headerRangeP2Q2 = worksheet.Range("P2:Q2");
                headerRangeP2Q2.Style.Font.Bold = true;
                headerRangeP2Q2.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRangeP2Q2.Style.Font.FontColor = XLColor.DarkBlue;
                headerRangeP2Q2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRangeP2Q2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                // --- Ghi dữ liệu sản phẩm ---
                int currentRow = 3;

                // Lọc chỉ lấy các sản phẩm chính (không có parent_product_id)
                var mainProducts = products.Where(p => string.IsNullOrEmpty(p.parent_product_id)).ToList();
                foreach (var product in mainProducts)
                {
                    // Mã ID
                    worksheet.Cell(currentRow, 1).Value = product._id;
                    worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Mã sản phẩm
                    worksheet.Cell(currentRow, 2).Value = product.code;
                    worksheet.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Tên sản phẩm
                    worksheet.Cell(currentRow, 3).Value = product.name;
                    worksheet.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Giá sản phẩm - Đã đổi định dạng số tiền
                    worksheet.Cell(currentRow, 4).Value = product.amount;
                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    // Giá thấp nhất - Đã đổi định dạng số tiền
                    worksheet.Cell(currentRow, 5).Value = product.amount_min;
                    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    // Giá cao nhất - Đã đổi định dạng số tiền
                    worksheet.Cell(currentRow, 6).Value = product.amount_max;
                    worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    // Link ảnh đại diện (Avatar)
                    worksheet.Cell(currentRow, 7).Value = product.avatar;
                    worksheet.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Link ảnh sản phẩm (product.images)
                    worksheet.Cell(currentRow, 8).Value = string.Join(", ", product.images ?? new List<string>());
                    worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    // Số sản phẩm trong kho
                    worksheet.Cell(currentRow, 9).Value = product.quanity_of_stock;
                    worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // Ngành hàng sản phẩm (group_product_id)
                    worksheet.Cell(currentRow, 10).Value = FormatGroupProduct(product.group_product_id, groupProducts);
                    worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Nhãn hàng (label_id)
                    worksheet.Cell(currentRow, 11).Value = GetLabelName(product.label_id, labels);
                    worksheet.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Nhà cung cấp (supplier_id)
                    worksheet.Cell(currentRow, 12).Value = GetSupplierName(product.supplier_id, suppliers);
                    worksheet.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    //// Kích thước đóng hàng
                    worksheet.Cell(currentRow, 13).Value = product.package_width;
                    worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    worksheet.Cell(currentRow, 14).Value = product.package_height;
                    worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(currentRow, 15).Value = product.package_depth;
                    worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    //// Khối lượng
                    worksheet.Cell(currentRow, 16).Value = product.weight;
                    worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    //// SKU
                    worksheet.Cell(currentRow, 17).Value = product.sku;
                    worksheet.Cell(currentRow, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    currentRow++;

                   
                    var associatedSubProducts = sub_product.Where(sp => sp.parent_product_id == product._id).ToList();
                    foreach (var subProduct in associatedSubProducts)
                    {
                        // Merge cells A to C (columns 1 to 3) for sub-product
                        var mergedCellRange = worksheet.Range(currentRow, 1, currentRow, 3);
                        mergedCellRange.Merge();
                        mergedCellRange.Value = ProductVariationHelper.RenderVariationDetail(subProduct.attributes,subProduct.attributes_detail,subProduct.variation_detail); // Assuming this helper function exists
                        mergedCellRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        mergedCellRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        mergedCellRange.Style.Fill.BackgroundColor = XLColor.LightGray; 

                        worksheet.Cell(currentRow, 4).Value = subProduct.amount;
                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        //worksheet.Cell(currentRow, 5).Value = subProduct.amount_min;
                        //worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                        //worksheet.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        //worksheet.Cell(currentRow, 6).Value = subProduct.amount_max;
                        //worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                        //worksheet.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        worksheet.Cell(currentRow, 7).Value = subProduct.avatar;
                        worksheet.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(currentRow, 8).Value = string.Join(", ", subProduct.images ?? new List<string>());
                        worksheet.Cell(currentRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        worksheet.Cell(currentRow, 9).Value = subProduct.quanity_of_stock;
                        worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        worksheet.Cell(currentRow, 10).Value = FormatGroupProduct(subProduct.group_product_id, groupProducts);
                        worksheet.Cell(currentRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(currentRow, 11).Value = GetLabelName(subProduct.label_id, labels);
                        worksheet.Cell(currentRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(currentRow, 12).Value = GetSupplierName(subProduct.supplier_id, suppliers);
                        worksheet.Cell(currentRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(currentRow, 13).Value = subProduct.package_width;
                        worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        worksheet.Cell(currentRow, 14).Value = subProduct.package_height;
                        worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        worksheet.Cell(currentRow, 15).Value = subProduct.package_depth;
                        worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(currentRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        worksheet.Cell(currentRow, 16).Value = subProduct.weight;
                        worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        worksheet.Cell(currentRow, 17).Value = subProduct.sku;
                        worksheet.Cell(currentRow, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        currentRow++;
                    }
                }

                // Auto-fit columns for better readability
                worksheet.Columns().AdjustToContents();
                // Save the workbook
                workbook.SaveAs(filePath);
            }
        }


        private static string FormatGroupProduct(string groupProductIds, List<GroupProduct> groupProducts)
        {
            if (string.IsNullOrEmpty(groupProductIds) || groupProducts == null)
            {
                return "";
            }

            var ids = groupProductIds.Split(',').Select(s => int.TryParse(s.Trim(), out int id) ? (int?)id : null).Where(id => id.HasValue).Select(id => id.Value).ToList();
            var names = new List<string>();
            foreach (var id in ids)
            {
                var group = groupProducts.FirstOrDefault(g => g.Id == id);
                if (group != null)
                {
                    names.Add(group.Name);
                }
            }
            return string.Join(" > ", names);
        }

        private static string GetLabelName(int? labelId, List<Label> labels)
        {
            if (!labelId.HasValue || labels == null)
            {
                return "";
            }
            return labels.FirstOrDefault(l => l.Id == labelId.Value)?.LabelName ?? "";
        }

        private static string GetSupplierName(int? supplierId, List<Supplier> suppliers)
        {
            if (!supplierId.HasValue || suppliers == null)
            {
                return "";
            }
            return suppliers.FirstOrDefault(s => s.SupplierId == supplierId.Value)?.FullName ?? "";
        }
    }

  
}