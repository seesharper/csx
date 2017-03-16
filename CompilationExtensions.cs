using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Linq;
namespace csx
{
    public static class CompilationExtensions
    {
        public static IEnumerable<ReferencedAssembly> GetReferencedAssemblies(this Compilation compilation)
        {
            // https://github.com/dotnet/roslyn/blob/version-2.0.0-beta4/src/Compilers/Core/Portable/Compilation/Compilation.cs#L478
            var referenceManager = compilation.Invoke<object>("GetBoundReferenceManager", BindingFlags.NonPublic);
                                                
            var referencedAssemblies =
                            // https://github.com/dotnet/roslyn/blob/version-2.0.0-beta4/src/Compilers/Core/Portable/ReferenceManager/CommonReferenceManager.State.cs#L34
                            referenceManager.Invoke<IEnumerable<KeyValuePair<MetadataReference, IAssemblySymbol>>>("GetReferencedAssemblies", BindingFlags.NonPublic);
            foreach(var kvp in referencedAssemblies)
            {
                var path = (kvp.Key as PortableExecutableReference)?.FilePath;
                if (path != null)
                {
                    yield return new ReferencedAssembly(path, kvp.Value.Identity);                    
                }
            }
        }
    }
}
public class ReferencedAssembly 
{
    public ReferencedAssembly(string path, AssemblyIdentity assemblyIdentity)
    {
        Path = path;
        Identity = assemblyIdentity;
    }
    public string Path {get; private set;}
    public AssemblyIdentity Identity {get; private set;}
}