using MagicVilla_VillaAPI.Models;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IVillaRepository
    {
        //actually automapping proccess is done out of repository, inside service or controller's action method 
        //data layer will interact directly with the database entity itself, it means we don't pass dto or any business level items to repository
        Task<List<Villa>> GetAllAsync(Expression<Func<Villa,bool>>? filter =null);
        Task<Villa> GetAsync(Expression<Func<Villa, bool>>? filter =null, bool tracked=true);
        Task CreateAsync(Villa villa);
        Task RemoveAsync(Villa villa);
        Task UpdateAsync(Villa villa);
        Task SaveAsync();
    }
}
