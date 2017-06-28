using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace csx
{
    public class ProjectFile
    {
        private readonly XDocument document;


        public ProjectFile()
        {
            var template = ResourceLoader.ReadResourceFile("csproj.template");
            document = XDocument.Parse(template);

        }

        public void AddPackageReference(PackageReference packageReference)
        {
            var itemGroupElement = document.Descendants("ItemGroup").Single();
            var packageReferenceElement = new XElement("PackageReference");
            packageReferenceElement.Add(new XAttribute("Include", packageReference.Id));
            packageReferenceElement.Add(new XAttribute("Version", packageReference.Version));
            itemGroupElement.Add(packageReferenceElement);
        }

        public void Save(string pathToProjectFile)
        {
            using (var fileStream = new FileStream(pathToProjectFile, FileMode.Create, FileAccess.Write))
            {
                document.Save(fileStream);
            }
        }
    }
}