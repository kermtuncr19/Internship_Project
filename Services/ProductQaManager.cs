using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using Services.Contracts;

namespace StoreApp.Services
{
    public class ProductQaManager : IProductQaService
    {
        private readonly IRepositoryManager _repo;

        public ProductQaManager(IRepositoryManager repo)
        {
            _repo = repo;
        }

        public async Task CreateQuestionAsync(int productId, string userId, string questionText)
        {
            var text = (questionText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Soru boş olamaz.");

            if (text.Length > 1000)
                throw new Exception("Soru en fazla 1000 karakter olabilir.");

            var q = new ProductQuestion
            {
                ProductId = productId,
                UserId = userId,
                QuestionText = text,
                CreatedAt = DateTime.UtcNow
            };

            _repo.QuestionRepository.Create(q);
            _repo.Save();
            await Task.CompletedTask;
        }

        public Task<List<ProductQuestion>> GetAnsweredByProductAsync(int productId)
        {
            return _repo.QuestionRepository.Query(false)
                .Where(q => q.ProductId == productId)
                .Include(q => q.User)
                .Include(q => q.Answer)
                    .ThenInclude(a => a!.AdminUser)
                .Where(q => q.Answer != null)               // ✅ sadece cevaplılar
                .OrderByDescending(q => q.Answer!.CreatedAt)
                .ToListAsync();
        }

        public Task<List<ProductQuestion>> GetPendingAsync()
        {
            return _repo.QuestionRepository.Query(false)
                .Include(q => q.Product)
                .Include(q => q.User)
                .Include(q => q.Answer)
                .Where(q => q.Answer == null)              // ✅ bekleyen
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task AnswerAsync(int questionId, string adminUserId, string answerText)
        {
            var text = (answerText ?? "").Trim();
            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Cevap boş olamaz.");

            if (text.Length > 2000)
                throw new Exception("Cevap en fazla 2000 karakter olabilir.");

            var q = _repo.QuestionRepository.GetOne(questionId, trackChanges: false);
            if (q == null) throw new Exception("Soru bulunamadı.");

            var existing = _repo.AnswerRepository.GetByQuestionId(questionId, trackChanges: false);
            if (existing != null) throw new Exception("Bu soru zaten cevaplanmış.");

            var a = new ProductAnswer
            {
                ProductQuestionId = questionId,
                AdminUserId = adminUserId,
                AnswerText = text,
                CreatedAt = DateTime.UtcNow
            };

            _repo.AnswerRepository.Create(a);
            _repo.Save();
            await Task.CompletedTask;
        }
    }
}
