using HabitTrakerApi.DbContext;
using HabitTrakerApi.DTOs.Auth;
using HabitTrakerApi.Models.Data;
using HabitTrakerApi.Models.DTO;

namespace HabitTrakerApi.Repositories;

public interface IJwtServiceRepository
{
   User CheckUser(RegisterDto dto);
}


public class JwtServiceRepository : IJwtServiceRepository
{
   private readonly AppDbContext _db;

   public JwtServiceRepository( AppDbContext db)
   {
      _db = db;
   }
   
   public User? CheckUser(RegisterDto dto)
   {
      User? user;
     user = _db.Users.FirstOrDefault(u => u.Login == dto.Login);
     return user;
   }


}