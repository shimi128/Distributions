using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Distributions.Web.Models;
using Distributions.Web.Utility;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Host.SystemWeb;


namespace Distributions.Web.Controllers
{
    [Authorize]
    public class AdminController : BaseApiController
    {
        private readonly IdentityDbContext _identityDbContext;
        
        public AdminController(IdentityDbContext identityDbContext)
        {
            _identityDbContext = identityDbContext;
        }

       
        // GET: api/Admin
         [Authorize(Roles = "Admin")]
        [Route("Admin")]
        public List<ManagerUserViewModel> Get()
        {
            var allUsers = _identityDbContext.Users.ToList();
            var roles = _identityDbContext.Roles.ToList();

            var viewModels = new List<ManagerUserViewModel>();

            foreach (var user in allUsers)
            {
                viewModels.Add(new ManagerUserViewModel { UserId = user.Id, UserName = user.UserName, Role = roles.First(x => x.Id == user.Roles.First().RoleId).Name });
            }

            return viewModels;
        }

        
        [Route("SignIn")]
        [HttpGet]
        public HttpResponseMessage SignIn()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { userName = HttpContext.Current.User.Identity.GetUserName(),isAdmin= HttpContext.Current.User.IsInRole("Admin") });
        }
        // GET: api/Admin/5
        public string Get(int id)
        {
            return "value";
        }

        
        [Route("GetRoles")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetRoles()
        {
            var roles = _identityDbContext.Roles.Where(x=>x.Name!="Admin").ToList();
            return Request.CreateResponse(HttpStatusCode.OK, new { Roles = roles });
        }

        //// POST: api/Admin
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Admin/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Admin/5
        //public void Delete(int id)
        //{
        //}

       
        [Route("Register")]
        [Authorize(Roles = "Admin")]
        public async Task<HttpResponseMessage> Register(RegisterViewModel model)
        {
            var errors = new List<string>();
            errors = ModelErrorChecker.Check(ModelState);
            if (errors.Count == 0)
            {
                if (ModelState.IsValid)
                {
                    if (UserManager.FindByEmail(model.Email) == null)
                    {
                        var user = new ApplicationUser { UserName = model.Email, Email = model.Email};
                        var result = await UserManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            var newUser = UserManager.FindByEmail(model.Email);
                            await UserManager.AddToRoleAsync(newUser.Id, model.Role);
                            //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, result.Errors.First());
                    }
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Email address is already in use.");
                }
            }
            else
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, errors);
            return Request.CreateResponse(HttpStatusCode.OK);
        }


    }
}
