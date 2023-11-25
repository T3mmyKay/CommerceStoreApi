namespace T3mmyStoreApi.Services
{
    public class OrderHelper
    {
        public static decimal ShippingFee { get; } = 5;

        public static Dictionary<string, string> PaymentMethods { get; } = new()
        {
            {"Cash", "Cash on delivery" },
            {"Paypal", "Paypal" },
            {"Credit card", "Credit Card" }
        };

        public static List<string> PaymentStatuses { get; } = new()
        {
            "Pending","Accepted","Cancelled"
        };

        public static List<string> OrderStatuses { get; } = new() { "Created", "Accepted", "Cancelled", "Shipped", "Delivered", "Returned" };
        /*
         * Recieves a string of product identifiers, seperated by '-'
         * Example: 9-9-7-9-6
         * 
         * Returns a list of pairs(dictionary):
         *      -the pair name is the product ID
         *      -the pair value is the product quantity
         *      
         * Example:
         * {
         *      9: 3,
         *      7: 1,
         *      6: 1,
         * }
         * 
         */
        public static Dictionary<int, int> GetProductDictionary(string productIdentifiers)
        {
            var productDictionary = new Dictionary<int, int>();
            if (productIdentifiers.Length > 0)
            {
                string[] productArray = productIdentifiers.Split('-');
                foreach (var productId in productArray)
                {
                    try
                    {
                        int id = int.Parse(productId);
                        if (productDictionary.ContainsKey(id))
                        {
                            productDictionary[id] += 1;

                        }
                        else
                        {
                            productDictionary.Add(id, 1);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return productDictionary;
        }
    }
}
