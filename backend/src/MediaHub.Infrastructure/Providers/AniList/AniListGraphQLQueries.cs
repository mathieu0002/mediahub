namespace MediaHub.Infrastructure.Providers.AniList;

internal static class AniListGraphQLQueries
{
    public const string Search = @"
        query ($search: String!, $type: MediaType!, $page: Int!, $perPage: Int!) {
          Page(page: $page, perPage: $perPage) {
            pageInfo { total currentPage hasNextPage }
            media(search: $search, type: $type, sort: POPULARITY_DESC) {
              id
              title { romaji english native }
              description(asHtml: false)
              coverImage { large color }
              bannerImage
              startDate { year }
              episodes
              chapters
              genres
              averageScore
              externalLinks { site url type }
            }
          }
        }";

    public const string GetById = @"
        query ($id: Int!) {
          Media(id: $id) {
            id
            title { romaji english native }
            description(asHtml: false)
            coverImage { large color }
            bannerImage
            startDate { year }
            episodes
            chapters
            genres
            averageScore
            externalLinks { site url type }
          }
        }";
}