﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using Microsoft.Diagnostics.Runtime.Implementation;

namespace Microsoft.Diagnostics.Runtime
{
    /// <summary>
    /// A wrapper for exception objects which help with common tasks for exception objects.
    /// Create this using <see cref="ClrObject.AsException"/>. You may call that when <see cref="ClrObject.IsException"/>
    /// is <see langword="true"/>.
    /// </summary>
    public struct ClrException
    {
        private readonly IExceptionHelpers _helpers;
        private readonly ClrObject _object;

        /// <summary>
        /// Gets the original thread this exception was thrown from.  This may be <see langword="null"/> if we do not know.
        /// </summary>
        public ClrThread? Thread { get; }

        public ClrException(IExceptionHelpers helpers, ClrThread? thread, ClrObject obj)
        {
            if (obj.IsNull)
                throw new InvalidOperationException($"Cannot construct a ClrException from a null object.");

            _helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));
            _object = obj;
            Thread = thread;

            DebugOnly.Assert(obj.IsException);
        }

        /// <summary>
        /// Gets the address of the exception object.
        /// </summary>
        public ulong Address => _object;

        /// <summary>
        /// Gets the <see cref="ClrType"/> for this exception object.
        /// </summary>
        public ClrType Type => _object.Type!;

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        public string? Message => _object.GetStringField("_message");

        /// <summary>
        /// Gets the inner exception, if one exists, <see langword="null"/> otherwise.
        /// </summary>
        public ClrException? Inner
        {
            get
            {
                ClrObject obj = _object.GetObjectField("_innerException");
                if (obj.IsNull)
                    return null;

                return obj.AsException();
            }
        }

        /// <summary>
        /// Gets the HRESULT associated with this exception (or S_OK if there isn't one).
        /// </summary>
        public int HResult => _object.GetField<int>("_HResult");

        /// <summary>
        /// Gets the StackTrace for this exception.  Note that this may be empty or partial depending
        /// on the state of the exception in the process.  (It may have never been thrown or we may be in
        /// the middle of constructing the stackwalk.)  This returns an empty list if no stack trace is
        /// associated with this exception object.
        /// </summary>
        public ImmutableArray<ClrStackFrame> StackTrace => _helpers.GetExceptionStackTrace(Thread, _object);
    }
}