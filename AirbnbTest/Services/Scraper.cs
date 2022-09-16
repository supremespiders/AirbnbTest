using System;
using AirbnbTest.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AirbnbTest.Models;
using ExcelHelperExe;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AirbnbTest.Services
{
    public class Scraper
    {
        private readonly HttpClient _client;
        private List<HttpClient> _clients;
        private int _idx;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly int _threads;
        private bool _userProxies = false;
        Regex _numberOnly = new Regex("[^0-9]");

        public Scraper(int threads)
        {
            _threads = threads;
            _client = new HttpClient(new HttpClientHandler()
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            _client.DefaultRequestHeaders.Add("x-airbnb-api-key", "d306zoyjsyarp7ifhu67rjxn52tv0t20");
            _client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            if (File.Exists("proxies.txt"))
            {
                InitClientProxies();
                _clients.Add(_client); //add your ip to proxies
            }
        }

        private void InitClientProxies()
        {
            _clients = new List<HttpClient>();
            var proxies = File.ReadAllLines("proxies.txt");
            foreach (var p in proxies)
            {
                var pp = p.Split(':');
                var proxy = new WebProxy($"{pp[0]}:{pp[1]}", true)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(pp[2], pp[3]),
                };
                _clients.Add(new HttpClient(new HttpClientHandler()
                {
                    Proxy = proxy,
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                }));
            }
        }

        async Task<HttpClient> GetNextClient()
        {
            if (!_userProxies) return _client;
            await _semaphore.WaitAsync();
            var client = _clients[_idx];
            _idx++;
            if (_idx == _clients.Count)
                _idx = 0;
            _semaphore.Release();
            return client;
        }

        async Task<string> Work(string url)
        {
            var client = await GetNextClient();
            var doc = await client.GetHtml(url).ToDoc();
            return url;
        }

        async Task<string> Autocomplete(string keyword)
        {
            var json = await _client.GetHtml($"https://www.airbnb.com/api/v2/autocompletes?language=en&locale=en&num_results=5&user_input={keyword}&api_version=1.2.0&vertical_refinement=homes");
            
            var autocompleteResult = JsonConvert.DeserializeObject<AutocompleteResult>(json);
            var r = autocompleteResult.autocomplete_terms.First();
            Notifier.Log($"Searched {keyword} , first : {r.display_name} google place ID : {r.location.google_place_id}");
            return r.location.google_place_id;
        }

        async Task<List<string>> Search(string url)
        {
            var allItems = new List<string>();
            var json = await _client.GetHtml(url);
            JObject obj;
            try
            {
                obj = JObject.Parse(json);
            }
            catch (Exception e)
            {
                File.WriteAllText($"lastJson", json);
                throw new KnownException($"Failed to parse json");
            }

            var itemsCount = (int)obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_NUMBERED_PAGINATION')].section.totalInventoryCount");
            var maxItemsCount = (int)obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_NUMBERED_PAGINATION')].section.maxTotalInventoryCount");
            var itemsJson = obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_SECTION_WRAPPER')].section.child.section.items")?.ToString();
            if (itemsJson == null)
                throw new KnownException($"Null items");
            var items = JsonConvert.DeserializeObject<List<SearchResult>>(itemsJson);
            foreach (var searchResult in items)
                allItems.Add(searchResult.listing.id);

            return allItems.ToList();
        }

        async Task<List<string>> Search(string placeId, int min, int max)
        {
            var allItems = new List<string>();
            var index = 0;
            var page = 1;
            do
            {
                // var url = "https://www.airbnb.com/api/v3/StaysSearch?operationName=StaysSearch&locale=en&currency=USD&variables={\"isInitialLoad\":true,\"hasLoggedIn\":false,\"cdnCacheSafe\":false,\"source\":\"EXPLORE\",\"exploreRequest\":{\"metadataOnly\":false,\"version\":\"1.8.3\",\"itemsPerGrid\":20,\"tabId\":\"home_tab\",\"refinementPaths\":[\"/homes\"],\"placeId\":\"" + placeId + "\",\"datePickerType\":\"flexible_dates\",\"checkin\":\"2022-12-01\",\"checkout\":\"2022-12-01\",\"source\":\"structured_search_input_header\",\"searchType\":\"filter_change\",\"flexibleTripDates\":[\"april\",\"august\",\"december\",\"february\",\"january\",\"july\",\"june\",\"march\",\"may\",\"november\",\"october\",\"september\"],\"flexibleTripLengths\":[\"one_week\"],\"priceMax\":" + max + ",\"priceMin\":" + min + ",\"federatedSearchSessionId\":\"6b699f43-1974-43c0-828e-91a3c9519ac4\",\"itemsOffset\":" + index + ",\"sectionOffset\":2,\"query\":\"\",\"cdnCacheSafe\":false,\"treatmentFlags\":[\"flex_destinations_june_2021_launch_web_treatment\",\"new_filter_bar_v2_fm_header\",\"new_filter_bar_v2_and_fm_treatment\",\"merch_header_breakpoint_expansion_web\",\"flexible_dates_12_month_lead_time\",\"storefronts_nov23_2021_homepage_web_treatment\",\"lazy_load_flex_search_map_compact\",\"lazy_load_flex_search_map_wide\",\"im_flexible_may_2022_treatment\",\"im_flexible_may_2022_treatment\",\"flex_v2_review_counts_treatment\",\"flexible_dates_options_extend_one_three_seven_days\",\"super_date_flexibility\",\"micro_flex_improvements\",\"micro_flex_show_by_default\",\"search_input_placeholder_phrases\",\"pets_fee_treatment\"],\"screenSize\":\"large\",\"isInitialLoad\":true,\"hasLoggedIn\":false},\"staysSearchRequest\":{},\"staysSearchM2Enabled\":false}&extensions={\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"bfdf3079592d93f5a1419b160537ae0a531adb42cf362e6c7c6f51cc55d24aa6\"}}";
                var url = "https://www.airbnb.com/api/v3/ExploreSections?operationName=ExploreSections&locale=en&currency=USD&variables={\"isInitialLoad\":true,\"hasLoggedIn\":false,\"cdnCacheSafe\":false,\"source\":\"EXPLORE\",\"exploreRequest\":{\"metadataOnly\":false,\"version\":\"1.8.3\",\"itemsPerGrid\":20,\"placeId\":\"" + placeId + "\",\"refinementPaths\":[\"/homes\"],\"tabId\":\"home_tab\",\"datePickerType\":\"flexible_dates\",\"flexibleTripDates\":[\"april\",\"august\",\"december\",\"february\",\"january\",\"july\",\"june\",\"march\",\"may\",\"november\",\"october\",\"september\"],\"source\":\"structured_search_input_header\",\"searchType\":\"filter_change\",\"priceMin\":" + min + ",\"priceMax\":" + max + ",\"flexibleTripLengths\":[\"one_week\"],\"federatedSearchSessionId\":\"\",\"itemsOffset\":" + index + ",\"sectionOffset\":1,\"query\":\"\",\"cdnCacheSafe\":false,\"treatmentFlags\":[\"flex_destinations_june_2021_launch_web_treatment\",\"new_filter_bar_v2_fm_header\",\"merch_header_breakpoint_expansion_web\",\"flexible_dates_12_month_lead_time\",\"storefronts_nov23_2021_homepage_web_treatment\",\"lazy_load_flex_search_map_compact\",\"lazy_load_flex_search_map_wide\",\"im_flexible_may_2022_treatment\",\"im_flexible_may_2022_treatment\",\"flex_v2_review_counts_treatment\",\"flexible_dates_options_extend_one_three_seven_days\",\"super_date_flexibility\",\"micro_flex_improvements\",\"micro_flex_show_by_default\",\"search_input_placeholder_phrases\",\"pets_fee_treatment\"],\"screenSize\":\"large\",\"isInitialLoad\":true,\"hasLoggedIn\":false}}&extensions={\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"8d917c046a71eb9ec72e1f1509e3d680c4a20215b189b521739033a125f5e3d8\"}}";
                Notifier.Display($"Working on ({min} - {max}) page {page}");
                page++;
                // Notifier.Log(url);
                // await Task.Delay(1000);
                var json = await _client.GetHtml(url);
                JObject obj;
                try
                {
                    obj = JObject.Parse(json);
                }
                catch (Exception e)
                {
                    File.WriteAllText($"lastJson", json);
                    throw new KnownException($"Failed to parse json");
                }

                var itemsCount = (int)obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_NUMBERED_PAGINATION')].section.totalInventoryCount");
                var maxItemsCount = (int)obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_NUMBERED_PAGINATION')].section.maxTotalInventoryCount");
                var itemsJson = obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_SECTION_WRAPPER')].section.child.section.items")?.ToString();
                if (itemsJson == null)
                    break;
                var items = JsonConvert.DeserializeObject<List<SearchResult>>(itemsJson);
                foreach (var searchResult in items)
                    allItems.Add(searchResult.listing.id);
                if (allItems.Count >= itemsCount || allItems.Count >= 300)
                    break;
                index += 20;
            } while (true);

            return allItems.ToList();
        }

        async Task<List<SearchResult>> SearchPage(string url)
        {
            var json = await _client.GetHtml(url);
            var obj = JObject.Parse(json);
            var itemsCount = (int)obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_NUMBERED_PAGINATION')].section.totalInventoryCount");
            var itemsJson = obj.SelectToken("data.presentation.explore.sections.sections[?(@.sectionComponentType == 'EXPLORE_SECTION_WRAPPER')].section.child.section.items").ToString();
            var items = JsonConvert.DeserializeObject<List<SearchResult>>(itemsJson);
            return items;
        }

        async Task<int> PriceRanges(string placeId, int min, int max)
        {
            var url = "https://www.airbnb.com/api/v3/ExploreSections?operationName=ExploreSections&locale=en&currency=USD&variables={\"isInitialLoad\":true,\"hasLoggedIn\":false,\"cdnCacheSafe\":false,\"source\":\"EXPLORE\",\"exploreRequest\":{\"metadataOnly\":true,\"version\":\"1.8.3\",\"itemsPerGrid\":20,\"placeId\":\"" + placeId + "\",\"refinementPaths\":[\"/homes\"],\"tabId\":\"home_tab\",\"datePickerType\":\"flexible_dates\",\"flexibleTripDates\":[\"april\",\"august\",\"december\",\"february\",\"january\",\"july\",\"june\",\"march\",\"may\",\"november\",\"october\",\"september\"],\"source\":\"structured_search_input_header\",\"searchType\":\"filter_change\",\"priceMax\":" + max + ",\"priceMin\":" + min + ",\"flexibleTripLengths\":[\"one_week\"],\"query\":\"\",\"cdnCacheSafe\":false,\"treatmentFlags\":[\"flex_destinations_june_2021_launch_web_treatment\",\"new_filter_bar_v2_fm_header\",\"merch_header_breakpoint_expansion_web\",\"flexible_dates_12_month_lead_time\",\"storefronts_nov23_2021_homepage_web_treatment\",\"lazy_load_flex_search_map_compact\",\"lazy_load_flex_search_map_wide\",\"im_flexible_may_2022_treatment\",\"im_flexible_may_2022_treatment\",\"flex_v2_review_counts_treatment\",\"flexible_dates_options_extend_one_three_seven_days\",\"super_date_flexibility\",\"micro_flex_improvements\",\"micro_flex_show_by_default\",\"search_input_placeholder_phrases\",\"pets_fee_treatment\"],\"screenSize\":\"large\",\"isInitialLoad\":true,\"hasLoggedIn\":false,\"location\":\"\",\"federatedSearchSessionId\":\"\"},\"gpRequest\":{\"expectedResponseType\":\"INCREMENTAL\"}}&extensions={\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"8d917c046a71eb9ec72e1f1509e3d680c4a20215b189b521739033a125f5e3d8\"}}";
            //var json = await _client.GetHtml("https://www.airbnb.com/api/v3/StaysSearch?operationName=StaysSearch&locale=en&currency=USD&variables={\"isInitialLoad\":true,\"hasLoggedIn\":false,\"cdnCacheSafe\":false,\"source\":\"EXPLORE\",\"exploreRequest\":{\"metadataOnly\":true,\"version\":\"1.8.3\",\"itemsPerGrid\":20,\"tabId\":\"home_tab\",\"refinementPaths\":[\"/homes\"],\"placeId\":\"" + placeId + "\",\"datePickerType\":\"flexible_dates\",\"checkin\":\"2022-09-01\",\"checkout\":\"2022-09-03\",\"source\":\"structured_search_input_header\",\"searchType\":\"filter_change\",\"flexibleTripDates\":[\"april\",\"august\",\"december\",\"february\",\"january\",\"july\",\"june\",\"march\",\"may\",\"november\",\"october\",\"september\"],\"flexibleTripLengths\":[\"one_week\"],\"query\":\"\",\"cdnCacheSafe\":false,\"treatmentFlags\":[\"flex_destinations_june_2021_launch_web_treatment\",\"new_filter_bar_v2_fm_header\",\"new_filter_bar_v2_and_fm_treatment\",\"merch_header_breakpoint_expansion_web\",\"flexible_dates_12_month_lead_time\",\"storefronts_nov23_2021_homepage_web_treatment\",\"lazy_load_flex_search_map_compact\",\"lazy_load_flex_search_map_wide\",\"im_flexible_may_2022_treatment\",\"im_flexible_may_2022_treatment\",\"flex_v2_review_counts_treatment\",\"flexible_dates_options_extend_one_three_seven_days\",\"super_date_flexibility\",\"micro_flex_improvements\",\"micro_flex_show_by_default\",\"search_input_placeholder_phrases\",\"pets_fee_treatment\"],\"screenSize\":\"large\",\"isInitialLoad\":true,\"hasLoggedIn\":false,\"location\":\"\",\"priceMin\":" + min + ",\"priceMax\":" + max + ",\"federatedSearchSessionId\":\"\"},\"staysSearchRequest\":{},\"staysSearchM2Enabled\":false,\"gpRequest\":{\"expectedResponseType\":\"INCREMENTAL\"}}&extensions={\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"bfdf3079592d93f5a1419b160537ae0a531adb42cf362e6c7c6f51cc55d24aa6\"}}");
            var json = await _client.GetHtml(url);
            var obj = JObject.Parse(json);
            var result = (string)obj.SelectToken("data.presentation.explore.sections.responseTransforms.transformData[0].sectionContainer.section.primaryAction.title");
            var y = _numberOnly.Replace(result, "");
            try
            {
                var c = int.Parse(y);
                return c;
            }
            catch (Exception)
            {
                throw new KnownException($"Failed to parse : {y}");
            }
        }

        async Task<List<PriceRange>> CreateFilters(string placeId)
        {
            var min = 10;
            var max = 1000;
            var lastMax = 1000;
            var ranges = new List<PriceRange>();
            var coef = 100;
            var sw = new Stopwatch();
            sw.Start();
            do
            {
                var t = await PriceRanges(placeId, min, max);
                Debug.WriteLine($"Min : {min}, Max : {max} => {t} , lastMax : {lastMax} , Coef : {coef}");
                if (t >= 250 && t <= 300 || min == max || coef == 0 || max == 1000 && t < 300)
                {
                    ranges.Add(new PriceRange() { Max = max, Min = min, Results = t });
                    if (max == 1000) break;
                    min = max + 1;
                    max = 1000;
                    coef = 100;
                }
                else if (t > 300)
                {
                    lastMax = max;
                    if (max - coef < min)
                        coef /= 10;
                    if (max - coef < min)
                        coef /= 10;
                    max -= coef;
                }
                else
                {
                    if (max + coef >= lastMax)
                        coef /= 10;
                    max += coef;
                }
            } while (true);

            Notifier.Log($"found {ranges.Count} price ranges, total : ({ranges.Sum(x => x.Results)}) in {sw.Elapsed.TotalSeconds:#0.00}");
            return ranges;
        }

        public List<string> CreateSearchUrls(string placeId, List<PriceRange> priceRanges)
        {
            var urls = new List<string>();
            foreach (var priceRange in priceRanges)
            {
                var pages = priceRange.Results / 20;
                if (pages % 20 == 0) pages++;
                for (int i = 0; i < pages; i++)
                {
                    var url = "https://www.airbnb.com/api/v3/ExploreSections?operationName=ExploreSections&locale=en&currency=USD&variables={\"isInitialLoad\":true,\"hasLoggedIn\":false,\"cdnCacheSafe\":false,\"source\":\"EXPLORE\",\"exploreRequest\":{\"metadataOnly\":false,\"version\":\"1.8.3\",\"itemsPerGrid\":20,\"placeId\":\"" + placeId + "\",\"refinementPaths\":[\"/homes\"],\"tabId\":\"home_tab\",\"datePickerType\":\"flexible_dates\",\"flexibleTripDates\":[\"april\",\"august\",\"december\",\"february\",\"january\",\"july\",\"june\",\"march\",\"may\",\"november\",\"october\",\"september\"],\"source\":\"structured_search_input_header\",\"searchType\":\"filter_change\",\"priceMin\":" + priceRange.Min + ",\"priceMax\":" + priceRange.Max + ",\"flexibleTripLengths\":[\"one_week\"],\"federatedSearchSessionId\":\"\",\"itemsOffset\":" + (i * 20) + ",\"sectionOffset\":1,\"query\":\"\",\"cdnCacheSafe\":false,\"treatmentFlags\":[\"flex_destinations_june_2021_launch_web_treatment\",\"new_filter_bar_v2_fm_header\",\"merch_header_breakpoint_expansion_web\",\"flexible_dates_12_month_lead_time\",\"storefronts_nov23_2021_homepage_web_treatment\",\"lazy_load_flex_search_map_compact\",\"lazy_load_flex_search_map_wide\",\"im_flexible_may_2022_treatment\",\"im_flexible_may_2022_treatment\",\"flex_v2_review_counts_treatment\",\"flexible_dates_options_extend_one_three_seven_days\",\"super_date_flexibility\",\"micro_flex_improvements\",\"micro_flex_show_by_default\",\"search_input_placeholder_phrases\",\"pets_fee_treatment\"],\"screenSize\":\"large\",\"isInitialLoad\":true,\"hasLoggedIn\":false}}&extensions={\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"8d917c046a71eb9ec72e1f1509e3d680c4a20215b189b521739033a125f5e3d8\"}}";
                    urls.Add(url);
                }
            }

            return urls;
        }

        async Task<Property> GetDetails(string listingId)
        {
            var id = $"StayListing:{listingId}".Base64Encode();
            var json = await _client.GetHtml($"https://www.airbnb.com/api/v3/StaysPdpSections?operationName=StaysPdpSections&locale=en&currency=USD&variables={{\"id\":\"{id}\",\"pdpSectionsRequest\":{{\"adults\":\"1\",\"bypassTargetings\":false,\"categoryTag\":null,\"causeId\":null,\"children\":null,\"disasterId\":null,\"discountedGuestFeeVersion\":null,\"displayExtensions\":null,\"federatedSearchId\":null,\"forceBoostPriorityMessageType\":null,\"infants\":null,\"interactionType\":null,\"layouts\":[\"SIDEBAR\",\"SINGLE_COLUMN\"],\"pets\":0,\"pdpTypeOverride\":null,\"preview\":false,\"previousStateCheckIn\":null,\"previousStateCheckOut\":null,\"priceDropSource\":null,\"privateBooking\":false,\"promotionUuid\":null,\"relaxedAmenityIds\":null,\"searchId\":null,\"selectedCancellationPolicyId\":null,\"selectedRatePlanId\":null,\"splitStays\":null,\"staysBookingMigrationEnabled\":false,\"translateUgc\":null,\"useNewSectionWrapperApi\":false,\"sectionIds\":null,\"checkIn\":null,\"checkOut\":null}}}}&extensions={{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"1b84beaefb598f1c33e59d02457e55b4656afc27e3aac5bdfabbc5c173561130\"}}}}");
            var obj = JObject.Parse(json);
            var title = (string)obj.SelectToken("$..sections[?(@.sectionId=='AVAILABILITY_CALENDAR_DEFAULT')].section.listingTitle");
            var rating = (decimal?)obj.SelectToken("$..sections[?(@.sectionId=='REVIEWS_DEFAULT')].section.overallRating");
            // var price = (decimal)obj.SelectToken("$..sections[?(@.sectionId=='BOOK_IT_SIDEBAR')].section.structuredDisplayPrice.primaryLine.price");
            var reviewsCount = (int?)obj.SelectToken("$..sections[?(@.sectionId=='REVIEWS_DEFAULT')].section.overallCount");
            var locationNode = obj.SelectToken("$..sections[?(@.sectionId=='LOCATION_DEFAULT')].section");
            if(locationNode==null) 
                throw new KnownException("null location " + listingId);
            string location=null;
                location= (string)locationNode.SelectToken("subtitle");//['seeAllLocationDetails'][0]['title']
            var lat = (decimal)locationNode.SelectToken("['lat']");
            var lng = (decimal)locationNode.SelectToken("['lng']");
            var roomType = (string)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['roomType']");
            var isSuperHost = (bool)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['isSuperhost']");
            var locationRating = (decimal?)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['locationRating']");
            var checkingRating = (decimal?)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['checkinRating']");
            var accuracyRating = (decimal?)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['accuracyRating']");
            var cleanlinessRating = (decimal?)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['cleanlinessRating']");
            var valueRating = (decimal?)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['valueRating']");
            var communicationRating = (decimal?)obj.SelectToken("['data']['presentation']['stayProductDetailPage']['sections']['metadata']['loggingContext']['eventDataLogging']['communicationRating']");
            var detailsNodes = obj.SelectToken("$..sections[?(@.sectionId=='OVERVIEW_DEFAULT')].section.detailItems").Select(x => (string)x.SelectToken("title")).ToList();
            string guests = null;
            string bedrooms = null;
            string beds = null;
            string baths = null;
            foreach (var detailsNode in detailsNodes)
            {
                if (detailsNode.Contains("guest"))
                    guests = detailsNode;
                else if (detailsNode.Contains("bedroom"))
                    bedrooms = detailsNode;
                else if (detailsNode.Contains("bed"))
                    beds = detailsNode;
                else if (detailsNode.Contains("bath"))
                    baths = detailsNode;
            }
             // guests = detailsNodes[0];
             // bedrooms = detailsNodes[1];
             // beds = detailsNodes[2];
             // baths = detailsNodes[3];
            var desc = (string)obj.SelectToken("$..sections[?(@.sectionId=='DESCRIPTION_DEFAULT')].section.htmlDescription.htmlText");
            var imgs = obj.SelectToken("$..sections[?(@.sectionId=='PHOTO_TOUR_SCROLLABLE_MODAL')].section.mediaItems").Select(x => (string)x.SelectToken("baseUrl")).ToList();
            var ammenities = obj.SelectToken("$..seeAllAmenitiesGroups").SelectMany(x => x.SelectToken("amenities")).Select(x => (string)x.SelectToken("title")).ToList();
            return new Property()
            {
                ListingId = listingId,
                Title = title,
                Address = location,
                RoomType = roomType,
                Amenities = string.Join("\n", ammenities),
                Baths = baths,
                Bedrooms = bedrooms,
                Beds = beds,
                Desc = desc,
                Guests = guests,
                Images = string.Join("\n", imgs),
                Lat = lat,
                Lng = lng,
                //Price = price,
                Rating = rating,
                AccuracyRating = accuracyRating,
                CheckingRating = checkingRating,
                CleanlinessRating = cleanlinessRating,
                CommunicationRating = communicationRating,
                LocationRating = locationRating,
                ValueRating = valueRating,
                ReviewsCount = reviewsCount,
                IsSuperHost = isSuperHost
            };
        }

        async Task<Night> GetPrice(string listingId, Night night)
        {
            var id = $"StayListing:{listingId}".Base64Encode();

            var json = await _client.PostJson("https://www.airbnb.com/api/v3/startStaysCheckout?operationName=startStaysCheckout&locale=en&currency=USD", $"{{\"operationName\":\"startStaysCheckout\",\"variables\":{{\"input\":{{\"businessTravel\":{{\"workTrip\":false}},\"checkinDate\":\"{night.DateTime:yyy-MM-dd}\",\"checkoutDate\":\"{night.DateTime.AddDays(1):yyy-MM-dd}\",\"guestCounts\":{{\"numberOfAdults\":1,\"numberOfChildren\":0,\"numberOfInfants\":0,\"numberOfPets\":0}},\"guestCurrencyOverride\":\"USD\",\"lux\":{{}},\"metadata\":{{\"internalFlags\":[\"LAUNCH_LOGIN_PHONE_AUTH\"]}},\"org\":{{}},\"productId\":\"{id}\",\"china\":{{}},\"quickPayData\":null}}}},\"extensions\":{{\"persistedQuery\":{{\"version\":1,\"sha256Hash\":\"520e02672faf212c7cf4612fd660ca2ef2b5532a47ef027a3e87c080df0b1985\"}}}}}}", 1, new Dictionary<string, string>()
            {
                { "Cookie", "" },
                { "Referer", "https://www.airbnb.com/" }
            });
            var obj = JObject.Parse(json);
            var bootstrapPaymentsJSON = (string)obj.SelectToken("$..bootstrapPaymentsJSON");
            if (bootstrapPaymentsJSON == null)
                return night;
            var pr = JObject.Parse(bootstrapPaymentsJSON);
            var price = (string)pr.SelectToken("$..priceBreakdown.priceItems[0].total.amountFormatted");
            night.Price = price;
            return night;
        }

        async Task<List<Night>> GetAvailability(string listingId)
        {
            var availableDays = new List<Night>();
            var url = "https://www.airbnb.com/api/v3/PdpAvailabilityCalendar?operationName=PdpAvailabilityCalendar&locale=en&currency=USD&variables={\"request\":{\"count\":12,\"listingId\":\"" + listingId + "\",\"month\":9,\"year\":2022}}&extensions={\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"8f08e03c7bd16fcad3c92a3592c19a8b559a0d0855a84028d1163d4733ed9ade\"}}";
            var json = await _client.GetHtml(url);
            var obj = JObject.Parse(json);
            var dayNodes = obj.SelectToken("$..calendarMonths").SelectMany(x => x.SelectToken("days"));
            foreach (var dayNode in dayNodes)
            {
                var bookable = (bool?)dayNode.SelectToken("bookable");
                if (bookable.GetValueOrDefault())
                    availableDays.Add(new Night() { DateTime = DateTime.Parse((string)dayNode.SelectToken("calendarDate")) });
            }

            return availableDays;
        }

        public async Task MainWork(CancellationToken ct)
        {
            Notifier.Display("Started working");


           // await GetDetails("686466905719532268");
            // var nights = await GetAvailability("29876738");
            // nights = await nights.Parallel(20, (x) => GetPrice("29876738", x),false);
            // nights.Save();
            //var t= await GetDetails("29876738");

            //var t = File.ReadAllLines("ids").ToHashSet().Count;
            //await Search("");
            // var locationId= await Autocomplete("Tunis");
            
            
            //  var locationId = await Autocomplete("Colorado Springs, CO");
            // // // var priceRanges = await CreateFilters(locationId);
            // // // priceRanges.Save();
            // var priceRanges = nameof(PriceRange).Load<PriceRange>();
            // Notifier.Log(JsonConvert.SerializeObject(priceRanges, Formatting.Indented));
            //
            // var urls = CreateSearchUrls(locationId, priceRanges);
            // var listingsIds = await urls.Parallel(_threads, Search, false);
            // File.WriteAllLines("ids", listingsIds);
            // var results = await listingsIds.Parallel(_threads, GetDetails, false);
            // results.Save();
            
            
             var results = nameof(Property).Load<Property>();
             Notifier.Log($"Start getting nights rate");
             for (var i = 0; i < 10; i++)
             {
                 Notifier.Log($"{i+1}/{results.Count}");
                 var property = results[i];
                 var nights = await GetAvailability(property.ListingId);
                 nights = await nights.Parallel(_threads, (x) => GetPrice(property.ListingId, x), false);
                 property.Availability = string.Join("\n", nights.Select(x => $"{x.DateTime:MM/dd/yyyy} : {x.Price}"));
             }
            
             results.Save();
            await results.SaveToExcel("outputSample.xlsx");
            Notifier.Display("Completed working");
        }
    }
}