﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Nethermind.Core
{
    public static class ClientVersion
    {
        static ClientVersion()
        {
            string osDescription = RuntimeInformation.OSDescription;
            if (osDescription.Contains('#'))
            {
                int indexOfHash = osDescription.IndexOf('#');
                osDescription = osDescription.Substring(0, Math.Max(0, indexOfHash - 1));
            }

            Assembly assembly = typeof(ClientVersion).Assembly;
            AssemblyInformationalVersionAttribute versionAttribute = assembly.GetCustomAttributes(false).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            Version = versionAttribute?.InformationalVersion;
            Description = $"Nethermind/{Version}/{RuntimeInformation.OSArchitecture}-{osDescription}/{RuntimeInformation.FrameworkDescription.Trim().Replace(".NET ", "").Replace(" ", "")}";
        }
        
        public static string Version { get; }
        
        public static string Description { get; }
    }
}