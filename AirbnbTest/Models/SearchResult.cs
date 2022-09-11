namespace AirbnbTest.Models
{
    public class SearchResult
    {
        public string __typename { get; set; }
        public Listing listing { get; set; }
        public object listingParamOverrides { get; set; }
        public Pricingquote pricingQuote { get; set; }
        public object luxuryInfo { get; set; }
        public Verified verified { get; set; }
        public bool? verifiedCard { get; set; }

        public class Listing
        {
            public string __typename { get; set; }
            public float? avgRating { get; set; }
            public object businessHostLabel { get; set; }
            public Contextualpicture[] contextualPictures { get; set; }
            public Contextualpicturespageinfo contextualPicturesPageInfo { get; set; }
            public object[] formattedBadges { get; set; }
            public Homedetail[] homeDetails { get; set; }
            public string id { get; set; }
            public bool? isNewListing { get; set; }
            public bool? isSuperhost { get; set; }
            public Kickercontent kickerContent { get; set; }
            public float? lat { get; set; }
            public string listingObjType { get; set; }
            public float? lng { get; set; }
            public object localizedDistanceText { get; set; }
            public object locationTitle { get; set; }
            public object mainSectionMessage { get; set; }
            public object[] mainSectionMessages { get; set; }
            public string name { get; set; }
            public Overview[] overview { get; set; }
            public object[] pdpDisplayExtensions { get; set; }
            public string pdpType { get; set; }
            public string pdpUrlType { get; set; }
            public int personCapacity { get; set; }
            public object[] pictureUrls { get; set; }
            public string[] previewAmenityNames { get; set; }
            public object relaxedFilterLabels { get; set; }
            public int reviewsCount { get; set; }
            public string roomTypeCategory { get; set; }
            public object roomTypeId { get; set; }
            public bool? showPhotoSwipeIndicator { get; set; }
            public object summary { get; set; }
            public int tierId { get; set; }
            public string titleLocale { get; set; }
            public string avgRatingLocalized { get; set; }
            public string avgRatingA11yLabel { get; set; }
            public Structuredcontent structuredContent { get; set; }
            public bool? isAutoTranslated { get; set; }
            public string title { get; set; }
        }

        public class Contextualpicturespageinfo
        {
            public string __typename { get; set; }
            public bool? hasNextPage { get; set; }
            public string endCursor { get; set; }
        }

        public class Kickercontent
        {
            public string __typename { get; set; }
            public object kickerBadge { get; set; }
            public string[] messages { get; set; }
            public object textColor { get; set; }
        }

        public class Structuredcontent
        {
            public string __typename { get; set; }
            public object[] distance { get; set; }
            public Primaryline[] primaryLine { get; set; }
            public object secondaryLine { get; set; }
        }

        public class Primaryline
        {
            public string __typename { get; set; }
            public string body { get; set; }
            public object bodyA11yLabel { get; set; }
            public object bodyType { get; set; }
            public object headline { get; set; }
        }

        public class Contextualpicture
        {
            public string __typename { get; set; }
            public Caption caption { get; set; }
            public string id { get; set; }
            public string picture { get; set; }
        }

        public class Caption
        {
            public string __typename { get; set; }
            public object kickerBadge { get; set; }
            public string[] messages { get; set; }
        }

        public class Homedetail
        {
            public string __typename { get; set; }
            public string title { get; set; }
        }

        public class Overview
        {
            public string __typename { get; set; }
            public string title { get; set; }
        }

        public class Pricingquote
        {
            public string __typename { get; set; }
            public object applicableDiscounts { get; set; }
            public object barDisplayPriceWithoutDiscount { get; set; }
            public object canInstantBook { get; set; }
            public object displayRateDisplayStrategy { get; set; }
            public object monthlyPriceFactor { get; set; }
            public object price { get; set; }
            public object priceDropDisclaimer { get; set; }
            public object priceString { get; set; }
            public object rate { get; set; }
            public object rateType { get; set; }
            public object rateWithoutDiscount { get; set; }
            public object rateWithServiceFee { get; set; }
            public object secondaryPriceString { get; set; }
            public bool? shouldShowFromLabel { get; set; }
            public Structuredstaydisplayprice structuredStayDisplayPrice { get; set; }
            public object totalPriceDisclaimer { get; set; }
            public object totalPriceWithoutDiscount { get; set; }
            public object weeklyPriceFactor { get; set; }
        }

        public class Structuredstaydisplayprice
        {
            public string __typename { get; set; }
            public Primaryline1 primaryLine { get; set; }
            public Secondaryline secondaryLine { get; set; }
            public Explanationdata explanationData { get; set; }
            public string explanationDataDisplayPosition { get; set; }
            public object explanationDataDisplayPriceTriggerType { get; set; }
            public string layout { get; set; }
        }

        public class Primaryline1
        {
            public string __typename { get; set; }
            public string displayComponentType { get; set; }
            public string accessibilityLabel { get; set; }
            public string price { get; set; }
            public string qualifier { get; set; }
            public bool? concatQualifierLeft { get; set; }
            public object trailingContent { get; set; }
        }

        public class Secondaryline
        {
            public string __typename { get; set; }
            public string displayComponentType { get; set; }
            public string accessibilityLabel { get; set; }
            public string price { get; set; }
            public object trailingContent { get; set; }
        }

        public class Explanationdata
        {
            public string __typename { get; set; }
            public string title { get; set; }
            public Pricedetail[] priceDetails { get; set; }
        }

        public class Pricedetail
        {
            public string __typename { get; set; }
            public string displayComponentType { get; set; }
            public Item[] items { get; set; }
            public bool? renderBorderTop { get; set; }
            public object collapsable { get; set; }
        }

        public class Item
        {
            public string __typename { get; set; }
            public string displayComponentType { get; set; }
            public string description { get; set; }
            public string priceString { get; set; }
            public object explanationData { get; set; }
        }

        public class Verified
        {
            public string __typename { get; set; }
            public string badgeSecondaryText { get; set; }
            public string badgeText { get; set; }
            public bool? enabled { get; set; }
            public string kickerBadgeType { get; set; }
        }

    }
}