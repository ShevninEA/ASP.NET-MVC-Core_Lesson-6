using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateEngine.Docx;

namespace ASP.NET_MVC_Core_Lesson_6.Services.Impl
{
    internal class ProductReportWord : IProductReport
    {
        #region Private Fields

        private const string _FieldCatalogName = "CatalogName";
        private const string _FieldCatalogDescription = "CatalogDescription";
        private const string _FieldCreationDate = "CreationDate";

        private const string _FieldProduct = "Product";

        private const string _FieldProductId = "ProductId";
        private const string _FieldProductName = "ProductName";
        private const string _FieldProductCategory = "ProductCategory";
        private const string _FieldProductPrice = "ProductPrice";
        private const string _FieldProductTotal = "ProductTotal";



        private readonly FileInfo _templateFile;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateFile">Наименование файла-шаблона</param>
        public ProductReportWord(string templateFile)
        {
            _templateFile = new FileInfo(templateFile);
        }

        #endregion

        public string CatalogName { get; set; } = null!;
        public string CatalogDescription { get; set; } = null!;
        public DateTime CreationDate { get; set; }
        public IEnumerable<(int id, string name, string category, decimal price)> Products { get; set; }

        public FileInfo Create(string reportFilePath)
        {
            if (!_templateFile.Exists)
                throw new FileNotFoundException();

            var reportFile = new FileInfo(reportFilePath);
            reportFile.Delete();
            _templateFile.CopyTo(reportFile.FullName);

            var rows = Products.Select(product => new TableRowContent(new List<FieldContent>
            {
                new FieldContent(_FieldProductId, product.id.ToString()),
                new FieldContent(_FieldProductName, product.name),
                new FieldContent(_FieldProductCategory, product.category),
                new FieldContent(_FieldProductPrice, product.price.ToString("c"))

            })).ToArray();

            var content = new Content(
                new FieldContent(_FieldCatalogName, CatalogName),
                new FieldContent(_FieldCatalogDescription, CatalogDescription),
                new FieldContent(_FieldCreationDate, CreationDate.ToString("dd.MM.yyyy HH:mm:ss")),
                TableContent.Create(_FieldProduct, rows),
                new FieldContent(_FieldProductTotal, Products.Sum(product => product.price).ToString("c"))
                );

            using (var templateProcessor = new TemplateProcessor(reportFile.FullName).SetRemoveContentControls(true))
            {
                templateProcessor.FillContent(content);
                templateProcessor.SaveChanges();
                reportFile.Refresh();
                return reportFile;
            }
        }
    }
}
