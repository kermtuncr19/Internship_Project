using Entities.Models;

namespace Services.Contracts
{
    public interface IProductQaService
    {
        Task CreateQuestionAsync(int productId, string userId, string questionText);
        Task<List<ProductQuestion>> GetAnsweredByProductAsync(int productId);
        Task<List<ProductQuestion>> GetPendingAsync();
        Task AnswerAsync(int questionId, string adminUserId, string answerText);
    }
}
