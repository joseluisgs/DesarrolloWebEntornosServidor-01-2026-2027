namespace _07_Refit.Dto;

public record User(
    int id,
    string name,
    string username,
    string email,
    Address address,
    string phone,
    string website,
    Company company
);

public record Address(
    string street,
    string suite,
    string city,
    string zipcode,
    Geo geo
);

public record Geo(
    string lat,
    string lng
);

public record Company(
    string name,
    string catchPhrase,
    string bs
);

