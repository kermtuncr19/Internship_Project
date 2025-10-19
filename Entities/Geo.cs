using System.Text.Json.Serialization;

public record City(
    [property: JsonPropertyName("sehir_id")] string Id,
    [property: JsonPropertyName("sehir_adi")] string Name
);

public record District(
    [property: JsonPropertyName("ilce_id")] string Id,
    [property: JsonPropertyName("ilce_adi")] string Name,
    [property: JsonPropertyName("sehir_id")] string CityId,
    [property: JsonPropertyName("sehir_adi")] string CityName
);

public record Neighborhood(
    [property: JsonPropertyName("mahalle_id")] string Id,
    [property: JsonPropertyName("mahalle_adi")] string Name,
    [property: JsonPropertyName("ilce_id")] string DistrictId,
    [property: JsonPropertyName("ilce_adi")] string DistrictName,
    [property: JsonPropertyName("sehir_id")] string CityId,
    [property: JsonPropertyName("sehir_adi")] string CityName
);
