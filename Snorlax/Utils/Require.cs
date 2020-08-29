using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Shared.App.Utils
{
    static class Require
    {
        [DebuggerStepThrough]
        public static void True(bool condition, string? message = null)
        {
            if (!condition)
            {
                throw new ArgumentException(message ?? "Assertion failed");
            }
        }

        [DebuggerStepThrough]
        public static T NonNull<T>(T? value, string? paramName = null) where T : class
        {
            if (value == null)
            {
                throw new ArgumentException($"{paramName ?? "Argument"} required");
            }
            return value;
        }

        [DebuggerStepThrough]
        public static T NonNull<T>(T? value, string? paramName = null) where T : struct
        {
            if (value == null)
            {
                throw new ArgumentException($"{paramName ?? "Argument"} required");
            }
            return value.Value;
        }

        [DebuggerStepThrough]
        public static string NonNullNonEmpty(string? value, string? paramName = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{paramName ?? "Argument"} required");
            }
            return value;
        }

        [DebuggerStepThrough]
        public static List<T> NonNullNonEmpty<T>(List<T>? value, string? paramName = null)
        {
            if (value == null || value.Count == 0)
            {
                throw new ArgumentException($"{paramName ?? "Argument"} required");
            }
            return value;
        }

        [DebuggerStepThrough]
        public static int NonZero(int value, string? paramName = null)
        {
            if (value == 0)
            {
                throw new ArgumentException($"{paramName ?? "Argument"} required");
            }
            return value;
        }

        [DebuggerStepThrough]
        public static int NonNullNonZero(int? value, string? paramName = null)
        {
            if (value == null || value == 0)
            {
                throw new ArgumentException($"{paramName ?? "Argument"} required");
            }
            return value.Value;
        }

        [DebuggerStepThrough]
        public static void MonitorHeld(object? monitor)
        {
            if (!Monitor.IsEntered(NonNull(monitor)))
            {
                throw new SynchronizationLockException();
            }
        }
    }
}
