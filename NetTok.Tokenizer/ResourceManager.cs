using System;
using System.IO;
using System.Linq;
using NetTok.Tokenizer.Utilities;

namespace NetTok.Tokenizer
{
    public static class ResourceManager
    {
        /// <summary>
        ///     Retrieves an embedded resource file.
        /// </summary>
        /// <param name="fileName">The file name of the resource being retrieved.</param>
        /// <returns>The contents of the specified resource file as a Stream.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the resource file name was not supplied or null.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the specified resource file does not exist.
        /// </exception>
        public static Stream Read(string fileName)
        {
            Guard.NotNull(fileName);
            var currentDomain = AppDomain.CurrentDomain;
            var assemblies = currentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("NetTok"))
                {
                    continue;
                }
                var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(fileName));
                if (resourcePath != null)
                {
                    return assembly.GetManifestResourceStream(resourcePath);
                }
            }

            throw new FileNotFoundException($"The resource file: {fileName} was not found.");
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
        public static Stream Read(string language, string fileName)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                language = "default";
            }

            Guard.NotNull(fileName);
            var fullFileName = $"{language}_{fileName}";
            var currentDomain = AppDomain.CurrentDomain;
            var assemblies = currentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.StartsWith("NetTok"))
                {
                    continue;
                }
                var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(fullFileName));
                if (resourcePath != null)
                {
                    return assembly.GetManifestResourceStream(resourcePath);
                }
            }

            // if we've gotten here then there isn't a resource for the specified language.
            // revert to the default resources
            fullFileName = $"default_{fileName}";
            foreach (var assembly in assemblies)
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }
                var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(str => str.EndsWith(fullFileName));
                if (resourcePath != null)
                {
                    return assembly.GetManifestResourceStream(resourcePath);
                }
            }

            throw new FileNotFoundException($"The resource file: {fileName} was not found.");
        }
    }
}