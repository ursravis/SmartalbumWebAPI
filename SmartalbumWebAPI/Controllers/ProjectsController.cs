using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using System.Web.Http.Cors;
using System.Drawing;
using System.Drawing.Imaging;

namespace SmartalbumWebAPI.Controllers
{
    //[EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    public class ProjectsController : ApiController
    {
        // GET api/values
        public IEnumerable<Project> Get()
        {

            return GetListOfProjects();
        }

        // GET api/values/5
        [HttpGet]
        public Project CreateThumbs(int id)
        {
            Project currentProject = null;
            var projects = GetListOfProjects().ToList();
            if (projects != null)
            {
                currentProject = projects.FirstOrDefault(it => it.ProjectId == id);
                if (currentProject != null && currentProject.Images != null)
                {
                    foreach (var image in currentProject.Images)
                    {
                        if (String.IsNullOrEmpty(image.ThumbnailSrc))
                        {
                            var test = image.ImageSrc.Replace("data:image/jpeg;base64,", string.Empty).Replace("data:image/png;base64,", string.Empty);
                            byte[] bytes = Convert.FromBase64String(test);
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                using (Image bmp = Image.FromStream(ms))
                                {
                                   var size= GetThumbnailSize(bmp);
                                    var thumImage= bmp.GetThumbnailImage(size.Width, size.Height, null, IntPtr.Zero);
                                    var thumbSrc = "data:image/jpeg;base64," + ToBase64String(thumImage);
                              
                                    image.ThumbnailSrc= thumbSrc;

                                }
                            }
                        }
                    }
                    SaveProjects(projects);
                }
            }
            return currentProject;
        }
        public string ToBase64String(Image bmp)
        {
            string base64String = string.Empty;
            MemoryStream memoryStream = null;

            try
            {
                memoryStream = new MemoryStream();
                bmp.Save(memoryStream, ImageFormat.Jpeg);
            }
            catch (Exception exc)
            {
                return String.Empty;
            }

            memoryStream.Position = 0;
            byte[] byteBuffer = memoryStream.ToArray();

            memoryStream.Close();

            base64String = Convert.ToBase64String(byteBuffer);
            byteBuffer = null;

            return base64String;
        }
        Size GetThumbnailSize(Image original)
        {
            // Maximum size of any dimension.
            const int maxPixels = 64;

            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double)maxPixels / originalWidth;
            }
            else
            {
                factor = (double)maxPixels / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }

        // POST api/values
        public Project Post(Project project)
        {
            var projects = GetListOfProjects().ToList();
            if (projects != null)
            {

                project.ProjectId = projects.Count + 1;
                project.CreatedBy = "sytem user";
                project.NoOfImages = project.Images.Count();
                projects.Add(project);
                SaveProjects(projects);
            }
            return project;



        }

        private void SaveProjects(List<Project> projects)
        {
            var projectsjson = Newtonsoft.Json.JsonConvert.SerializeObject(projects);
            string path = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "projects.json");
            System.IO.File.WriteAllText(path, projectsjson);
        }

        private IEnumerable<Project> GetListOfProjects()
        {
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            using (StreamReader file = File.OpenText(Path.Combine(path, "projects.json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                List<Project> projectlist = (List<Project>)serializer.Deserialize(file, typeof(List<Project>));
                return projectlist;
            }
        }
        // PUT api/values/5
        public Project Put(int id, Project project)
        {
            var projects = GetListOfProjects().ToList();
            if (projects != null && projects.FindIndex(it => it.ProjectId == project.ProjectId) >= 0)
            {
                project.CreatedBy = "sytem user";
                project.NoOfImages = project.Images.Count();
                int dbproj = projects.FindIndex(it => it.ProjectId == project.ProjectId);
                projects[dbproj] = project;
                SaveProjects(projects);
            }
            return project;
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
    public class Project
    {
        private string projectName;

        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
        }
        private int projectId;

        public int ProjectId
        {
            get { return projectId; }
            set { projectId = value; }
        }
        public string CreatedBy { get; set; }
        public int NoOfImages { get; set; }
        public List<SmartAlbum> Images { get; set; }

    }
    public class SmartAlbum
    {
        public string ImageName { get; set; }
        public string ImageSrc { get; set; }
        public string ThumbnailSrc { get; set; }
        public int ImageId { get; set; }
    }
}
