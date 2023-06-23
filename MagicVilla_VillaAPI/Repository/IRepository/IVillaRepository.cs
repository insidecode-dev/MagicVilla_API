using MagicT_TAPI.Repository.IRepository;
using MagicVilla_VillaAPI.Models;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IVillaRepository:IRepository<Villa>
    {
        //actually automapping proccess is done out of repository, inside service or controller's action method 
        //data layer will interact directly with the database entity itself, it means we don't pass dto or any business level items to repository
        
        //in real world project, Update is not same for all entities, that's why it's written for each repository respectly
        Task<Villa> UpdateAsync(Villa villa);
        
    }
}
