using MagicT_TAPI.Repository.IRepository;
using MagicVilla_VillaAPI.Models;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IVillaNumberRepository:IRepository<VillaNumber>
    {
        public Task<VillaNumber> UpdateAsync(VillaNumber villaNumber);
    }
}
