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

namespace BetaTesterSite.Controllers
{
    public class PhaseCreationController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string parentFolder = "1ZmHTYNmAhknN9IiWczo2OpOX1oznxRgD";
        private readonly DAL.BetaTesterContext context;

        public PhaseCreationController(DAL.BetaTesterContext context, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            return await Task.Run<IActionResult>(() => View());
        }

        public async Task<IActionResult> Save([FromForm]string[] data, string name)
        {
            if (name == null) return await Task.Run<IActionResult>(() => Json(false));


            var credentials = Autenticar();
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
            }
            return await Task.Run<IActionResult>(() => Json(true));
        }

        [HttpPost]
        public async Task<IActionResult> OpenArchive([FromForm]string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return await Task.Run<IActionResult>(() => Json(false));

            try
            {
                List<List<int>> data = new List<List<int>>();
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
                        List<int> line;
                        while ((s = sr.ReadLine()) != null)
                        {
                            line = new List<int>();
                            var val = s.Split(',');
                            line.AddRange(val.Select(x => int.Parse(x)));
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
        [ActionName("ListPhases")]
        public async Task<IActionResult> ListPhases()
        {
            var phases = new List<PhaseViewModel>();

            var credentials = Autenticar();
            using (var service = OpenService(await credentials))
            {
                var request = service.Result.Files.List();
                request.Fields = "files(id, name)";
                request.Q = "mimeType != 'application/vnd.google-apps.folder'";
                var resultado = request.Execute();
                var arquivos = resultado.Files;

                if (arquivos != null && arquivos.Any())
                {
                    foreach (var arquivo in arquivos)
                    {
                        phases.Add(new PhaseViewModel()
                        {
                            Id = arquivo.Id,
                            Name = arquivo.Name
                        });
                    }
                }
            }
            return await Task.Run<IActionResult>(() => Json(new
            {
                recordsTotal = phases.Count,
                recordsFiltered = phases.Count,
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