﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EcsCore
{
    /// <summary>
    /// Encapsulate creation of modules and let take them by type
    /// Only for internal usage
    /// </summary>
    internal class EcsModulesRepository
    {
        private readonly Dictionary<Type, EcsModule> _globalModules;
        private readonly Dictionary<Type, EcsModule> _localModules;

        internal IReadOnlyDictionary<Type, EcsModule> GlobalModules => _globalModules;
        internal IReadOnlyDictionary<Type, EcsModule> LocalModules => _localModules;

        internal EcsModulesRepository()
        {
            var allModules = CreateAllEcsModules().ToArray();
            for (var i = 0; i < allModules.Length; ++i)
                allModules[i].InjectRepository(this);

            _globalModules = allModules.Where(m => m.IsGlobal).ToDictionary(m => m.GetType(), m => m);
            _localModules = allModules.Where(m => !m.IsGlobal).ToDictionary(m => m.GetType(), m => m);
        }

        /// <summary>
        /// Return global module by type
        /// </summary>
        /// <returns>Global module or null</returns>
        internal T GetGlobalModule<T>() where T : EcsModule
        {
            return GetModule<T>(_globalModules);
        }

        private static T GetModule<T>(IReadOnlyDictionary<Type, EcsModule> modules) where T : EcsModule
        {
            var type = typeof(T);
            if (modules.ContainsKey(type))
                return modules[type] as T;
            return null;
        }

        private static IEnumerable<EcsModule> CreateAllEcsModules()
        {
            return Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(t => t.IsSubclassOf(typeof(EcsModule)) && !t.IsAbstract)
                           .Select(t => (EcsModule) Activator.CreateInstance(t));
        }
    }
}