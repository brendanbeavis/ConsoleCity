namespace ConsoleCity.Services;

/// <summary>
/// Enumeration of specific service categories beyond the basic ServiceType.
/// Used to distinguish different facilities and their specializations.
/// </summary>
public enum ServiceCategory
{
    // Emergency services
    PoliceStation,
    FireStation,

    // Healthcare
    Clinic,
    Hospital,
    Specialist,

    // Education
    PrimarySchool,
    SecondarySchool,
    University,

    // Civic/Government
    CityHall,
    Library,
    CommunityCenter,

    // Recreation
    Park,
    RecreationCenter,

    // Retail
    Supermarket,
    Market,

    // Other
    Other
}
