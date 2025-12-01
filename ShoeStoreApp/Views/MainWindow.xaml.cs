using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShoeStoreApp.Views
{
    public partial class MainWindow : Window
    {
        private List<ProductViewModel> allProducts;
        private List<ProductViewModel> filteredProducts;

        public MainWindow()
        {
            InitializeComponent();
            SetupUserInterface();
            LoadProducts();
        }

        private void SetupUserInterface()
        {
            if (LoginWindow.CurrentUser == null)
            {
                TxtUserInfo.Text = "Гость";
            }
            else
            {
                var user = LoginWindow.CurrentUser;
                TxtUserInfo.Text = user.LastName + " " + user.FirstName + " " + user.Patronymic;

                if (user.Role.RoleName == "Менеджер" || user.Role.RoleName == "Администратор")
                {
                    EnableFiltersAndSearch();
                    BtnOrders.Visibility = Visibility.Visible;
                }

                if (user.Role.RoleName == "Администратор")
                {
                    BtnAddProduct.Visibility = Visibility.Visible;
                }
            }
        }

        private void EnableFiltersAndSearch()
        {
            foreach (UIElement element in PanelFilters.Children)
            {
                element.Visibility = Visibility.Visible;
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var db = new ShoeStoreDBEntities())
                {
                    var products = db.Products
                        .Include("Category")
                        .Include("Manufacturer")
                        .Include("Supplier")
                        .Include("Unit")
                        .ToList();

                    allProducts = products.Select(p => new ProductViewModel
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        CategoryName = p.Category.CategoryName,
                        ManufacturerName = p.Manufacturer.ManufacturerName,
                        SupplierName = p.Supplier.SupplierName,
                        UnitName = p.Unit.UnitName,
                        Price = p.Price,
                        Discount = p.Discount,
                        QuantityInStock = p.QuantityInStock,
                        Photo = p.Photo
                    }).ToList();


                    filteredProducts = new List<ProductViewModel>(allProducts);
                    UpdateProductsList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProductsList()
        {
            ProductsList.ItemsSource = null;
            ProductsList.ItemsSource = filteredProducts;
        }

        private void ApplyFilters()
        {
            filteredProducts = new List<ProductViewModel>(allProducts);

            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                string searchText = TxtSearch.Text.ToLower();
                filteredProducts = filteredProducts.Where(p =>
                    p.ProductName.ToLower().Contains(searchText) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchText)) ||
                    p.ManufacturerName.ToLower().Contains(searchText)
                ).ToList();
            }

            if (CmbFilter.SelectedIndex > 0)
            {
                switch (CmbFilter.SelectedIndex)
                {
                    case 1:
                        filteredProducts = filteredProducts.Where(p => p.QuantityInStock > 0).ToList();
                        break;
                    case 2:
                        filteredProducts = filteredProducts.Where(p => p.Discount > 0).ToList();
                        break;
                    case 3:
                        filteredProducts = filteredProducts.Where(p => p.Discount == 0).ToList();
                        break;
                }
            }

            if (CmbSort.SelectedIndex > 0)
            {
                switch (CmbSort.SelectedIndex)
                {
                    case 1:
                        filteredProducts = filteredProducts.OrderBy(p => p.FinalPrice).ToList();
                        break;
                    case 2:
                        filteredProducts = filteredProducts.OrderByDescending(p => p.FinalPrice).ToList();
                        break;
                    case 3:
                        filteredProducts = filteredProducts.OrderBy(p => p.ProductName).ToList();
                        break;
                    case 4:
                        filteredProducts = filteredProducts.OrderByDescending(p => p.ProductName).ToList();
                        break;
                }
            }

            UpdateProductsList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allProducts != null)
                ApplyFilters();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allProducts != null)
                ApplyFilters();
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (allProducts != null)
                ApplyFilters();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функционал просмотра заказов будет реализован в следующих модулях.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функционал добавления товара будет реализован в следующих модулях.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.CurrentUser = null;
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }

    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
        public string SupplierName { get; set; }
        public string UnitName { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int QuantityInStock { get; set; }
        public string Photo { get; set; }

        public decimal FinalPrice
        {
            get
            {
                if (Discount > 0)
                    return Price * (1 - Discount / 100);
                return Price;
            }
        }

        public bool HasDiscount
        {
            get { return Discount > 0; }
        }

        public bool HasHighDiscount
        {
            get { return Discount > 15; }
        }

        public bool IsOutOfStock
        {
            get { return QuantityInStock == 0; }
        }

        public Visibility HasDiscountVisibility
        {
            get { return HasDiscount ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string PhotoPath
        {
            get
            {
                if (string.IsNullOrEmpty(Photo))
                    return "/Resources/picture.png";
                return "/Resources/" + Photo;
            }
        }

        public string PriceFormatted
        {
            get { return Price.ToString("N2") + " руб"; }
        }

        public string FinalPriceFormatted
        {
            get { return FinalPrice.ToString("N2") + " руб"; }
        }

        public string DiscountFormatted
        {
            get { return "Скидка: " + Discount + "%"; }
        }

        public string StockFormatted
        {
            get
            {
                if (IsOutOfStock)
                    return "Нет в наличии";
                return "На складе: " + QuantityInStock + " " + UnitName;
            }
        }

        public string StockColor
        {
            get { return IsOutOfStock ? "#FF0000" : "#4CAF50"; }
        }
    }
}