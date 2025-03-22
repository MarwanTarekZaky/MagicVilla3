using System.Linq.Expressions;
using MagicVilla_VillaAPI.Models;

namespace MagicVilla_VillaAPI.Repository.IRepository;

public interface IVillaRepository
{
    
    public Task<List<Villa>> GetAllAsync(Expression<Func<Villa, bool>> filter = null);
    
    public Task<Villa> GetAsync(Expression<Func<Villa, bool>> filter = null, bool tracked = true);
    
    public Task  CreateAsync(Villa entity);
    
    public Task RemoveAsync(Villa entity);
    
    public Task SaveAsync();
    
    public Task UpdateAsync(Villa entity);
    
    
    
}