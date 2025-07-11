using CineHub.Models;

namespace CineHub.Services.Ranking
{
    public class MovieRankingService : IRankingService
    {
        // Calculates the Wilson Score for movie ranking
        // Considers both average rating and number of votes for a more reliable ranking
        public double CalculateWilsonScore(double rating, int voteCount, double confidence = 0.95)
        {
            if (voteCount == 0) return 0;

            // Normalize rating to 0-1 scale (TMDb uses 0-10)
            double p = rating / 10.0;
            double n = voteCount;

            // Z-score for the confidence level
            double z = confidence switch
            {
                0.90 => 1.645,
                0.95 => 1.96,
                0.99 => 2.576,
                _ => 1.96
            };

            // Wilson Score formula
            double denominator = 1 + (z * z) / n;
            double centre_adjusted_probability = p + (z * z) / (2 * n);
            double adjusted_standard_deviation = Math.Sqrt((p * (1 - p) + (z * z) / (4 * n)) / n);

            double lower_bound = (centre_adjusted_probability - z * adjusted_standard_deviation) / denominator;

            return Math.Max(0, lower_bound);
        }

        // Applies Wilson Score ranking to a list of movies
        // For Top Rated, filtered searches, general rankings
        public List<Movie> ApplyWilsonScoreRanking(List<Movie> movies, double confidence = 0.95)
        {
            return movies
                .Select(m => new
                {
                    Movie = m,
                    WilsonScore = CalculateWilsonScore(m.VoteAverage, m.VoteCount, confidence)
                })
                .OrderByDescending(x => x.WilsonScore)
                .ThenByDescending(x => x.Movie.VoteAverage)
                .ThenByDescending(x => x.Movie.VoteCount)
                .Select(x => x.Movie)
                .ToList();
        }

        // Calculates a custom popularity score
        // Considers: total votes, average rating, and movie recency
        public double CalculatePopularityScore(Movie movie)
        {
            if (movie.VoteCount == 0) return 0;

            // Components of the score
            double ratingComponent = movie.VoteAverage / 10.0; // 0-1
            double voteComponent = Math.Log10(Math.Max(1, movie.VoteCount)) / 6.0; // Log scale, max ~6

            // Recency component (newer movies get a boost)
            double recencyComponent = 0.5; // Default for movies without a release date
            if (movie.ReleaseDate.HasValue)
            {
                var yearsOld = DateTime.Now.Year - movie.ReleaseDate.Value.Year;
                recencyComponent = Math.Max(0.1, 1.0 - (yearsOld * 0.02)); // Decays 2% per year
            }

            // Combined formula with weights
            return (ratingComponent * 0.4) + (voteComponent * 0.4) + (recencyComponent * 0.2);
        }

        // Current popularity ranking
        // "Popular" section, homepage, discover movies
        public List<Movie> ApplyPopularityRanking(List<Movie> movies)
        {
            return movies
                .Select(m => new
                {
                    Movie = m,
                    PopularityScore = CalculatePopularityScore(m)
                })
                .OrderByDescending(x => x.PopularityScore)
                .ThenByDescending(x => x.Movie.VoteCount)
                .Select(x => x.Movie)
                .ToList();
        }

        // Strict ranking for Top Rated
        // Requires minimum number of votes and uses a more conservative Wilson Score
        public List<Movie> ApplyTopRatedRanking(List<Movie> movies, int minVotes = 100)
        {
            return movies
                .Where(m => m.VoteCount >= minVotes && m.VoteAverage >= 7.0)
                .Select(m => new
                {
                    Movie = m,
                    WilsonScore = CalculateWilsonScore(m.VoteAverage, m.VoteCount, 0.99) // More conservative
                })
                .OrderByDescending(x => x.WilsonScore)
                .ThenByDescending(x => x.Movie.VoteAverage)
                .ThenByDescending(x => x.Movie.VoteCount)
                .Select(x => x.Movie)
                .ToList();
        }

        // Search relevance ranking
        // Considers matches in title, overview and combines with quality
        public List<Movie> ApplySearchRelevanceRanking(List<Movie> movies, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return ApplyWilsonScoreRanking(movies);

            var query = searchQuery.ToLower();

            return movies
                .Select(m => new
                {
                    Movie = m,
                    RelevanceScore = CalculateSearchRelevance(m, query),
                    QualityScore = CalculateWilsonScore(m.VoteAverage, m.VoteCount)
                })
                .OrderByDescending(x => x.RelevanceScore)
                .ThenByDescending(x => x.QualityScore)
                .Select(x => x.Movie)
                .ToList();
        }

        // Recency ranking combined with quality
        // Newer movies get a boost but quality is still considered
        public List<Movie> ApplyRecencyRanking(List<Movie> movies)
        {
            return movies
                .Select(m => new
                {
                    Movie = m,
                    RecencyScore = CalculateRecencyScore(m),
                    QualityScore = CalculateWilsonScore(m.VoteAverage, m.VoteCount)
                })
                .OrderByDescending(x => x.RecencyScore + (x.QualityScore * 0.3)) // Combines recency with quality
                .Select(x => x.Movie)
                .ToList();
        }

        // Calculates search relevance score
        private double CalculateSearchRelevance(Movie movie, string query)
        {
            double score = 0;

            // Exact match in title = max score
            if (movie.Title.ToLower() == query)
                score += 100;
            else if (movie.Title.ToLower().StartsWith(query))
                score += 50;
            else if (movie.Title.ToLower().Contains(query))
                score += 25;

            // Match in overview
            if (!string.IsNullOrEmpty(movie.Overview) && movie.Overview.ToLower().Contains(query))
                score += 10;

            // Boost by quality (better movies appear first among relevant ones)
            score += movie.VoteAverage;

            return score;
        }

        // Calculates recency score
        private double CalculateRecencyScore(Movie movie)
        {
            if (!movie.ReleaseDate.HasValue)
                return 0;

            var now = DateTime.Now;
            var releaseDate = movie.ReleaseDate.Value;

            // Future or very old movies have lower score
            if (releaseDate > now)
                return 0.1;

            var daysSinceRelease = (now - releaseDate).TotalDays;

            // Score decays exponentially over time
            return Math.Max(0.1, Math.Exp(-daysSinceRelease / 365.0)); // Half-life of ~1 year
        }
    }
}