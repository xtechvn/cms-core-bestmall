using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Cần thêm để sử dụng LINQ (FirstOrDefault)
using System.Drawing;
using Entities.ViewModels.Products; // Cần thêm để sử dụng màu sắc


namespace Utilities
{
    public static class ExcelHelper
    {
        public static void ExportProductsToExcelWithSubProducts(List<ProductMongoDbModel> products, List<ProductMongoDbModel> sub_products, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Product Data");

                // Row 1: Main Headers
                worksheet.Cell(1, 1).Value = "Mã ID";
                worksheet.Cell(1, 2).Value = "Mã sản phẩm";
                worksheet.Cell(1, 3).Value = "Tên sản phẩm";
                worksheet.Cell(1, 4).Value = "Giá sản phẩm";
                worksheet.Cell(1, 5).Value = "Giá thấp nhất";
                worksheet.Cell(1, 6).Value = "Giá cao nhất";
                worksheet.Cell(1, 7).Value = "Link ảnh đại diện";
                worksheet.Cell(1, 8).Value = "Số sản phẩm trong kho";
                worksheet.Cell(1, 9).Value = "Mô tả chung";
                worksheet.Cell(1, 10).Value = "Mô tả công thức";
                worksheet.Cell(1, 11).Value = "Mô tả tác dụng";
                worksheet.Cell(1, 12).Value = "Mô tả cách dùng";
                worksheet.Range("M1:O1").Merge().Value = "Kích thước đóng hàng";
                worksheet.Cell(1, 16).Value = "Khối lượng (gram)";
                worksheet.Cell(1, 17).Value = "SKU";

                // Row 2: Sub-headers for "Kích thước đóng hàng" and other headers shifted down
                // Các ô này có thể để trống hoặc điền giá trị tùy theo nhu cầu hiển thị header của bạn
                worksheet.Cell(2, 13).Value = "Rộng (cm)";
                worksheet.Cell(2, 14).Value = "Cao (cm)";
                worksheet.Cell(2, 15).Value = "Sâu (cm)";
                worksheet.Cell(2, 16).Value = "Khối lượng (gram)";
                worksheet.Cell(2, 17).Value = "SKU";

                // Định dạng cho Header Cells (áp dụng cho cả 2 hàng header)
                var headerRange1 = worksheet.Range("A1:L1, P1:Q1"); // Các header không bị ảnh hưởng bởi merge
                var headerRange2 = worksheet.Range("M1:O2"); // Header "Kích thước đóng hàng" và các sub-header
                var headerRange3 = worksheet.Range("A2:L2, P2:Q2"); // Các header khác ở hàng 2

                // Định dạng cho header hàng 1 (trừ phần gộp)
                headerRange1.Style.Font.Bold = true;
                headerRange1.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange1.Style.Font.FontColor = XLColor.DarkBlue;
                headerRange1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange1.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Định dạng cho header "Kích thước đóng hàng" và các sub-header của nó
                headerRange2.Style.Font.Bold = true;
                headerRange2.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange2.Style.Font.FontColor = XLColor.DarkBlue;
                headerRange2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Định dạng cho các header còn lại ở hàng 2
                headerRange3.Style.Font.Bold = true;
                headerRange3.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange3.Style.Font.FontColor = XLColor.DarkBlue;
                headerRange3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange3.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                // Bắt đầu ghi dữ liệu từ hàng thứ 3
                int currentRow = 3;

                foreach (var product in products)
                {
                    // Ghi thông tin sản phẩm cha
                    WriteProductRow(worksheet, currentRow, product, isSubProduct: false);
                    currentRow++;

                    // Tìm và ghi thông tin các sản phẩm con
                    var relatedSubProducts = sub_products.Where(sp => sp.parent_product_id == product._id).ToList();
                    foreach (var subProduct in relatedSubProducts)
                    {
                        WriteProductRow(worksheet, currentRow, subProduct, isSubProduct: true);
                        currentRow++;
                    }
                }

                // Auto-fit columns for better readability
                worksheet.Columns().AdjustToContents();

                // Save the workbook
                workbook.SaveAs(filePath);
            }
        }

        // Hàm helper để ghi một hàng sản phẩm (cha hoặc con)
        public static void WriteProductRow(IXLWorksheet worksheet, int row, ProductMongoDbModel product, bool isSubProduct)
        {
            int startCol = isSubProduct ? 2 : 1; // Nếu là sản phẩm con, bắt đầu từ cột 2 (thụt vào)

            // Căn giữa cho các trường là chuỗi
            worksheet.Cell(row, startCol).Value = product._id;
            worksheet.Cell(row, startCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            if (isSubProduct) // Để lại cột 1 trống nếu là sub-product
            {
                // Các trường còn lại sẽ dịch chuyển tương ứng
                worksheet.Cell(row, startCol + 1).Value = product.code; // code ở cột 3
                worksheet.Cell(row, startCol + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, startCol + 2).Value = product.name; // name ở cột 4
                worksheet.Cell(row, startCol + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            else
            {
                worksheet.Cell(row, startCol + 1).Value = product.code;
                worksheet.Cell(row, startCol + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, startCol + 2).Value = product.name;
                worksheet.Cell(row, startCol + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Định dạng số có dấu phẩy và căn phải
            worksheet.Cell(row, startCol + 3).Value = product.amount;
            worksheet.Cell(row, startCol + 3).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, startCol + 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 4).Value = product.amount_min;
            worksheet.Cell(row, startCol + 4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, startCol + 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 5).Value = product.amount_max;
            worksheet.Cell(row, startCol + 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, startCol + 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 6).Value = product.avatar;
            worksheet.Cell(row, startCol + 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(row, startCol + 7).Value = product.quanity_of_stock;
            worksheet.Cell(row, startCol + 7).Style.NumberFormat.Format = "#,##0";
            worksheet.Cell(row, startCol + 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 8).Value = product.description;
            worksheet.Cell(row, startCol + 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(row, startCol + 9).Value = product.description_ingredients;
            worksheet.Cell(row, startCol + 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(row, startCol + 10).Value = product.description_effect;
            worksheet.Cell(row, startCol + 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(row, startCol + 11).Value = product.description_usepolicy;
            worksheet.Cell(row, startCol + 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Kích thước đóng hàng sẽ tương ứng với cột M, N, O (hoặc M+1, N+1, O+1 nếu thụt vào)
            worksheet.Cell(row, startCol + 12).Value = product.package_width;
            worksheet.Cell(row, startCol + 12).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, startCol + 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 13).Value = product.package_height;
            worksheet.Cell(row, startCol + 13).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, startCol + 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 14).Value = product.package_depth;
            worksheet.Cell(row, startCol + 14).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, startCol + 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 15).Value = product.weight;
            worksheet.Cell(row, startCol + 15).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, startCol + 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(row, startCol + 16).Value = product.sku;
            worksheet.Cell(row, startCol + 16).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }

}

