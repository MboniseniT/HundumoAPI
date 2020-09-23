using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BinmakAPI.Data;
using BinmakAPI.Entities;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using BinmakAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BinmakBackEnd.Entities;

namespace BinmakAPI.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly BinmakDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(BinmakDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] BinmakBackEnd.Models.ForgotPassword forgotPassword)
        {
            try
            {
                if (forgotPassword is null)
                    return BadRequest("Make Sure Form Is Filled!");


                var user = await _userManager.FindByEmailAsync(forgotPassword.Email);

                if (user == null)
                    return BadRequest("No user found!");

                var token = _userManager.GeneratePasswordResetTokenAsync(user);
                var password = CreatePassword(6);

                var result = await _userManager.ResetPasswordAsync(user, token.ToString(), password);

                if (result.Succeeded)
                {
                    
                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new NetworkCredential("binmak-systems@m-theth.co.za", "Binmak@2020"),
                            Timeout = 20000
                        };

                        using (var message = new MailMessage("binmak-systems@m-theth.co.za", forgotPassword.Email)
                        {
                            IsBodyHtml = true,
                            Subject = "Password Change",
                            Body = "<html><body>Hi " + user.FirstName + ", <br/><br/>Your Password change in Binmak System has been successfully changed, Here are your updated credentials: <br/> Username: " + user.Email + " <br/>Password: " + password + "  <br/><br/><p>Binmak</p></body></html></body></html>"
                        })
                        {
                            smtp.Send(message);
                        }

                    return Ok();
                }
                else
                {
                    return BadRequest("Something bad happened!");
                }

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! "+Ex.Message);
            }

        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();

            return Ok(roles);
        }

        [HttpGet("binmakModules")]
        public IActionResult GetBinmakSystems()
        {
            var bnmakSystems = _context.BinmakModules.ToList();

            return Ok(bnmakSystems);
        }

        [HttpGet("groupsByRoot")]
        public IActionResult GetGroupsByRoot(int rootId)
        {
            var groups = _context.Groups.Where(id => id.RootId == rootId);

            return Ok(groups);
        }

        [HttpGet("groups")]
        public IActionResult GetGroups(string reference)
        {
            var groups = _context.Groups.Where(id => id.Reference == reference);

            return Ok(groups);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Post([FromBody] Register applicationUser)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string response = "";

                    if (applicationUser == null)
                    {
                        return BadRequest("Invalid User Data");
                    }

                    var user = await _userManager.FindByEmailAsync(applicationUser.Email);

                    if (user == null)
                    {

                        user = new ApplicationUser
                        {
                            FirstName = applicationUser.FirstName,
                            LastName = applicationUser.LastName,
                            Email = applicationUser.Email,
                            UserName = applicationUser.Email,
                            DateStamp = DateTime.Now,
                            Reference = applicationUser.Reference,
                        };


                        var password = CreatePassword(6);

                        var userResult = await _userManager.CreateAsync(user, password);

                         if (applicationUser.RoleId == 2)
                        {
                            await _userManager.AddToRoleAsync(user, "ADMINISTRATOR");
                            user.IsAdmin = true;
                        }
                        else if (applicationUser.RoleId == 3)
                        {
                            await _userManager.AddToRoleAsync(user, "USER");
                            user.IsUser = true;
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(user, "GUEST");
                            user.IsGuest = true;
                        }

                        _context.Users.Update(user);

                        List<int> tempGI = new List<int>();
                        tempGI.Add(applicationUser.GroupIds);

                        List<int> groupIds = tempGI;

                        foreach (int groupId in groupIds)
                        {
                            Group group = _context.Groups.FirstOrDefault(id => id.GroupId == groupId);
                            AssetNode accessFromAssetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == group.AssetNodeId);

                            List<AssetNode> assetNodes = _context.AssetNodes.Where(id => (id.RootAssetNodeId == accessFromAssetNode.RootAssetNodeId) 
                            && (id.AssetNodeId > accessFromAssetNode.AssetNodeId)).ToList();

                            foreach (var item in assetNodes)
                            {
                                UserGroup userGroup = new UserGroup();
                                userGroup.GroupId = item.GroupId;
                                userGroup.UserId = user.Id;
                                userGroup.RootId = item.RootAssetNodeId;

                                UserGroup userGroupChecker = _context.UserGroups.FirstOrDefault(id => (id.GroupId == groupId) && (id.UserId == user.Id));

                                if (userGroupChecker == null)
                                {
                                    _context.UserGroups.Add(userGroup);
                                    _context.SaveChanges();
                                }
                            }
                        }

                        foreach (var assignedModule in applicationUser.AssignedBinmakModulesIds)
                        {
                            BinmakModuleAccess binmakModule = new BinmakModuleAccess();
                            binmakModule.BinmakModuleId = assignedModule;
                            binmakModule.Reference = user.Id;
                            binmakModule.DateStamp = DateTime.Now;

                            _context.BinmakModuleAccesses.Add(binmakModule);
                        }

                        _context.SaveChanges();

                        if (userResult != IdentityResult.Success)
                        {
                            response = "Account for " + applicationUser.Email + " Could Not Be Created At This Time. Try again. ";
                            return BadRequest(response);
                        }
                        else
                        {

                            var smtp = new SmtpClient
                            {
                                Host = "smtp.gmail.com",
                                Port = 587,
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                Credentials = new NetworkCredential("binmak-systems@m-theth.co.za", "Binmak@2020"),
                                Timeout = 20000
                            };

                            using (var message = new MailMessage("binmak-systems@m-theth.co.za", applicationUser.Email)
                            {
                                IsBodyHtml = true,
                                Subject = "Binmak Account Details",
                                Body = "<html><body>Hi " + applicationUser.FirstName + ", <br/>Please use the credentials below in order to log in to Binmak System: <br/><br/>Link: http://binmakdev.dedicated.co.za <br/>Username: " + applicationUser.Email + " <br/>Password: " + password + "  <br/><br/><p>Binmak</p></body></html></body></html>"
                            })
                            {
                                smtp.Send(message);
                            }

                        }
                    }

                    return Ok();
                }
                catch (Exception Ex)
                {
                    return BadRequest("Could not create account for: " + applicationUser.Email + " " + Ex.Message);
                }
            }
            return BadRequest("Model not valid ");
        }

        [HttpPost("systemAccount")]
        public async Task<IActionResult> CreateSystemAccount([FromBody] ApplicationUserVM applicationUser)
        {

            try
            {
                string response = "";

                if (applicationUser == null)
                {
                    return BadRequest("Invalid User Data");
                }

                var user = await _userManager.FindByEmailAsync(applicationUser.Email);

                if (user == null)
                {

                    user = new ApplicationUser
                    {
                        FirstName = applicationUser.FirstName,
                        LastName = applicationUser.LastName,
                        Email = applicationUser.Email,
                        UserName = applicationUser.Email,
                        DateStamp = DateTime.Now,
                        CompanyId = CreateCompany(applicationUser.CompanyName).CompanyId,
                        CountryId = applicationUser.CountryId,
                        Address = applicationUser.Address,
                        Address2 = applicationUser.Address2,
                        City = applicationUser.City,
                        Zip = applicationUser.Zip,
                        IsSuperAdmin = true,
                        IsAccountOwner = true,
                        RoleId = 1
                    };


                    var userResult = await _userManager.CreateAsync(user, applicationUser.Password);

                    if (userResult != IdentityResult.Success)
                    {
                        response = "Account for " + applicationUser.Email + " Could Not Be Created At This Time. Try again. ";
                        return BadRequest(response);
                    }
                    else
                    {

                        if (user.RoleId == 1)
                        {
                        await _userManager.AddToRoleAsync(user, "ADMINISTRATOR");
                        user.IsAdmin = true;
                    }
                        else if (user.RoleId == 2)
                        {
                            await _userManager.AddToRoleAsync(user, "ADMINISTRATOR");
                            user.IsAdmin = true;
                        }
                    else if (user.RoleId == 3)
                    {
                        await _userManager.AddToRoleAsync(user, "USER");
                        user.IsUser = true;
                    }
                    else
                        {
                            await _userManager.AddToRoleAsync(user, "GUEST");
                        user.IsUser = true;
                    }

                    foreach (var bm in _context.BinmakModules.ToList())
                    {
                        BinmakModuleAccess binmakModule = new BinmakModuleAccess();
                        binmakModule.BinmakModuleId = bm.BinmakModuleId;
                        binmakModule.Reference = user.Id;

                        _context.BinmakModuleAccesses.Add(binmakModule);
                    }

                    _context.SaveChanges();

                    var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new NetworkCredential("binmak-systems@m-theth.co.za", "Binmak@2020"),
                            Timeout = 20000
                        };

                        using (var message = new MailMessage("binmak-systems@m-theth.co.za", applicationUser.Email)
                        {
                            IsBodyHtml = true,
                            Subject = "Binmak Software System Account Details",
                            Body = "<html><body>Hi " + applicationUser.FirstName + ", <br/>Please use the credentials below in order to log in to Binmak Software System: <br/><br/>Link: http://binmakdev.dedicated.co.za <br/>Domain: " + applicationUser.CompanyName + "<br/>Username: " + applicationUser.Email + " <br/>Password: " + applicationUser.Password+ "  <br/><br/><p>Binmak</p></body></html></body></html>"
                        })
                        {
                            smtp.Send(message);
                        }

                    }
                }
                else
                {
                    return BadRequest("Account already created! Choose different email or sign-in");
                }

                    
            }
            catch (Exception Ex)
            {
                return BadRequest("Could not create account for: " + applicationUser.Email + " " + Ex.Message);
            }

            return Ok();
        }
    

        public Company CreateCompany(string CompanyName)
        {
            Company company = new Company();
            company.DateStamp = DateTime.Now;
            company.CompanyName = CompanyName;
            _context.Companies.Add(company);
            _context.SaveChanges();

            return company;
        }

        [HttpGet("countries")]
        public IActionResult GetCountries()
        {
            try
            {
                return Ok(_context.Countries.ToList());
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! " + Ex.Message);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody] Login model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    if (user.IsLocked)
                    {
                        return BadRequest("User is currently locked, ask administrator to unlock you!");
                    }

                    if (user.IsDeleted)
                    {
                        return BadRequest("User is deleted, ask administrator for reinstatement!");
                    }

                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (result.Succeeded)
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Id ),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
                            new Claim(JwtRegisteredClaimNames.Email, user.UserName)
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mysupersedkjhulfgyuerfw344cret"));

                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: "http://binmakdev.dedicated.co.za:81",
                            audience: "http://binmakdev.dedicated.co.za:80",
                            claims: claims,
                            expires: DateTime.UtcNow.AddDays(29),
                            signingCredentials: creds);

                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            userId = token.Subject,
                            username = user.UserName,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            isSuperAdmin = user.IsSuperAdmin,
                            isBinmak = user.IsBinmak,
                            isUser = user.IsUser,
                            isGuest = user.IsGuest,
                            isAdmin = user.IsAdmin,
                            role = user.RoleId,
                            binmakModules = GetBinmakModulesByUser(user.Id),
                            assignedAssetNodes = GetAssetNodesByUser(user.Id),
                            topAssetNode = GetTopAssetNodesByUser(user.Id),
                        };

                        return Created("", results);
                    }
                    else
                    {
                        return BadRequest("Login Failed, Wrong Username/Password");
                    }
                }
                else
                {
                    return BadRequest("Account Does Not Exist, Please Register First");
                }
            }
            return BadRequest("Login Failed");
        }

        [HttpGet("admins")]
        public IActionResult Lookups()
        {
            try
            {
                var users = _context.Users.Where(x => x.Email != "admin@m-theth.co.za");

                return Ok(users);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("deleteUser")]
        public IActionResult DeleteUser([FromBody] BinmakBackEnd.Models.DeleteUser model)
        {
            if (model.Id == "" || model.Reference == "")
            {
                return BadRequest("Error, Make sure user is selected.");
            }

            try
            {
                if (model.Id == model.Reference)
                {
                    return BadRequest("Error, You can not delete yourself.");
                }

                ApplicationUser applicationUser = _context.Users.FirstOrDefault(id => id.Id == model.Id);

                if (applicationUser != null)
                {
                    applicationUser.IsDeleted = true;
                    _context.Users.Update(applicationUser);
                    _context.SaveChanges();
                }

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("reinstate")]
        public IActionResult ReinstateUser([FromBody] BinmakBackEnd.Models.DeleteUser model)
        {
            if (model.Id == "" || model.Reference == "")
            {
                return BadRequest("Error, Make sure user is selected.");
            }

            try
            {
                if (model.Id == model.Reference)
                {
                    return BadRequest("Error, You can not re-instate yourself.");
                }

                ApplicationUser applicationUser = _context.Users.FirstOrDefault(id => id.Id == model.Id);

                if (applicationUser != null)
                {
                    applicationUser.IsDeleted = false;
                    _context.Users.Update(applicationUser);
                    _context.SaveChanges();
                }

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpGet("users")]
        public IActionResult GetAssetAdmins(string reference)
        {
            try
            {
                UserGroup userGroup = _context.UserGroups.FirstOrDefault(id => id.UserId == reference);

                if (userGroup == null)
                {
                    return BadRequest("Something bad happened. Make sure you have added atleast one asset node");
                }

                int groupId = userGroup.RootId;

                List<UserGroup> userGroups = _context.UserGroups.Where(id => id.RootId == groupId).ToList();
                
                List<string> refs = new List<string>();

                foreach (var ug in userGroups)
                {
                    refs.Add(ug.UserId);
                }

                var usersDist = refs.Distinct().ToList();

                List<ApplicationUser> users = new List<ApplicationUser>();

                foreach (var r in usersDist)
                {
                    users.Add(_context.Users.FirstOrDefault(x => x.Id == r));
                }

                var assetUsers = users.Select(result => new
                {
                    Id = result.Id,
                    Name = result.FirstName,
                    LastName = result.LastName,
                    Email = result.Email,
                    Date = result.DateStamp,
                    AssignedBinmakModules = GetBinmakModulesByUser(result.Id),
                    AssignedBinmakModulesIds = GetBinmakModulesByUserIds(result.Id),
                    AssignedAssetNodes = GetAssetNodesByUser(result.Id),
                    AssignedAssetNodesIds = GetAssetNodesIdsByUser(result.Id),
                    TopAssetNode = GetTopAssetNodesByUser(result.Id),
                    GroupAssetNodes = GetAssetNodesByRoot(result.Id),
                    IsDeleted = result.IsDeleted,
                    RoleId = result.RoleId,
                    BinmakModules = GetBinmakModulesByUserIds(),
                    RootId = result.RootId
                    //Role =  _context.Roles.FirstOrDefault(id=>id.Id == result.RoleId)
                });

            return Ok(assetUsers);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }


        [HttpPost("updateUser")]
        public IActionResult UpdateUser([FromBody] BinmakBackEnd.Models.UpdateUser updateUser)
        {
            if (updateUser == null)
            {
                return BadRequest("Something bad happened. Make sure user is selected");
            }
            try
            {
                ApplicationUser applicationUser = _context.Users.FirstOrDefault(id => id.Id == updateUser.Id);

                if (updateUser.RoleId == 1)
                {
                    return BadRequest("Error. Super administrator can not be added, atleat for now.");
                }

                applicationUser.RoleId = updateUser.RoleId;
                applicationUser.FirstName = updateUser.FirstName;
                applicationUser.LastName = updateUser.LastName;

                //Updating modules
                List<BinmakModuleAccess> binmakModuleAccesses = _context.BinmakModuleAccesses.Where(id => id.Reference == updateUser.Id).ToList();

                _context.BinmakModuleAccesses.RemoveRange(binmakModuleAccesses);
                _context.SaveChanges();

                List<BinmakModule> binmakModules = new List<BinmakModule>();
                foreach (int item in updateUser.BinmakModuleId)
                {
                    binmakModules.Add(_context.BinmakModules.FirstOrDefault(id => id.BinmakModuleId == item));
                }

                foreach (var item in binmakModules)
                {
                    BinmakModuleAccess binmakModule = new BinmakModuleAccess();
                    binmakModule.BinmakModuleId = item.BinmakModuleId;
                    binmakModule.Reference = updateUser.Id;

                    _context.BinmakModuleAccesses.Add(binmakModule);
                }

                _context.SaveChanges();

                List<UserGroup> userGroupsToBeRemoved = _context.UserGroups.Where(id => id.UserId == updateUser.Id).ToList();
                _context.RemoveRange(userGroupsToBeRemoved);
                _context.SaveChanges();

                List<UserGroup> userGroups = new List<UserGroup>();

                AssetNode assetNodes = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == updateUser.AssignedAssetsNode);
                List<AssetNode> assetNodes1 = new List<AssetNode>();

                if (assetNodes.RootAssetNodeId == 0)
                {
                    assetNodes1 = _context.AssetNodes.Where(id => (id.RootAssetNodeId == assetNodes.AssetNodeId) || (id.AssetNodeId >= assetNodes.AssetNodeId)).ToList();
                }
                else
                {
                    assetNodes1 = _context.AssetNodes.Where(id => (id.RootAssetNodeId == assetNodes.RootAssetNodeId) && (id.AssetNodeId >= assetNodes.AssetNodeId)).ToList();
                }

                
                var orderAssetNodes = assetNodes1.OrderBy(id => id.AssetNodeId).ToList();
                var lastItem = orderAssetNodes.LastOrDefault();
                int rootLatItem = 0;
                if (lastItem.RootAssetNodeId == 0)
                {
                    rootLatItem = lastItem.AssetNodeId;
                }
                else
                {
                    rootLatItem = lastItem.RootAssetNodeId;
                }

                List<AssetNode> assetNodes2 = assetNodes1.Where(id => (id.RootAssetNodeId == rootLatItem) || (id.AssetNodeId <= updateUser.AssignedAssetsNode)).ToList();

                foreach (var item in assetNodes2)
                {
                    UserGroup userGroup = new UserGroup();
                    userGroup.RootId = rootLatItem;
                    userGroup.GroupId = _context.Groups.FirstOrDefault(id=>id.AssetNodeId == item.AssetNodeId).GroupId;
                    userGroup.UserId = updateUser.Id;
                    _context.UserGroups.Add(userGroup);
                }

                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        public List<BinmakModule> GetBinmakModulesByUserIds()
        {
            List<BinmakModule> binmakModules = _context.BinmakModules.ToList();

            return binmakModules;
        }

        public List<int> GetBinmakModulesByUserIds(string userId)
        {
            List<BinmakModuleAccess> binmakModuleAccess = _context.BinmakModuleAccesses.Where(id => id.Reference == userId).ToList();
            List<int> binmakModules = new List<int>();

            foreach (var bma in binmakModuleAccess)
            {
                binmakModules.Add(_context.BinmakModules.FirstOrDefault(id => id.BinmakModuleId == bma.BinmakModuleId).BinmakModuleId);
            }

            return binmakModules;
        }

        public List<BinmakModule> GetBinmakModulesByUser(string userId)
        {
            List<BinmakModuleAccess> binmakModuleAccess = _context.BinmakModuleAccesses.Where(id => id.Reference == userId).ToList();
            List<BinmakModule> binmakModules = new List<BinmakModule>();

            foreach (var bma in binmakModuleAccess)
            {
                binmakModules.Add(_context.BinmakModules.FirstOrDefault(id => id.BinmakModuleId == bma.BinmakModuleId));
            }

            return binmakModules;
        }

        public List<AssetNode> GetAssetNodesByUser(string userId)
        {
            List<UserGroup> userGroups = _context.UserGroups.Where(id => id.UserId == userId).ToList();
            List<AssetNode> assetNodes = new List<AssetNode>();

            foreach (var bma in userGroups)
            {
                Group group = _context.Groups.FirstOrDefault(id => id.GroupId == bma.GroupId);
                if (group == null)
                {
                    return null;
                }
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == group.AssetNodeId);
                assetNodes.Add(assetNode);
            }

            return assetNodes;
        }

        public List<AssetNode> GetAssetNodesByRoot(string userId)
        {
            List<UserGroup> userGroups1 = _context.UserGroups.Where(id => id.UserId == userId).OrderBy(id => id.GroupId).ToList();
            List<AssetNode> assetNodes = new List<AssetNode>();
            //List<int> groupIds = new List<int>();
            //List<Group> groups = new List<Group>();

            //foreach (UserGroup item in userGroups)
            //{
            //    groupIds.Add(item.GroupId);
            //}

            //foreach (int groupId in groupIds)
            //{
            //    groups.Add(_context.Groups.FirstOrDefault(id => id.GroupId == groupId));
            //}

            //foreach (Group group in groups)
            //{
            //    assetNodes.Add(_context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == group.AssetNodeId));
            //}


            foreach (var bma in userGroups1)
            {
                Group group = _context.Groups.FirstOrDefault(id => id.GroupId == bma.GroupId);
                if (group == null)
                {
                    return null;
                }
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == group.AssetNodeId);
                assetNodes.Add(assetNode);
            }

            AssetNode lastAssetNode = assetNodes.LastOrDefault();
            if (lastAssetNode == null)
            {
                return new List<AssetNode>();
            }

            List<AssetNode> assetNodes2 = _context.AssetNodes.Where(r => r.RootAssetNodeId == lastAssetNode.RootAssetNodeId).ToList();

            if (lastAssetNode.RootAssetNodeId == 0)
            {
                AssetNode root = _context.AssetNodes.FirstOrDefault(r => r.AssetNodeId == lastAssetNode.AssetNodeId);
                assetNodes2.Add(root);
            }
            else
            {
                AssetNode root = _context.AssetNodes.FirstOrDefault(r => r.AssetNodeId == lastAssetNode.RootAssetNodeId);
                assetNodes2.Add(root);
            }

            List<AssetNode> orderedAN = assetNodes2.OrderBy(id => id.AssetNodeId).ToList();

            return orderedAN;

            //return assetNodes;
        }

        public List<int> GetAssetNodesIdsByUser(string userId)
        {
            List<UserGroup> userGroups = _context.UserGroups.Where(id => id.UserId == userId).ToList();
            List<int> assetNodes = new List<int>();

            foreach (var bma in userGroups)
            {
                Group group = _context.Groups.FirstOrDefault(id => id.GroupId == bma.GroupId);
                if (group == null)
                {
                    return null;
                }
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == group.AssetNodeId);
                assetNodes.Add(assetNode.AssetNodeId);
            }

            return assetNodes;
        }

        public AssetNode GetTopAssetNodesByUser(string userId)
        {
            List<UserGroup> userGroups = _context.UserGroups.Where(id => id.UserId == userId).ToList();
            List<UserGroup> userGroups1 = userGroups.OrderBy(id => id.GroupId).ToList();
            List<AssetNode> assetNodes = new List<AssetNode>();

            foreach (var bma in userGroups1)
            {
                Group group = _context.Groups.FirstOrDefault(id => id.GroupId == bma.GroupId);
                if (group == null)
                {
                    return null;
                }
                AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == group.AssetNodeId);
                assetNodes.Add(assetNode);
            }

            if (assetNodes.Count > 0)
            {
                return assetNodes[0];
            }

            return null;
        }

        [HttpGet("lookups")]
        public IActionResult Admins()
        {
            try
            {
                var templates = new Dictionary<string, object>();

                //var template = _context.Templates.ToList();

                templates.Add("template", new List<object>());

                //return Ok(templates);
                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happaned! " + Ex.Message);
            }
        }

        //[HttpPost("OverallProductionProcess")]
        //public IActionResult Post([FromBody] OverallProductionProcess model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id=>id.AssetUserId == model.AssetUserId);
        //        assetUser.IsOverallProductionProcess = model.IsOverallProductionProcess;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. "+Ex.Message);
        //    }
        //}

        //[HttpPost("OverallProductionBuffer")]
        //public IActionResult Post([FromBody] OverallProductionBuffer model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id => id.AssetUserId == model.AssetUserId);
        //        assetUser.IsOverallProductionBuffer = model.IsOverallProductionBuffer;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. " + Ex.Message);
        //    }
        //}

        //[HttpPost("DrillBlast")]
        //public IActionResult Post([FromBody] DrillBlast model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id => id.AssetUserId == model.AssetUserId);
        //        assetUser.IsDrillAndBlast = model.IsDrillAndBlast;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. " + Ex.Message);
        //    }
        //}

        //[HttpPost("LoadHaul")]
        //public IActionResult Post([FromBody] LoadHaul model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id => id.AssetUserId == model.AssetUserId);
        //        assetUser.IsLoadAndHaul = model.IsLoadAndHaul;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. " + Ex.Message);
        //    }
        //}

        //[HttpPost("Support")]
        //public IActionResult Post([FromBody] Support model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id => id.AssetUserId == model.AssetUserId);
        //        assetUser.IsSupport = model.IsSupport;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. " + Ex.Message);
        //    }
        //}

        //[HttpPost("She")]
        //public IActionResult Post([FromBody] She model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id => id.AssetUserId == model.AssetUserId);
        //        assetUser.IsShe = model.IsShe;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. " + Ex.Message);
        //    }
        //}

        //[HttpPost("FacePrep")]
        //public IActionResult Post([FromBody] FacePrep model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id => id.AssetUserId == model.AssetUserId);
        //        assetUser.IsFacePreparation = model.IsFacePrep;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. " + Ex.Message);
        //    }
        //}

        //[HttpPost("EquipStatus")]
        //public IActionResult Post([FromBody] EquipStatus model)
        //{
        //    try
        //    {
        //        AssetUser assetUser = _context.AssetUsers.FirstOrDefault(id => id.AssetUserId == model.AssetUserId);
        //        assetUser.IsEquipmentStatus = model.IsEquipStatus;
        //        _context.AssetUsers.Update(assetUser);
        //        _context.SaveChanges();

        //        return Ok();
        //    }
        //    catch (Exception Ex)
        //    {
        //        return BadRequest("Something bad happened. " + Ex.Message);
        //    }
        //}

    }
}