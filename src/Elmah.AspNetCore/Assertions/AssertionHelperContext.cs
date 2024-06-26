﻿using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Elmah.AspNetCore.Assertions;

public sealed class AssertionHelperContext
{
    private Exception? _baseException;
    private int _httpStatusCode;
    private bool _statusCodeInitialized;

    public AssertionHelperContext(Exception e, HttpContext context) :
        this(null, e, context)
    {
    }

    public AssertionHelperContext(object? source, Exception e, HttpContext context)
    {
        Debug.Assert(e != null);

        FilterSource = source ?? this;
        Exception = e;
        Context = context;
    }

    public object FilterSource { get; }

    public Type FilterSourceType => FilterSource.GetType();

    public AssemblyName FilterSourceAssemblyName => FilterSourceType.Assembly.GetName();

    public Exception Exception { get; }

    public Exception BaseException => _baseException ??= Exception.GetBaseException();

    public bool HasHttpStatusCode => HttpStatusCode != 0;

    public int HttpStatusCode
    {
        get
        {
            if (_statusCodeInitialized)
            {
                return _httpStatusCode;
            }

            _statusCodeInitialized = true;

            if (Exception is HttpRequestException { StatusCode: not null } exception)
            {
                _httpStatusCode = (int)exception.StatusCode;
            }

            return _httpStatusCode;
        }
    }

    public HttpContext Context { get; }
}