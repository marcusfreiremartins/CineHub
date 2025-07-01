using CineHub.Models;

namespace CineHub.Services.Ranking
{
    public interface IRankingService
    {
        /// Applies Wilson Score for ranking based on statistical reliability
        List<Movie> ApplyWilsonScoreRanking(List<Movie> movies, double confidence = 0.95);

        /// Ranking based on current popularity (combining recent votes and trends)
        List<Movie> ApplyPopularityRanking(List<Movie> movies);

        /// Ranking for "Top Rated" with stricter criteria
        List<Movie> ApplyTopRatedRanking(List<Movie> movies, int minVotes = 100);

        /// Ranking by search relevance (considers title, overview, etc.)
        List<Movie> ApplySearchRelevanceRanking(List<Movie> movies, string searchQuery);

        /// Ranking by recency (newer movies first, but considering quality)
        List<Movie> ApplyRecencyRanking(List<Movie> movies);

        /// Calculates individual Wilson Score for a movie
        double CalculateWilsonScore(double rating, int voteCount, double confidence = 0.95);

        /// Calculates popularity score based on a custom algorithm
        double CalculatePopularityScore(Movie movie);
    }
}