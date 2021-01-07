using System.IO;
using System.Linq;
using System.Reflection;

namespace NetTok.Tokenizer
{
    public static class ResourceMethods
    {
        /// <summary>
        ///     Retrieves an embedded resource file.
        /// </summary>
        /// <param name="fileName">The file name (and extension) of the resource being retrieved.</param>
        /// <remarks>
        ///     Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        /// </remarks>
        /// <returns>The text contents of the specified file, or an empty string if there are no contents.</returns>
        public static Stream ReadResource(string fileName)
        {
            Guard.NotNull(fileName);
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(fileName));
            if (resourcePath == null)
            {
                throw new FileNotFoundException($"The resource file: {fileName} was not found.");
            }

            return assembly.GetManifestResourceStream(resourcePath);
        }

        /// <summary>
        ///     Retrieves an embedded resource file.
        /// </summary>
        /// <param name="language">The two letter abbreviation of the language resource being retrieved.</param>
        /// <param name="fileName">The file name (and extension) of the language resource being retrieved.</param>
        /// <remarks>
        ///     Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        ///     If there are no resource files for the specified language, the method will return the default information.
        /// </remarks>
        /// <returns>The text contents of the specified file.</returns>
        public static Stream ReadResource(string language, string fileName)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                language = "default";
            }

            Guard.NotNull(fileName);
            var fullFileName = $"{language}_{fileName}";
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(fullFileName));
            if (resourcePath == null)
            {
                fullFileName = $"default_{fileName}";
                resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(fullFileName));
                if (resourcePath == null)
                {
                    throw new FileNotFoundException($"The resource file: {fullFileName} was not found.");
                }
            }

            return assembly.GetManifestResourceStream(resourcePath);
        }
    }
}