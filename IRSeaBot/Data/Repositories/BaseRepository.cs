using IRSeaBot.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Data.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class, IBotEntity
    {
        protected readonly ApplicationDbContext _context;
        protected DbSet<T> _set;
        
        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<T>();
        }

        public async Task<List<T>> GetSet()
        {
            try
            {
                return await _set.ToListAsync();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<T>();
            }          
        }

        public async Task<T> GetById(int id)
        {
            try
            {
                return await _set.FirstOrDefaultAsync(x => x.Id == id);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<T> Insert(T entity)
        {
            try
            {
                await _set.AddAsync(entity); 
                await _context.SaveChangesAsync();
                return entity;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return entity;
            }
        }

        public async Task<T> Edit(T entity)
        {
            try
            {
                T entityToUpdate = await GetById(entity.Id);
                entityToUpdate = entity ?? entityToUpdate;
                _set.Update(entityToUpdate);
                await _context.SaveChangesAsync();
                return entityToUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return entity;
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                T entity = await GetById(id);
                _context.Remove(entity);
                await _context.SaveChangesAsync();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
