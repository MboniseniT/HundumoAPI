
using BinmakAPI.Data;
using BinmakAPI.Entities;
using BinmakBackEnd.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Helpers
{
    public class Seed
    {
        private readonly BinmakDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public Seed(BinmakDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async void SeedRoles()
        {
            string[] roles = new string[] { "BINMAK", "ADMINISTRATOR", "USER", "GUEST" };

            foreach (string role in roles)
            {
                var r = _context.Roles.Where(u => u.Name == role).Any();

                if (r == false)
                {
                    _context.Roles.Add(new IdentityRole()
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
                    _context.SaveChanges();
                }

                //var roleStore = new RoleStore<IdentityRole>(_context);

                //if (!_context.Roles.Any(r => r.Name == role))
                //{
                //    roleStore.CreateAsync(new IdentityRole(role));
                //}
            }
        }

        public async void SeedUser()
        {
            var user = _context.Users.Where(u => u.Email == "admin@m-theth.co.za").Any();

            if (user == false)
            {

                List<ApplicationUser> userObj = new List<ApplicationUser>()
                {
                    new ApplicationUser
                    {
                        Email = "admin@m-theth.co.za",
                        UserName = "admin@m-theth.co.za",
                        FirstName = "Admin",
                        LastName = "Admin",
                        DateStamp = DateTime.Now,
                        IsSuperAdmin = true,
                        RoleId = 1,
                        CountryId = 203,
                        IsBinmak = true
                    },
                    new ApplicationUser
                    {
                        Email = "Mboniseni.Thethwayo@m-theth.co.za",
                        UserName = "Mboniseni.Thethwayo@m-theth.co.za",
                        FirstName = "Mboniseni",
                        LastName = "Thethwayo",
                        DateStamp = DateTime.Now,
                        IsAdmin = true,
                        RoleId = 2,
                        CountryId = 203
                    },
                    new ApplicationUser
                    {
                        Email = "Mboniseh@gmail.com",
                        UserName = "Mboniseh@gmail.com",
                        FirstName = "Mboniseni",
                        LastName = "Thethwayo",
                        DateStamp = DateTime.Now,
                        IsUser = true,
                        RoleId = 3,
                        CountryId = 203

                    },
                };


                foreach (ApplicationUser item in userObj)
                {
                    IdentityResult result = _userManager.CreateAsync(item, item.FirstName + "@Mtheth").Result;
                }

                _context.SaveChanges();
            }
        }

        public async void AssignRoles(ApplicationUser applicationUser, string role)
        {
            await _userManager.AddToRoleAsync(applicationUser, role);
        }
    }
}
