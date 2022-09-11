namespace AirbnbTest.Models
{
    public class AutocompleteResult
    {
        public Autocomplete_Terms[] autocomplete_terms { get; set; }
        public Metadata metadata { get; set; }
        public Terms_Offsets terms_offsets { get; set; }
        public string autocomplete_result_title { get; set; }
        public object[] experiments_to_log { get; set; }
        public bool disable_cdn_cache { get; set; }
        public string autocompleteResultTitle { get; set; }

        public class Metadata
        {
            public string request_id { get; set; }
        }

        public class Terms_Offsets
        {
        }

        public class Autocomplete_Terms
        {
            public string id { get; set; }
            public Explore_Search_Params explore_search_params { get; set; }
            public string suggestion_type { get; set; }
            public string vertical_type { get; set; }
            public string display_name { get; set; }
            public Metadata1 metadata { get; set; }
            public Location location { get; set; }
            public object[] refinements { get; set; }
            public Highlight[] highlights { get; set; }
            public Sxs_Debug_Info sxs_debug_info { get; set; }
            public string suggestionType { get; set; }
            public string verticalType { get; set; }
        }

        public class Explore_Search_Params
        {
            public Param[] _params { get; set; }
            public string place_id { get; set; }
            public string query { get; set; }
            public string[] refinement_paths { get; set; }
            public string refinement_path { get; set; }
            public string tab_id { get; set; }
            public bool reset_filters { get; set; }
            public object[] reset_keys { get; set; }
        }

        public class Param
        {
            public string key { get; set; }
            public string value_type { get; set; }
            public bool in_array { get; set; }
            public string value { get; set; }
            public bool delete { get; set; }
            public bool invisible_to_user { get; set; }
        }

        public class Metadata1
        {
            public bool location_only_result { get; set; }
            public string airmoji { get; set; }
        }

        public class Location
        {
            public int offset_start { get; set; }
            public int offset_end { get; set; }
            public string location_name { get; set; }
            public string google_place_id { get; set; }
            public string[] types { get; set; }
            public Term[] terms { get; set; }
            public string parent_city_display_name { get; set; }
            public string country_code { get; set; }
            public string countryCode { get; set; }
            public string parentCityDisplayName { get; set; }
            public string parent_city_place_id { get; set; }
            public string parentCityPlaceId { get; set; }
        }

        public class Term
        {
            public int offset { get; set; }
            public string value { get; set; }
        }

        public class Sxs_Debug_Info
        {
            public Sxs_Score_Attrs sxs_score_attrs { get; set; }
            public Sxs_Debug_Metadata sxs_debug_metadata { get; set; }
        }

        public class Sxs_Score_Attrs
        {
        }

        public class Sxs_Debug_Metadata
        {
        }

        public class Highlight
        {
            public int offset_start { get; set; }
            public int offset_end { get; set; }
        }

    }
}