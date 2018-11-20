using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using BetaTesterSite.Models;
using Microsoft.AspNetCore.Identity;

namespace BetaTesterSite.Controllers
{
    public class PhaseCreationController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string parentFolder = "1ZmHTYNmAhknN9IiWczo2OpOX1oznxRgD";
        private readonly DAL.BetaTesterContext context;
        private readonly UserManager<DAL.Identity.User> userManager;

        public PhaseCreationController(DAL.BetaTesterContext context, IHostingEnvironment hostingEnvironment, UserManager<DAL.Identity.User> userManager)
        {
            _hostingEnvironment = hostingEnvironment;
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return await Task.Run<IActionResult>(() => View());
        }

        public async Task<IActionResult> Create()
        {
            return await Task.Run<IActionResult>(() => View());
        }


        public async Task<IActionResult> Save([FromForm]string[] data, string name)
        {
            if (name == null) return await Task.Run<IActionResult>(() => Json(false));


            var credentials = Autenticar();
            Google.Apis.Drive.v3.Data.File file;
            using (var service = OpenService(await credentials))
            {
                var memStream = new MemoryStream();
                var streamWriter = new StreamWriter(memStream);

                foreach (var item in data)
                    streamWriter.WriteLine(item);

                streamWriter.Flush();


                var fileMetadata = new Google.Apis.Drive.v3.Data.File() { Name = name };
                fileMetadata.Parents = new List<string>() { parentFolder };

                var request = service.Result.Files.Create(fileMetadata, new MemoryStream(memStream.ToArray()), "application/octet-stream");
                request.Fields = "id";
                request.Upload();
                file = request.ResponseBody;
            }

            var user = await userManager.GetUserAsync(User);
            var phase = new DAL.Phase
            {
                FileId = file.Id,
                UserId = user.Id,
                Name = name
            };

            this.context.Phase.Add(phase);
            this.context.SaveChanges();

            return await Task.Run<IActionResult>(() => Json(true));
        }

        [HttpPost]
        public async Task<IActionResult> OpenArchive([FromForm]string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return await Task.Run<IActionResult>(() => Json(false));

            try
            {
                List<List<string>> data = new List<List<string>>();
                var credentials = Autenticar();
                using (var service = OpenService(await credentials))
                {

                    var request = service.Result.Files.Get(id);
                    var stream = new System.IO.MemoryStream();
                    request.Download(stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    using (StreamReader sr = new StreamReader(stream))
                    {
                        string s = "";
                        List<string> line;
                        while ((s = sr.ReadLine()) != null)
                        {
                            line = new List<string>();
                            var val = s.Split(',');
                            line.AddRange(val);
                            data.Add(line);
                        }
                    }
                }
                return await Task.Run<IActionResult>(() => Json(data));
            }
            catch
            {
                return Json(false);
            }
        }

        [HttpPost]
        [ActionName("RemovePhase")]
        public async Task<IActionResult> RemovePhase(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return await Task.Run<IActionResult>(() => Json(false));

            var credentials = Autenticar();
            using (var service = OpenService(await credentials))
            {
                var request = service.Result.Files.Delete(id);
                request.Execute();
            }

            var phase = this.context.Phase.Single(x => x.FileId == id);
            this.context.Phase.Remove(phase);
            this.context.SaveChanges();

            return await Task.Run<IActionResult>(() => Json(true));
        }

        private static async Task<Google.Apis.Auth.OAuth2.UserCredential> Autenticar()
        {
            Google.Apis.Auth.OAuth2.UserCredential credentials;

            using (var stream = new System.IO.FileStream("client_id.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var diretorioAtual = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var diretoriocredentials = System.IO.Path.Combine(diretorioAtual, "credential");

                credentials = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                    Google.Apis.Auth.OAuth2.GoogleClientSecrets.Load(stream).Secrets,
                    new[] { Google.Apis.Drive.v3.DriveService.Scope.Drive },
                    "user",
                    System.Threading.CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(diretoriocredentials, true)).Result;
            }

            return await Task.Run<Google.Apis.Auth.OAuth2.UserCredential>(() => credentials);
        }

        private static async Task<Google.Apis.Drive.v3.DriveService> OpenService(Google.Apis.Auth.OAuth2.UserCredential credentials)
        {
            return await Task.Run<Google.Apis.Drive.v3.DriveService>(() => new Google.Apis.Drive.v3.DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials
            }));
        }

        [HttpPost]
        [ActionName("ListIndexPhases")]
        public async Task<IActionResult> ListRecentPhases()
        {
            var user = await userManager.GetUserAsync(User);
            var UserId = user.Id;

            var phases = (from y in this.context.PhasesIndexView
                     join upf in this.context.UserPhaseFav on new { y.PhaseId, UserId } equals new { upf.PhaseId, upf.UserId } into up
                     join pr in this.context.PhaseRate on new { y.PhaseId, UserId } equals new { pr.PhaseId, pr.UserId } into _pr
                     from upf in up.DefaultIfEmpty()
                     from pr in _pr.DefaultIfEmpty()
                     orderby y.PhaseId descending
                     select new Models.PhasesIndexViewModel{
                         Completed = y.Completed,
                         UserId = y.UserId,
                         Dies = y.Dies,
                         FileId = y.FileId,
                         Name = y.Name,
                         PhaseId = y.PhaseId,
                         Played = y.Played,
                         Rating = y.Rating,
                         UserRate = pr != null? pr.Rate : 0,
                         Tested = y.Tested,
                         Fav = upf == null? false : true
                     }).Take(10);

            //var phases = this.context.PhasesIndexView.OrderByDescending(x => x.PhaseId).Take(10);

            return await Task.Run<IActionResult>(() => Json(new
            {
                recordsTotal = phases.Count(),
                recordsFiltered = phases.Count(),
                data = phases
            }));
        }

        [HttpPost]
        [ActionName("FavoritePhase")]
        public async Task<IActionResult> FavoritePhase(int? phaseId)
        {
            if (!phaseId.HasValue) return Forbid();

            var user = await userManager.GetUserAsync(User);
            
            if(this.context.UserPhaseFav.Any(x => x.PhaseId == phaseId && x.UserId == user.Id))
            {
                var userPhaseFav = this.context.UserPhaseFav.Single(x => x.PhaseId == phaseId && x.UserId == user.Id);
                this.context.UserPhaseFav.Remove(userPhaseFav);
            }
            else
            {
                this.context.UserPhaseFav.Add(new DAL.UserPhaseFav
                {
                    PhaseId = phaseId.Value,
                    UserId = user.Id
                });
            }
            this.context.SaveChanges();

            return await Task.Run<IActionResult>(() => Json(true));
        }

        [HttpPost]
        [ActionName("SaveRate")]
        public async Task<IActionResult> SaveRate(int? phaseId, int? rate)
        {
            if (!phaseId.HasValue && !rate.HasValue) return Forbid();

        var user = await userManager.GetUserAsync(User);
            
            if(this.context.PhaseRate.Any(x => x.PhaseId == phaseId && x.UserId == user.Id))
            {
            var phaseRate = this.context.PhaseRate.Single(x => x.PhaseId == phaseId && x.UserId == user.Id);
                phaseRate.Rate = rate.Value;
            this.context.PhaseRate.Update(phaseRate);
        }
            else
            {
            this.context.PhaseRate.Add(new DAL.PhaseRate
            {
                PhaseId = phaseId.Value,
                UserId = user.Id
            });
        }
            this.context.SaveChanges();

            return await Task.Run<IActionResult>(() => Json(true));
        }

    [HttpPost]
        [ActionName("ListPhases")]
        public async Task<IActionResult> ListPhases()
        {
            //var phases = new List<PhaseViewModel>();

            var credentials = Autenticar();
            //using (var service = OpenService(await credentials))
            //{
            //    var request = service.Result.Files.List();
            //    request.Fields = "files(id, name)";
            //    request.Q = "mimeType != 'application/vnd.google-apps.folder'";
            //    var resultado = request.Execute();
            //    var arquivos = resultado.Files;

            //    if (arquivos != null && arquivos.Any())
            //    {
            //        foreach (var arquivo in arquivos)
            //        {
            //            phases.Add(new PhaseViewModel()
            //            {
            //                Id = arquivo.Id,
            //                Name = arquivo.Name
            //            });
            //        }
            //    }
            //}
            var user = await userManager.GetUserAsync(User);
            var phases = this.context.Phase.Where(x => x.UserId == user.Id).OrderByDescending(x => x.PhaseId);


            return await Task.Run<IActionResult>(() => Json(new
            {
                recordsTotal = phases.Count(),
                recordsFiltered = phases.Count(),
                data = phases
            }));

        }

        private static void CreateFolder(Google.Apis.Drive.v3.DriveService service, string directoryName)
        {
            var diretorio = new Google.Apis.Drive.v3.Data.File();
            diretorio.Name = directoryName;
            diretorio.MimeType = "application/vnd.google-apps.folder";
            var request = service.Files.Create(diretorio);
            request.Execute();
        }
    }
}