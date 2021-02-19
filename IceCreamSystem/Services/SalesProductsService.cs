using IceCreamSystem.DBContext;
using IceCreamSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IceCreamSystem.Services
{
    public class SalesProductsService
    {
        public static List<SaleProduct> ReturnSaleProducts(string[] productsInCookie, int companyId, int saleId)
        {
            if (productsInCookie == null || productsInCookie.Length == 0)
                return null;
            else
            {
                Context db = new Context();
                List<SaleProduct> saleProducts = new List<SaleProduct>();

                for (int i = 0; i < productsInCookie.Length; i++)
                {
                    if (!String.IsNullOrEmpty(productsInCookie[i]))
                    {
                        string[] products = productsInCookie[i].Split(',');
                        int idProd = Convert.ToInt32(products[0]);
                        Product prod = db.Product.Where(x => x.CompanyId == companyId && x.IdProduct == idProd).FirstOrDefault();

                        SaleProduct saleProduct = new SaleProduct { ProductId = prod.IdProduct, SaleId = saleId, Amount = Convert.ToInt32(products[3]) };

                        saleProducts.Add(saleProduct);
                    }
                }
                return saleProducts;

            }
        }

        public static decimal GetTotalPrice(List<SaleProduct> saleProducts)
        {
            Context db = new Context();
            decimal totalPrice = 0M;
            for (int i = 0; i < saleProducts.Count; i++)
            {
                int IdProduct = saleProducts[i].ProductId;
                Product prod = db.Product.Where(x => x.IdProduct == IdProduct).FirstOrDefault();

                decimal pricePerProduct = saleProducts[i].Amount * prod.SalePrice;

                totalPrice += pricePerProduct;
            }
            return totalPrice;
        }
    }
}

