using System.Text.RegularExpressions;

namespace Ledger.Infrastructure.Services;

/// <summary>
/// Pattern-based transaction categorizer. Matches description text against
/// known merchant/keyword patterns to assign a category automatically.
/// Returns "Uncategorized" when no pattern matches.
/// </summary>
public static class TransactionCategorizer
{
    private static readonly (Regex Pattern, string Category)[] Rules =
    [
        // Groceries
        (Regex(@"whole foods|trader joe|kroger|safeway|costco|walmart|target|aldi|publix|wegmans|sprouts|fresh market|food lion|giant food|stop & shop"), "Groceries"),
        // Dining
        (Regex(@"restaurant|mcdonald|starbucks|chipotle|subway|pizza|burger|cafe|coffee|sushi|taco|doordash|grubhub|uber eats|instacart|chick-fil|panera|domino|dunkin|wendy|kfc|popeyes"), "Dining"),
        // Transportation
        (Regex(@"uber|lyft|taxi|metro|mta|bart|caltrain|amtrak|greyhound|parking|toll|eztoll|sunpass|fastrak|gas station|shell|chevron|exxon|bp oil|mobil|speedway|marathon|circle k"), "Transportation"),
        // Gas
        (Regex(@"\bgas\b|gasoline|fuel|petrol"), "Gas"),
        // Travel
        (Regex(@"airline|delta|united|southwest|american air|jetblue|spirit air|frontier|alaska air|airbnb|vrbo|marriott|hilton|hyatt|holiday inn|expedia|booking\.com|kayak|hotel"), "Travel"),
        // Entertainment
        (Regex(@"netflix|spotify|hulu|disney\+|apple tv|amazon prime|youtube|hbo|paramount|peacock|crunchyroll|twitch|steam|playstation|xbox|nintendo|ticketmaster|eventbrite|cinema|theater|theatre|concert|museum"), "Entertainment"),
        // Subscriptions
        (Regex(@"subscription|monthly fee|annual fee|membership"), "Subscriptions"),
        // Shopping
        (Regex(@"amazon|ebay|etsy|wayfair|best buy|apple store|ikea|home depot|lowes|bed bath|tj maxx|marshalls|nordstrom|macy|gap|h&m|zara|uniqlo|old navy|nike|adidas"), "Shopping"),
        // Health & Medical
        (Regex(@"pharmacy|walgreens|cvs|rite aid|doctor|clinic|hospital|dental|optometrist|urgent care|lab corp|quest diagnostics|health insurance|blue cross|aetna|cigna|humana"), "Health"),
        // Fitness
        (Regex(@"gym|fitness|planet fitness|la fitness|equinox|anytime fitness|ymca|crossfit|peloton|soul cycle|yoga"), "Fitness"),
        // Utilities
        (Regex(@"electric|gas company|water bill|utility|pg&e|con ed|national grid|verizon|at&t|t-mobile|sprint|comcast|xfinity|spectrum|cox cable|dish network"), "Utilities"),
        // Internet & Phone
        (Regex(@"internet|broadband|wifi|phone bill|wireless|mobile plan"), "Phone & Internet"),
        // Insurance
        (Regex(@"insurance|geico|state farm|allstate|progressive|liberty mutual|usaa|travelers"), "Insurance"),
        // Income / Payroll
        (Regex(@"payroll|direct deposit|salary|paycheck|ach credit|employer|adp|paychex"), "Income"),
        // Transfers
        (Regex(@"transfer|zelle|venmo|paypal|cash app|wire transfer|ach transfer|inter-account"), "Transfer"),
        // ATM / Cash
        (Regex(@"atm|cash withdrawal|teller"), "Cash & ATM"),
        // Education
        (Regex(@"university|college|tuition|coursera|udemy|linkedin learning|skillshare|udacity|edx|student loan"), "Education"),
        // Investments
        (Regex(@"fidelity|vanguard|schwab|etrade|robinhood|coinbase|binance|dividend|investment"), "Investments"),
        // Rent / Mortgage
        (Regex(@"rent|mortgage|landlord|property management|hoa|homeowner"), "Housing"),
        // Fees & Charges
        (Regex(@"bank fee|overdraft|late fee|annual fee|service charge|maintenance fee|wire fee"), "Fees"),
    ];

    public static string Categorize(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "Uncategorized";

        string lower = description.ToLowerInvariant();

        foreach ((Regex pattern, string category) in Rules)
        {
            if (pattern.IsMatch(lower))
                return category;
        }

        return "Uncategorized";
    }

    private static Regex Regex(string pattern) =>
        new(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
}
