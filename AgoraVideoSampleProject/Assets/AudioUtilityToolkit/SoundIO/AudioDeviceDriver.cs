// This code is based on AudioSystem and InputDeviceList from Lasp.
// https://github.com/keijiro/Lasp/blob/v2/Packages/jp.keijiro.lasp/Runtime/AudioSystem.cs
// https://github.com/keijiro/Lasp/blob/v2/Packages/jp.keijiro.lasp/Runtime/Internal/InputDeviceList.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.LowLevel;

namespace AudioUtilityToolkit.SoundIOExtension
{
    public static class AudioDeviceDriver
    {
        #region Public members

        public static List<InputStream> InputDeviceList => GetInputDeviceList();

        public static InputStream GetInputDevice(string name)
            => InputDeviceList.FirstOrDefault(device => name == device.DeviceName);

        #endregion

        #region Device list management

        static List<InputStream> _inputStreams = new List<InputStream>();

        static List<InputStream> GetInputDeviceList()
        {
            Context.FlushEvents();

            var deviceCount = Context.InputDeviceCount;
            var defaultIndex = Context.DefaultInputDeviceIndex;

            var founds = new List<InputStream>();

            for (var i = 0; i < deviceCount; i++)
            {
                var dev = Context.GetInputDevice(i);

                // Check if the device is useful. Reject it if not.
                if (dev.IsRaw || dev.Layouts.Length < 1)
                {
                    dev.Dispose();
                    continue;
                }

                // Find the same device in the current list.
                var handle = _inputStreams.FindAndRemove(h => h.DeviceID == dev.ID);

                if (handle != null)
                {
                    // We reuse the handle, so this libsoundio device object
                    // should be disposed.
                    dev.Dispose();
                }
                else
                {
                    // Create a new handle with transferring the ownership of
                    // this libsoundio device object.
                    handle = new InputStream(dev);
                }

                // Default device: Insert it at the head of the list.
                // Others: Simply append it to the list.
                if (i == defaultIndex)
                    founds.Insert(0, handle);
                else
                    founds.Add(handle);
            }

            // Dispose the remained handles (disconnected devices).
            foreach (var dev in _inputStreams) dev.Dispose();

            // Replace the list with the new one.
            _inputStreams = founds;

            return _inputStreams;
        }
        
        #endregion

        #region libsoundio context management

        static SoundIO.Context Context => GetContextWithLazyInitialization();
        static SoundIO.Context _context;

        static SoundIO.Context GetContextWithLazyInitialization()
        {
            if (_context == null)
            {
                // libsoundio context initialization
                _context = SoundIO.Context.Create();
                _context.Connect();
                _context.FlushEvents();

                // Install the Player Loop System.
                InsertPlayerLoopSystem();

                // Install the "on-exit" callback.
            #if UNITY_EDITOR
                UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnExit;
                UnityEditor.EditorApplication.quitting += OnExit;
            #else
                UnityEngine.Application.quitting += OnExit;
            #endif
            }

            return _context;
        }

        static void OnExit()
        {
            foreach (var s in _inputStreams) s.Dispose();
            _inputStreams = null;

            _context?.Dispose();
            _context = null;
        }

        #endregion

        #region Update method implementation

        static void Update()
        {
            Context.FlushEvents();

            foreach (var stream in _inputStreams)
            {
                if (stream.IsValid) stream.Update();
            }
        }

        #endregion

        #region PlayerLoopSystem implementation

        static void InsertPlayerLoopSystem()
        {
            // Append a custom system to the Early Update phase.

            var customSystem = new PlayerLoopSystem()
            {
                type = typeof(AudioDeviceDriver),
                updateDelegate = () => AudioDeviceDriver.Update()
            };

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            for (var i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                ref var phase = ref playerLoop.subSystemList[i];
                if (phase.type == typeof(UnityEngine.PlayerLoop.EarlyUpdate))
                {
                    phase.subSystemList = phase.subSystemList.
                        Concat(new[]{ customSystem }).ToArray();
                    break;
                }
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        #endregion
    }

    // Extension methods for List<T>
    static class ListExtensions
    {
        // Find and retrieve an entry with removing it
        public static T FindAndRemove<T>(this List<T> list, Predicate<T> match)
        {
            var index = list.FindIndex(match);
            if (index < 0) return default(T);
            var res = list[index];
            list.RemoveAt(index);
            return res;
        }
    }
}