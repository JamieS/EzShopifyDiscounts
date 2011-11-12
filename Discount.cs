using FileHelpers;

namespace EzShopifyDiscounts
{

    /// <summary>
    /// Data class that contains Shopify discount data. 
    /// </summary>
    [DelimitedRecord(","), IgnoreFirst(1), IgnoreEmptyLines()] 
    public class Discount
    {
        // File Header used for the first line of CSV files. Note that Id is not ID to prevent excel from interpreting as SYLK file.
        [FieldIgnored()]
        public static string CsvFileHeader = "Id,CODE,TYPE,VALUE,APPLIES_TO_TYPE,MINIMUM_ORDER_AMOUNT,APPLIES_TO_ID,STARTS_AT,ENDS_AT,USAGE_COUNT,USAGE_LIMIT,ENABLED";

        [FieldTrim(TrimMode.Both)]
        public string Id;

        [FieldTrim(TrimMode.Both)]
        public string Code;

        [FieldTrim(TrimMode.Both)]
        public string Type;

        [FieldTrim(TrimMode.Both)]
        public string Value;

        [FieldTrim(TrimMode.Both)]
        public string AppliesToType;

        [FieldTrim(TrimMode.Both)]
        public string MinimumOrderAmount;

        [FieldTrim(TrimMode.Both)]
        public string AppliesToId;

        [FieldTrim(TrimMode.Both)]
        public string StartsAt;

        [FieldTrim(TrimMode.Both)]
        public string EndsAt;

        [FieldTrim(TrimMode.Both)]
        public string UsageCount;

        [FieldTrim(TrimMode.Both)]
        public string UsageLimit;

        [FieldTrim(TrimMode.Both)]
        public string Enabled;
    }
}
