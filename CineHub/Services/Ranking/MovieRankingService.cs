using CineHub.Models;

namespace CineHub.Services.Ranking
{
    public class MovieRankingService : IRankingService
    {
        /// Calcula o Wilson Score para ranking de filmes
        /// Considera tanto a nota média quanto o número de votos para um ranking mais confiável
        public double CalculateWilsonScore(double rating, int voteCount, double confidence = 0.95)
        {
            if (voteCount == 0) return 0;

            // Normaliza a avaliação para escala 0-1 (TMDb usa 0-10)
            double p = rating / 10.0;
            double n = voteCount;

            // Z-score para o nível de confiança
            double z = confidence switch
            {
                0.90 => 1.645,
                0.95 => 1.96,
                0.99 => 2.576,
                _ => 1.96
            };

            // Fórmula do Wilson Score
            double denominator = 1 + (z * z) / n;
            double centre_adjusted_probability = p + (z * z) / (2 * n);
            double adjusted_standard_deviation = Math.Sqrt((p * (1 - p) + (z * z) / (4 * n)) / n);

            double lower_bound = (centre_adjusted_probability - z * adjusted_standard_deviation) / denominator;

            return Math.Max(0, lower_bound);
        }

        /// Aplica Wilson Score a uma lista de filmes
        /// Top Rated, Pesquisas com filtros, Rankings gerais
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

        /// Calcula score de popularidade personalizado
        /// Considera: votos totais, nota média, recência do filme
        public double CalculatePopularityScore(Movie movie)
        {
            if (movie.VoteCount == 0) return 0;

            // Componentes do score
            double ratingComponent = movie.VoteAverage / 10.0; // 0-1
            double voteComponent = Math.Log10(Math.Max(1, movie.VoteCount)) / 6.0; // Log scale, max ~6

            // Componente de recência (filmes mais novos têm boost)
            double recencyComponent = 0.5; // Default para filmes sem data
            if (movie.ReleaseDate.HasValue)
            {
                var yearsOld = DateTime.Now.Year - movie.ReleaseDate.Value.Year;
                recencyComponent = Math.Max(0.1, 1.0 - (yearsOld * 0.02)); // Decai 2% por ano
            }

            // Fórmula combinada com pesos
            return (ratingComponent * 0.4) + (voteComponent * 0.4) + (recencyComponent * 0.2);
        }

        /// Ranking por popularidade atual
        ///Seção "Popular", Homepage, Descobrir filmes
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

        /// Ranking rigoroso para Top Rated
        /// Requer número mínimo de votos e usa Wilson Score mais conservador
        public List<Movie> ApplyTopRatedRanking(List<Movie> movies, int minVotes = 100)
        {
            return movies
                .Where(m => m.VoteCount >= minVotes && m.VoteAverage >= 7.0)
                .Select(m => new
                {
                    Movie = m,
                    WilsonScore = CalculateWilsonScore(m.VoteAverage, m.VoteCount, 0.99) // Mais conservador
                })
                .OrderByDescending(x => x.WilsonScore)
                .ThenByDescending(x => x.Movie.VoteAverage)
                .ThenByDescending(x => x.Movie.VoteCount)
                .Select(x => x.Movie)
                .ToList();
        }

        /// Ranking por relevância de pesquisa
        /// Considera correspondência no título, overview e combina com qualidade
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

        /// Ranking por recência combinado com qualidade
        /// Filmes mais novos ganham boost, mas ainda considera qualidade
        public List<Movie> ApplyRecencyRanking(List<Movie> movies)
        {
            return movies
                .Select(m => new
                {
                    Movie = m,
                    RecencyScore = CalculateRecencyScore(m),
                    QualityScore = CalculateWilsonScore(m.VoteAverage, m.VoteCount)
                })
                .OrderByDescending(x => x.RecencyScore + (x.QualityScore * 0.3)) // Combina recência com qualidade
                .Select(x => x.Movie)
                .ToList();
        }

        /// Calcula relevância para pesquisa
        private double CalculateSearchRelevance(Movie movie, string query)
        {
            double score = 0;

            // Correspondência exata no título = máximo score
            if (movie.Title.ToLower() == query)
                score += 100;
            else if (movie.Title.ToLower().StartsWith(query))
                score += 50;
            else if (movie.Title.ToLower().Contains(query))
                score += 25;

            // Correspondência no overview
            if (!string.IsNullOrEmpty(movie.Overview) && movie.Overview.ToLower().Contains(query))
                score += 10;

            // Boost por qualidade (filmes melhores aparecem primeiro entre os relevantes)
            score += movie.VoteAverage;

            return score;
        }

        /// Calcula score de recência
        private double CalculateRecencyScore(Movie movie)
        {
            if (!movie.ReleaseDate.HasValue)
                return 0;

            var now = DateTime.Now;
            var releaseDate = movie.ReleaseDate.Value;

            // Filmes futuros ou muito antigos têm score menor
            if (releaseDate > now)
                return 0.1;

            var daysSinceRelease = (now - releaseDate).TotalDays;

            // Score decai exponencialmente com o tempo
            return Math.Max(0.1, Math.Exp(-daysSinceRelease / 365.0)); // Meia-vida de ~1 ano
        }
    }
}