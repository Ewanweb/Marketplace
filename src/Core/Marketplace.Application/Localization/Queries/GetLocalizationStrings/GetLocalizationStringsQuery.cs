using System.Globalization;
using Marketplace.Shared.Results;
using MediatR;

namespace Marketplace.Application.Localization.Queries.GetLocalizationStrings;

public sealed record GetLocalizationStringsQuery() : IRequest<Result<Dictionary<string, string>>>;

public sealed class GetLocalizationStringsQueryHandler : IRequestHandler<GetLocalizationStringsQuery, Result<Dictionary<string, string>>>
{
    public Task<Result<Dictionary<string, string>>> Handle(GetLocalizationStringsQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();
        var isPashto = culture.StartsWith("ps");
        var isDari = culture.StartsWith("prs") || culture.StartsWith("fa");

        var strings = new Dictionary<string, string>
        {
            ["AppName"] = isPashto ? "د نورزي بازار" : (isDari ? "بازار نورزی" : "BuyMore"),
            ["PopularProducts"] = isPashto ? "مشهور توکي" : (isDari ? "محصولات محبوب" : "Popular Products"),
            ["ExploreNew"] = isPashto ? "نوې موندنې" : (isDari ? "کشف جدید" : "Explore New"),
            ["ClothingAndShoes"] = isPashto ? "جامې او بوټان" : (isDari ? "پوشاک و کفش" : "Clothing and Shoes"),
            ["GiftsAndLiving"] = isPashto ? "ډالۍ او د کور سامان" : (isDari ? "هدایا و لوازم منزل" : "Gifts and Living"),
            ["Inspiration"] = isPashto ? "الهام" : (isDari ? "الهام‌بخش" : "Inspiration"),
            ["QuickActions"] = isPashto ? "چټک اقدامات" : (isDari ? "اقدامات سریع" : "Quick actions"),
            ["RequestForProduct"] = isPashto ? "د توکي غوښتنه" : (isDari ? "درخواست محصول" : "Request for product"),
            ["AddMember"] = isPashto ? "عضو زیاتول" : (isDari ? "افزودن عضو" : "Add member"),
            ["LastOrders"] = isPashto ? "وروستي فرمایشونه" : (isDari ? "سفارشات اخیر" : "Last orders"),
            ["LogOut"] = isPashto ? "وتل" : (isDari ? "خروج" : "Log out"),
            ["GetUpTo50Off"] = isPashto ? "تر ۵۰٪ پورې تخفیف ترلاسه کړئ" : (isDari ? "تا ۵۰٪ تخفیف طلایی دریافت کنید" : "GET UP TO 50% OFF"),
            ["GetDiscount"] = isPashto ? "تخفیف ترلاسه کول" : (isDari ? "دریافت تخفیف" : "Get Discount"),
            ["WinterWeekend"] = isPashto ? "د ژمي رخصتۍ" : (isDari ? "تعطیلات زمستانی" : "Winter's weekend"),
            ["KeepItCasual"] = isPashto ? "کژوال او هوسا سټایل" : (isDari ? "استایل راحت و خاص" : "keep it casual"),
            ["OurPicks"] = isPashto ? "زموږ انتخاب" : (isDari ? "انتخاب ما" : "Our Picks"),
            ["YourChoice"] = isPashto ? "ستاسو انتخاب" : (isDari ? "انتخاب شما" : "Your Choice"),
            ["AvailOffers"] = isPashto ? "وړاندیزونه وګورئ" : (isDari ? "مشاهده پیشنهادات" : "Avail Offers"),
            ["Favourites"] = isPashto ? "غوره شوي" : (isDari ? "محبوب‌ترین‌ها" : "Favourites"),
            ["SeeAll"] = isPashto ? "ټول لیدل" : (isDari ? "مشاهده همه" : "See All"),
            ["BringBoldFashion"] = isPashto ? "په زړه پورې فیشن" : (isDari ? "مد و فیشن جسورانه" : "Bring Bold Fashion"),
            ["LayersOnLayers"] = isPashto ? "لایه په لایه سټایلونه" : (isDari ? "لایه در لایه و خاص" : "Layers on Layers"),
            ["Orders"] = isPashto ? "فرمایشونه" : (isDari ? "سفارشات" : "Orders"),
            ["Last7Days"] = isPashto ? "وروستۍ ۷ ورځې" : (isDari ? "۷ روز گذشته" : "Last 7 days"),
            ["Dashboard"] = isPashto ? "ډشبورډ" : (isDari ? "داشبورد" : "Dashboard"),
            ["Website"] = isPashto ? "ویب پاڼه" : (isDari ? "وب‌سایت" : "Website"),
            ["Cart"] = isPashto ? "سبد خرید" : (isDari ? "سبد خرید" : "Cart"),
            ["Explore"] = isPashto ? "کاوش" : (isDari ? "کاوش" : "Explore"),
            ["All"] = isPashto ? "ټول" : (isDari ? "همه" : "All"),
            ["Men"] = isPashto ? "سړي" : (isDari ? "مردانه" : "Men"),
            ["Women"] = isPashto ? "ښځې" : (isDari ? "زنانه" : "Women"),
            ["Filters"] = isPashto ? "فیلټرونه" : (isDari ? "فیلترها" : "Filters"),
            ["Search"] = isPashto ? "لټون" : (isDari ? "جستجو" : "Search")
        };

        return Task.FromResult(Result.Success(strings));
    }
}
