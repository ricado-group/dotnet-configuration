using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace RICADO.Configuration
{
    public static class ConfigurationProvider
    {
        #region Private Properties

        private static IConfigurationRoot _configuration;

        #endregion


        #region Public Properties

        public static IConfigurationRoot Configuration
        {
            get
            {
                return _configuration;
            }
        }

        #endregion


        #region Public Methods

        public static void Initialize(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public static void Destroy()
        {
        }

        /// <summary>
        /// Attempts to Select a Value based on the <paramref name="path"/> provided
        /// </summary>
        /// <example>
        /// SelectValue<bool>("Section1.SubSection1.Enabled", false);
        /// </example>
        /// <typeparam name="T">The Value Type</typeparam>
        /// <param name="path">A Path String using dot seperators (e.g. object.subobject.key)</param>
        /// <param name="defaultValue">The Default Value to Return if the Key did not Exist</param>
        /// <returns>The Requested Value from the Configuration</returns>
        public static T SelectValue<T>(string path, T defaultValue)
        {
            T value;
            
            if(TrySelectValue<T>(path, defaultValue, out value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Attempts to Select a Value based on the <paramref name="path"/> provided
        /// </summary>
        /// <example>
        /// bool enabled;
        /// 
        /// if(TrySelectValue<bool>("Section1.SubSection1.Enabled", false, out enabled))
        /// {
        ///     Console.WriteLine("Is Enabled: {0}", enabled);
        /// }
        /// </example>
        /// <typeparam name="T">The Value Type</typeparam>
        /// <param name="path">A Path String using dot seperators (e.g. object.subobject.key)</param>
        /// <param name="defaultValue">The Default Value to Return if the Key did not Exist</param>
        /// <param name="value">The Requestd Value from the Configuration</param>
        /// <returns>Whether the Value was found and valid</returns>
        public static bool TrySelectValue<T>(string path, T defaultValue, out T value)
        {
            value = defaultValue;

            if (path == null || path.Length == 0)
            {
                return false;
            }

            if (_configuration == null)
            {
                return false;
            }

            if (path.Contains('.') == false)
            {
                T topLevelValue = _configuration.GetValue<T>(path);
                
                if(topLevelValue == null)
                {
                    return false;
                }

                value = topLevelValue;

                return true;
            }

            Queue<string> pathSegments = new Queue<string>(path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            IConfigurationSection section = null;

            while (pathSegments.Count > 1)
            {
                string sectionKey = pathSegments.Dequeue();

                if (section == null)
                {
                    section = _configuration.GetSection(sectionKey);
                }
                else
                {
                    section = section.GetSection(sectionKey);
                }
            }

            if (section == null || pathSegments.Count != 1)
            {
                return false;
            }

            string valueKey = pathSegments.Dequeue();

            T sectionValue = section.GetValue<T>(valueKey);

            if(sectionValue == null)
            {
                return false;
            }

            value = sectionValue;

            return true;
        }

        /// <summary>
        /// Attempts to Populate Properties of an Object based on the <paramref name="path"/> provided
        /// </summary>
        /// <example>
        /// public record MySetting
        /// {
        ///     public Enabled = false;
        ///     public MyString = "default";
        /// }
        /// 
        /// MySetting setting = PopulateObject<MySetting>("FirstObject.MySetting");
        /// </example>
        /// <typeparam name="T">The Object Type (e.g. a class or record)</typeparam>
        /// <param name="path">A Path String using dot seperators (e.g. object.subobject.key)</param>
        /// <returns>A New Instance of the Object with Properties Populated</returns>
        public static T PopulateObject<T>(string path) where T: new()
        {
            T instance;

            if(TryPopulateObject<T>(path, out instance))
            {
                return instance;
            }

            return new T();
        }

        /// <summary>
        /// Attempts to Populate Properties of an Object based on the <paramref name="path"/> provided
        /// </summary>
        /// <example>
        /// public record MySetting
        /// {
        ///     public Enabled = false;
        ///     public MyString = "default";
        /// }
        /// 
        /// MySetting setting;
        /// 
        /// if(TryPopulateObject<MySetting>("FirstObject.MySetting", out setting))
        /// {
        ///     Console.WriteLine("Is Enabled: {0}, My String: {1}", setting.Enabled, setting.MyString);
        /// }
        /// </example>
        /// <typeparam name="T">The Object Type (e.g. a class or record)</typeparam>
        /// <param name="path">A Path String using dot seperators (e.g. object.subobject.key)</param>
        /// <param name="instance">A New Instance of the Object with Properties Populated</param>
        /// <returns>Whether the Configuration Section was found and valid</returns>
        public static bool TryPopulateObject<T>(string path, out T instance) where T : new()
        {
            instance = new T();

            if (path == null || path.Length == 0)
            {
                return false;
            }

            if (_configuration == null)
            {
                return false;
            }

            if (path.Contains('.') == false)
            {
                if(_configuration.GetSection(path).Exists() == false)
                {
                    return false;
                }

                _configuration.Bind(path, instance);

                return true;
            }

            Queue<string> pathSegments = new Queue<string>(path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            IConfigurationSection section = null;

            while (pathSegments.Count > 1)
            {
                string sectionKey = pathSegments.Dequeue();

                if (section == null)
                {
                    section = _configuration.GetSection(sectionKey);
                }
                else
                {
                    section = section.GetSection(sectionKey);
                }
            }

            if (section == null || pathSegments.Count != 1 || section.Exists() == false)
            {
                return false;
            }

            string objectKey = pathSegments.Dequeue();

            if(section.GetSection(objectKey).Exists() == false)
            {
                return false;
            }

            section.Bind(objectKey, instance);

            return true;
        }

        #endregion
    }
}
