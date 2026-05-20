using System;
using UnityEngine;

namespace _Project.Code.Core.Services
{
    public interface IService : IDisposable
    {
        /// <summary>
        /// Called during two-phase initialization after all services are registered.
        /// Safe to reference other services here.
        /// </summary>
        Awaitable InitializeAsync();
    }
}