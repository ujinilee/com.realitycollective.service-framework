﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealityCollective.ServiceFramework.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, Type> ServiceInterfaceCache = new Dictionary<Type, Type>();

        internal static Type FindServiceInterfaceType(this Type serviceType, Type interfaceType)
        {
            if (serviceType == null)
            {
                return null;
            }

            var returnType = interfaceType;

            if (typeof(IService).IsAssignableFrom(serviceType))
            {
                if (!ServiceInterfaceCache.TryGetValue(serviceType, out returnType))
                {
                    if (IsValidServiceType(interfaceType, out returnType))
                    {
                        // If the interface we pass in is a Valid Service Type, cache it and move on.
                        ServiceInterfaceCache.Add(serviceType, returnType);
                        return returnType;
                    }

                    var allInterfaces = new HashSet<Type>(serviceType.GetInterfaces());
                    if (serviceType.BaseType != null)
                    {
                        // Remove all the interfaces implemented by the base class, so that now only
                        // interfaces implemented by the most derived class and interfaces implemented by those
                        // (interfaces of the most derived class) remain.
                        allInterfaces.ExceptWith(serviceType.BaseType.GetInterfaces());
                    }

                    // We want to remove interfaces that are implemented by other interfaces
                    // i.e
                    // public interface A : B {}
                    // public interface B {}
                    // public class Top : A {} → We only want to dump interface A so interface B must be removed

                    // Considering class A given above allInterfaces contains A and B now.
                    var toRemove = new HashSet<Type>();
                    foreach (var implementedByMostDerivedClass in allInterfaces)
                    {
                        // For interface A this will only contain single element, namely B
                        // For interface B this will an empty array
                        foreach (var implementedByOtherInterfaces in implementedByMostDerivedClass.GetInterfaces())
                        {
                            toRemove.Add(implementedByOtherInterfaces);
                        }
                    }

                    // Finally remove the interfaces that do not belong to the most derived class.
                    allInterfaces.ExceptWith(toRemove);

                    foreach (var typeInterface in allInterfaces)
                    {
                        if (IsValidServiceType(typeInterface, out returnType))
                        {
                            break;
                        }
                    }

                    ServiceInterfaceCache.Add(serviceType, returnType);
                }
            }

            return returnType;
        }

        /// <summary>
        /// Checks if the <see cref="IService"/> has any valid implementations.
        /// </summary>
        /// <typeparam name="T">The specific <see cref="IService"/> interface to check.</typeparam>
        /// <returns>True, if the project contains valid implementations of <typeparamref name="T"/>.</returns>
        internal static bool HasValidImplementations<T>() where T : IService
        {
            var concreteTypes = TypeCache.Current
                .Select(pair => pair.Value)
                .Where(type => typeof(T).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            var isValid = concreteTypes.Any();

            if (!isValid)
            {
                UnityEngine.Debug.LogError($"Failed to find valid implementations of {typeof(T).Name}");
            }

            return isValid;
        }

        private static bool IsValidServiceType(Type inputType, out Type returnType)
        {
            returnType = null;

            if (!typeof(IService).IsAssignableFrom(inputType))
            {
                return false;
            }

            if (!ServiceManager.ServiceInterfaceTypes.Contains(inputType))
            {
                returnType = inputType;
                return true;
            }
            return false;
        }
    }
}